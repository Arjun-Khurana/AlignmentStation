using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using AlignmentStation.Models;
using System.Threading;
using System.Threading.Tasks;

namespace AlignmentStation
{
    /// <summary>
    /// Interaction logic for Step2.xaml
    /// </summary>
    public partial class Step2 : Page
    {
        List<string> ErrorMessages = new();
        int attemptNumber = 0;
        bool barrelReplaced = false;
        bool firstLightFail = false;
        double tosaFirstLightThreshold = 0.01;
        double rosaFirstLightThreshold = 0.02;
        CancellationTokenSource tokenSource2 = new CancellationTokenSource();

        public Step2()
        {
            InitializeComponent();
        }

        void OnLoad(object sender, RoutedEventArgs e)
        {
            var w = MainWindow.GetWindow(this) as MainWindow;

            if (w.device is ROSADevice)
            {
                fifthInstruction.Visibility = Visibility.Collapsed;
            }
        }

        private void Next_Step_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Step3());
        }

        private async void AlignmentButtonClick(object sender, RoutedEventArgs e)
        {
            if (barrelReplaced)
            {
                // complete fail go back to home
                NavigationService.Navigate(new HomePage());
                // output device so far with failure
                return;
            }
            else if (attemptNumber == 3)
            {
                // change instructions to replace barrel
                barrelReplaced = true;
            }


            Mouse.OverrideCursor = Cursors.Wait;

            ErrorMessages.Clear();

            var w = Window.GetWindow(this) as MainWindow;
            if (w.device is TOSADevice)
            {
                // TODO: query reference position for this device,
                // move aerotech there if it exists.

                await TosaStep2();
            }
            else
            {
                // TODO: query reference position for this device,
                // move aerotech there if it exists.

                RosaStep2();
            }

            Mouse.OverrideCursor = Cursors.Arrow;
        }

        private void stepSuccessUiUpdate()
        {
            errorPanel.Visibility = Visibility.Collapsed;
            failedMessage.Visibility = Visibility.Collapsed;
            successMessage.Visibility = Visibility.Visible;

            NextStepButton.Visibility = Visibility.Visible;
            AlignmentButton.Visibility = Visibility.Collapsed;
        }

        private void stepErrorUiUpdate()
        {
            var w = Window.GetWindow(this) as MainWindow;
            var output = w.output;

            errorList.ItemsSource = ErrorMessages;
            errorPanel.Visibility = Visibility.Visible;
            failedMessage.Visibility = Visibility.Visible;
            failedMessage.Text = $"Test attempt {attemptNumber} failed, check TO and lens";
            successMessage.Visibility = Visibility.Collapsed;
            NextStepButton.Visibility = Visibility.Collapsed;

            if (attemptNumber > 3)
            {
                if (output is ROSAOutput)
                {

                    MainWindow.Conn.SaveROSAOutput(output as ROSAOutput);
                }
                else
                {
                    MainWindow.Conn.SaveTOSAOutput(output as TOSAOutput);
                }
                endJobButton.Visibility = Visibility.Visible;
                nextDeviceButton.Visibility = Visibility.Visible;
            }
            else
            {
                if (attemptNumber == 3)
                {
                    firstInstruction.Text = "(1) Remove lens barrel";
                    this.failedMessage.Text = $"Test attempt {attemptNumber} failed\nReplace TO and try again.";
                    secondInstruction.Visibility = Visibility.Collapsed;
                    thirdInstruction.Visibility = Visibility.Collapsed;
                }

                this.AlignmentButton.Content = "Retry alignment";
            }
        }

        private void RosaStep2()
        {
            var w = Window.GetWindow(this) as MainWindow;
            var rosa = w.device as ROSADevice;
            var output = w.output as ROSAOutput;

            // get ref units and go there if they exist
            var refUnits = MainWindow.Conn.GetROSAReferenceUnits(output);
            if (refUnits != null)
            {
                Instruments.instance.SetAerotechPosition(refUnits.X, refUnits.Y, refUnits.Z);
            }


            var voltage = Instruments.instance.GetAerotechAnalogVoltage();

            firstLightInfoPanel.Visibility = Visibility.Visible;
            firstLightVoltage.Text = $"Input voltage: {voltage}";

            if (voltage < rosaFirstLightThreshold)
            {
                Debug.Print("Voltage is too low: {0}", voltage);
                Debug.Print("Finding first light");
                Instruments.instance.FindFirstLight();
                voltage = Instruments.instance.GetAerotechAnalogVoltage();
            }

            firstLightVoltage.Text = $"Input voltage: {voltage}";

            if (voltage < rosaFirstLightThreshold)
            {
                Debug.Print("Input voltage: {0}", voltage);
                Debug.Print("Could not find first light");

                ErrorMessages.Clear();
                ErrorMessages.Add("Could not find first light.");
                errorList.ItemsSource = ErrorMessages;
                errorPanel.Visibility = Visibility.Visible;
                AlignmentButton.Visibility = Visibility.Visible;
                endJobButton.Visibility = Visibility.Visible;

                AlignmentButton.Visibility = Visibility.Collapsed;
                nextDeviceButton.Visibility = Visibility.Visible;
                firstLightFail = true;

                retryPanel.Visibility = Visibility.Visible;

                return;
            }

            attemptNumber++;
            Instruments.instance.FindCentroid(voltage * 0.90, 0.00025 * 5);

            var voltageAfterAlignment = Instruments.instance.GetAerotechAnalogVoltage();
            firstLightVoltage.Text = $"Voltage after alignment: {voltageAfterAlignment}";

            var currentAfterAlignment = (voltageAfterAlignment * 0.596) -0.006;
            var responsivityAfterAlignment = currentAfterAlignment / output.Fiber_Power;

            if (responsivityAfterAlignment < rosa.Resp_Min) // get min responsivity from device
            {
                ErrorMessages.Add($"Responsivity is too low: {responsivityAfterAlignment}");
            }

            if (ErrorMessages.Count == 0) 
            {
                output.Resp = responsivityAfterAlignment;
                stepSuccessUiUpdate();
            }
            else
            {
                stepErrorUiUpdate();
            }
        }

        private async Task TosaStep2()
        {
            var w = Window.GetWindow(this) as MainWindow;
            var o = w.output as TOSAOutput;
            var d = w.device as TOSADevice;

            var refUnits = MainWindow.Conn.GetTOSAReferenceUnits(o);
            if (refUnits != null)
            {
                Instruments.instance.SetAerotechPosition(refUnits.X, refUnits.Y, refUnits.Z);
            }
            
            var firstLightPower = Instruments.instance.GetThorlabsPower();

            double pFC = 0;
            double popCt = 0;
            int iterCount = 0;

            pFC = firstLightPower / Instruments.instance.alignmentPowerCalibration;
            popCt = pFC / o.P_TO;

            firstLightVoltage.Visibility = Visibility.Visible;
            firstLightVoltage.Text = $"Fiber Coupled Power: {pFC}";

            if (firstLightPower < tosaFirstLightThreshold)
            {
                Debug.Print("Power is too low: {0}", firstLightPower);
                Debug.Print("Finding first light");
                Instruments.instance.FindFirstLight();
            }

            firstLightPower = Instruments.instance.GetThorlabsPower();
            firstLightVoltage.Text = $"Input voltage: {firstLightPower}";

            if (firstLightPower < tosaFirstLightThreshold)
            {
                Debug.Print("Input voltage: {0}", firstLightPower);
                Debug.Print("Could not find first light");

                ErrorMessages.Clear();
                ErrorMessages.Add("Could not find first light.");
                errorList.ItemsSource = ErrorMessages;
                errorPanel.Visibility = Visibility.Visible;
                AlignmentButton.Visibility = Visibility.Collapsed;

                endJobButton.Visibility = Visibility.Visible;
                nextDeviceButton.Visibility = Visibility.Visible;
                firstLightFail = true;

                retryPanel.Visibility = Visibility.Visible;

                return;
            }

            ErrorMessages.Clear();

            firstLightVoltage.Text = $"Initial popCT: {String.Format("{0:0.00}", popCt * 100)}%";

            if (popCt < d.POPCT_Min)
            {
                attemptNumber++;

                CancellationToken ct = tokenSource2.Token;

                AlignmentButton.Visibility = Visibility.Collapsed;
                progressPanel.Visibility = Visibility.Visible;

                var task = Task.Run(() =>
                {
                    ct.ThrowIfCancellationRequested();

                    while (iterCount < 3 && popCt < 0.72)
                    {
                        firstLightPower = Instruments.instance.GetThorlabsPower();
                        Debug.Print($"Input voltage: {firstLightPower}");

                        Instruments.instance.FindCentroid(firstLightPower * 0.85, 0.00025);

                        var powerAfterAlignment = Instruments.instance.GetThorlabsPower();
                        Debug.Print("Power after alignment: {0}", powerAfterAlignment);

                        pFC = powerAfterAlignment / Instruments.instance.alignmentPowerCalibration;
                        popCt = pFC / o.P_TO;

                        Debug.Print($"Iteration {iterCount} popCT: {popCt}");
                        iterCount++;

                        if (ct.IsCancellationRequested)
                        {
                            ct.ThrowIfCancellationRequested();
                        }
                    }
                }, tokenSource2.Token);

                try
                {
                    await task;
                }
                catch(OperationCanceledException e)
                {
                    Debug.Print($"{nameof(OperationCanceledException)} thrown with message: {e.Message}");
                }

                progressPanel.Visibility = Visibility.Collapsed;
                AlignmentButton.Visibility = Visibility.Visible;

                o.P_FC = pFC;
                o.POPCT = popCt;

                if (o.POPCT < d.POPCT_Min)
                {
                    ErrorMessages.Add("POPCT < POPCT_MIN");
                    failedMessage.Visibility = Visibility.Visible;
                    successMessage.Visibility = Visibility.Collapsed;
                }
            }

            if (ErrorMessages.Count == 0) 
            {
                stepSuccessUiUpdate();
            }
            else
            {
                stepErrorUiUpdate();
            }

            firstLightVoltage.Text = $"Final popCT = {String.Format("{0:0.00}", getTosaPopCT() * 100)}%";
        }

        private void Next_Device_Click(object sender, RoutedEventArgs e)
        {
            var w = Window.GetWindow(this) as MainWindow;
            var currentOutput = w.output;
            var job = currentOutput.Job_Number;
            var unitNumber = currentOutput.Unit_Number;
            var op = currentOutput.Operator;

            if (w.device is TOSADevice)
            {
                w.output = new TOSAOutput
                {
                    Part_Number = w.device.Part_Number,
                    Passed = false,
                    Job_Number = job,
                    Operator = op,
                    Unit_Number = unitNumber + 1
                };
            }
            else
            {
                w.output = new ROSAOutput
                {
                    Part_Number = w.device.Part_Number,
                    Passed = false,
            //TODO: Query aerotech position and save reference position
                    Job_Number = job,
                    Operator = op,
                    Unit_Number = unitNumber + 1
                };
            }

            NavigationService.Navigate(new Step1());
        }

        private void Quit_Button_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult messageBoxResult = 
                MessageBox.Show("Are you sure?", "Quit Confirmation", MessageBoxButton.YesNo);

            if (messageBoxResult == MessageBoxResult.Yes)
            {
                NavigationService.Navigate(new HomePage());
            }

        }

        private void retryFirstLightButton_Click(object sender, RoutedEventArgs e)
        {
            var w = Window.GetWindow(this) as MainWindow;
            //TODO: Query aerotech position and save reference position
            var d = w.device;

            var pow = 0.0;
            var found = false;

            if (d is TOSADevice)
            {
                pow = Instruments.instance.GetThorlabsPower();
                found = displayFirstLightErrors(pow, tosaFirstLightThreshold);
            }
            else
            {
                pow = Instruments.instance.GetAerotechAnalogVoltage();
                found = displayFirstLightErrors(pow, rosaFirstLightThreshold);
            }

            if (found) return;

            Mouse.OverrideCursor = Cursors.Wait;
            firstLightVoltage.Text = "Finding first light";

            Instruments.instance.FindFirstLight(false);
            Mouse.OverrideCursor = Cursors.Arrow;

            //TODO: Query aerotech position and save reference position
            if (d is TOSADevice)
            {
                var firstLightPower = Instruments.instance.GetThorlabsPower();
                displayFirstLightErrors(firstLightPower, tosaFirstLightThreshold);
            }
            else
            {
                var voltage = Instruments.instance.GetAerotechAnalogVoltage();
                displayFirstLightErrors(voltage, rosaFirstLightThreshold);
            }
        }

        private double getTosaPopCT()
        {
            var w = Window.GetWindow(this) as MainWindow;
            var output = w.output as TOSAOutput;

             
            double pow = Instruments.instance.GetThorlabsPower();
            double pFC = pow / Instruments.instance.alignmentPowerCalibration;

            return pFC / output.P_TO;
        }

        private bool displayFirstLightErrors(double voltage, double threshold)
        {
            firstLightVoltage.Text = $"Input voltage: {voltage}";

            if (voltage < threshold)
            {
                errorList.Visibility = Visibility.Collapsed;
                ErrorMessages.Clear();
                ErrorMessages.Add("Retry first light failed.");
                errorList.ItemsSource = ErrorMessages;
                errorList.Visibility = Visibility.Visible;

                return false;
            }
            else
            {
                AlignmentButton.Visibility = Visibility.Visible;
                firstLightVoltage.Text = $"Found first light power: {voltage}";
                retryPanel.Visibility = Visibility.Collapsed;
                nextDeviceButton.Visibility = Visibility.Collapsed;
                endJobButton.Visibility = Visibility.Collapsed;
                errorPanel.Visibility = Visibility.Collapsed;

                return true;
            }
        }

        private void endJobButton_Click(object sender, RoutedEventArgs e)
        {
            attemptNumber = 3;
            barrelReplaced = true;

            NavigationService.Navigate(new HomePage());
        }

        private void cancelAlignmentButton_Click(object sender, RoutedEventArgs e)
        {
            Instruments.instance.AerotechAbort();
            tokenSource2.Cancel();
        }
    }
}
