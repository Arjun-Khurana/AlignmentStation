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

namespace AlignmentStation
{
    /// <summary>
    /// Interaction logic for Step2.xaml
    /// </summary>
    public partial class RosaStep2 : Page
    {
        List<string> ErrorMessages = new();
        double rosaFirstLightThreshold = 0.06;

        public RosaStep2()
        {
            InitializeComponent();
        }

        void OnLoad(object sender, RoutedEventArgs e)
        {
            var w = MainWindow.GetWindow(this) as MainWindow;
        }
       
        private void Next_Step_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new RosaStep3());
        }
         
        private void GoTo_Start_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new RosaStep0());
        }

        private void AlignmentButtonClick(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Wait;

            ErrorMessages.Clear();

            Instruments.instance.SetArroyoLaserOff();
            ROSAStep2();

            Mouse.OverrideCursor = Cursors.Arrow;
        }

        private void stepSuccessUiUpdate()
        {
            errorPanel.Visibility = Visibility.Collapsed;
            failedMessage.Visibility = Visibility.Collapsed;
            successMessage.Visibility = Visibility.Visible;

            NextStepButton.Visibility = Visibility.Visible;
            AlignmentButton.Visibility = Visibility.Collapsed;
            nextDeviceButton.Visibility = Visibility.Collapsed;
            endJobButton.Visibility = Visibility.Collapsed;
        }

        private void stepErrorUiUpdate()
        {
            errorList.ItemsSource = ErrorMessages;
            errorPanel.Visibility = Visibility.Visible;
            failedMessage.Visibility = Visibility.Visible;
            failedMessage.Text = $"Alignment attempt failed, check TO and lens";
            successMessage.Visibility = Visibility.Collapsed;
            NextStepButton.Visibility = Visibility.Collapsed;

            endJobButton.Visibility = Visibility.Visible;
            nextDeviceButton.Visibility = Visibility.Visible;

            this.AlignmentButton.Content = "Retry alignment";
            AlignmentButton.Visibility = Visibility.Visible;
        }

        private async void ROSAStep2()
        {
            var w = Window.GetWindow(this) as MainWindow;
            var rosa = w.device as ROSADevice;
            var output = w.output as ROSAOutput;

            var refUnits = MainWindow.Conn.GetROSAReferenceUnits(output);
            var voltage = Instruments.instance.GetAerotechAnalogVoltage();

            if (refUnits != null)
            {
                if (voltage < rosaFirstLightThreshold)
                {
                    Instruments.instance.SetAerotechPosition(refUnits.X, refUnits.Y, refUnits.Z);
                }
            }

            firstLightInfoPanel.Visibility = Visibility.Visible;
            firstLightVoltage.Text = $"Initial Resp: {String.Format("{0:0.00}", (((voltage * 0.596) - 0.006) / output.Fiber_Power))}";

            if (voltage < rosaFirstLightThreshold)
            {
                Debug.Print("Voltage is too low: {0}", voltage);
                Debug.Print("Finding first light");

                // show progress bar
                progressPanel.Visibility = Visibility.Visible;
                w.IsHitTestVisible = false;

                var t = Task.Run(() =>
                {
                    Instruments.instance.FindFirstLightROSA();
                });

                await t;

                w.IsHitTestVisible = true;
                progressPanel.Visibility = Visibility.Collapsed;

                voltage = Instruments.instance.GetAerotechAnalogVoltage();
                firstLightVoltage.Text = $"Resp: {String.Format("{0:0.00}", (((voltage * 0.596) - 0.006) / output.Fiber_Power))}";
            }

            if (voltage < rosaFirstLightThreshold)
            {
                Debug.Print("Input voltage: {0}", voltage);
                Debug.Print("Could not find first light");

                ErrorMessages.Clear();
                ErrorMessages.Add("Could not find first light.");

                firstLightVoltage.Text = $"Resp: {String.Format("{0:0.00}", ((voltage * 0.596) - 0.006) / output.Fiber_Power)}";

                errorList.ItemsSource = ErrorMessages;
                errorPanel.Visibility = Visibility.Visible;
                AlignmentButton.Visibility = Visibility.Visible;
                endJobButton.Visibility = Visibility.Visible;

                AlignmentButton.Visibility = Visibility.Collapsed;
                nextDeviceButton.Visibility = Visibility.Visible;

                retryPanel.Visibility = Visibility.Visible;

                return;
            }

            int iterCount = 0;
            double resp = voltage * 0.596 - 0.006 / output.Fiber_Power;

            if (resp < rosa.Resp_Min)
            {
                AlignmentButton.Visibility = Visibility.Collapsed;

                progressPanel.Visibility = Visibility.Visible;
                w.IsHitTestVisible = false;

                var t = Task.Run(() =>
                {
                    while (iterCount < 3 && resp < 0.41)
                    {

                        if (iterCount == 0)
                        {
                            Instruments.instance.FindCentroidRosa1(voltage * 0.85, 0.0015);
                        }

                        if (iterCount == 1)
                        {
                            Instruments.instance.FindCentroidRosa2(voltage * 0.95, 0.001);
                        }

                        if (iterCount == 2)
                        {
                            Instruments.instance.FindCentroidRosa3(voltage * 0.95, 0.00025);
                        }

                        var voltageAfterAlignment = Instruments.instance.GetAerotechAnalogVoltage();

                        var currentAfterAlignment = (voltageAfterAlignment * 0.596) - 0.006;
                        var responsivityAfterAlignment = currentAfterAlignment / output.Fiber_Power;

                        iterCount++;

                        resp = responsivityAfterAlignment;
                    }
                });

                await t;

                w.IsHitTestVisible = true;
                progressPanel.Visibility = Visibility.Collapsed;

                if (resp < rosa.Resp_Min) // get min responsivity from device
                {
                    ErrorMessages.Add($"Responsivity is too low:");
                }

                if (resp > 0.65)
                {
                    ErrorMessages.Add($"Restest Fiber Power. Resp is too high: {String.Format("{0:0.00}", (resp))}");
                    GoToStartButton.Visibility = Visibility.Visible;
                }

                if (ErrorMessages.Count == 0)
                {
                    output.Resp = resp;
                    stepSuccessUiUpdate();
                }
                else
                {
                    output.Resp = resp;
                    stepErrorUiUpdate();
                }
            }
        }

        private void Next_Device_Click(object sender, RoutedEventArgs e)
        {
            var w = Window.GetWindow(this) as MainWindow;
            var currentOutput = w.output;
            var job = currentOutput.Job_Number;
            var unitNumber = currentOutput.Unit_Number;
            var op = currentOutput.Operator;

            w.output = new ROSAOutput
            {
                Part_Number = w.device.Part_Number,
                Passed = false,
                Job_Number = job,
                Operator = op,
                Unit_Number = unitNumber + 1
            };

            NavigationService.Navigate(new RosaStep1());
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

        private async void retryFirstLightButton_Click(object sender, RoutedEventArgs e)
        {
            var pow = Instruments.instance.GetAerotechAnalogVoltage();
            var found = displayFirstLightErrors(pow, rosaFirstLightThreshold);

            if (found) return;

            Mouse.OverrideCursor = Cursors.Wait;
            firstLightVoltage.Text = "Finding first light";

            var w = Window.GetWindow(this) as MainWindow;

            progressPanel.Visibility = Visibility.Visible;
            w.IsHitTestVisible = false;

            var t = Task.Run(() =>
            {
                Instruments.instance.FindFirstLightROSA();
            });

            await t;

            w.IsHitTestVisible = true;
            progressPanel.Visibility = Visibility.Collapsed;

            Mouse.OverrideCursor = Cursors.Arrow;

            var voltage = Instruments.instance.GetAerotechAnalogVoltage();
            displayFirstLightErrors(voltage, rosaFirstLightThreshold);
        }

        private bool displayFirstLightErrors(double voltage, double threshold)
        {
            var w = Window.GetWindow(this) as MainWindow;
            var output = w.output as ROSAOutput;
            firstLightVoltage.Text = $"Resp. after alignment: {String.Format("{0:0.00}", (((voltage * 0.596) - 0.006) / output.Fiber_Power))}";

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
                firstLightVoltage.Text = $"Found first light.";
                retryPanel.Visibility = Visibility.Collapsed;
                nextDeviceButton.Visibility = Visibility.Collapsed;
                endJobButton.Visibility = Visibility.Collapsed;
                errorPanel.Visibility = Visibility.Collapsed;

                return true;
            }
        }

        private void endJobButton_Click(object sender, RoutedEventArgs e)
        {
            var w = Window.GetWindow(this) as MainWindow;
            var currentOutput = w.output;
            var job = currentOutput.Job_Number;
            var unitNumber = currentOutput.Unit_Number;
            var op = currentOutput.Operator;

            w.output = new ROSAOutput
            {
                Part_Number = w.device.Part_Number,
                Passed = false,
                Job_Number = job,
                Operator = op,
                Unit_Number = unitNumber + 1
            };

            NavigationService.Navigate(new HomePage());
        }
    }
}
