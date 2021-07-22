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
    public partial class Step2 : Page
    {
        List<string> ErrorMessages = new();
        bool barrelReplaced = false;
        bool firstLightFail = false;
        double tosaFirstLightThreshold = 0.06;
        CancellationTokenSource tokenSource2 = new CancellationTokenSource();

        public Step2()
        {
            InitializeComponent();
        }

        void OnLoad(object sender, RoutedEventArgs e)
        {
            var w = MainWindow.GetWindow(this) as MainWindow;
        }

        private void Next_Step_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Step3());
        }

        private async void AlignmentButtonClick(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Wait;

            ErrorMessages.Clear();

            await TosaStep2();

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
            errorList.ItemsSource = ErrorMessages;
            errorPanel.Visibility = Visibility.Visible;
            failedMessage.Visibility = Visibility.Visible;
            failedMessage.Text = $"Test failed, check TO and lens";
            successMessage.Visibility = Visibility.Collapsed;
            NextStepButton.Visibility = Visibility.Collapsed;

            endJobButton.Visibility = Visibility.Visible;
            nextDeviceButton.Visibility = Visibility.Visible;
            this.AlignmentButton.Content = "Retry alignment";
        }

        private async Task TosaStep2()
        {
            var w = Window.GetWindow(this) as MainWindow;
            var o = w.output as TOSAOutput;
            var d = w.device as TOSADevice;

            var refUnits = MainWindow.Conn.GetTOSAReferenceUnits(o);
            var firstLightPower = Instruments.instance.GetThorlabsPower();

            if (refUnits != null)
            {
                if (firstLightPower < tosaFirstLightThreshold)
                {
                    Instruments.instance.SetAerotechPosition(refUnits.X, refUnits.Y, refUnits.Z);
                }
            }

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

                // add progress bar
                progressPanel.Visibility = Visibility.Visible;

                w.IsHitTestVisible = false;

                var t = Task.Run(() =>
                {
                    Instruments.instance.FindFirstLight();
                });

                await t;

                w.IsHitTestVisible = true;
                progressPanel.Visibility = Visibility.Collapsed;
            }
            
            firstLightPower = Instruments.instance.GetThorlabsPower();
            firstLightVoltage.Text = $"Input power: {firstLightPower / Instruments.instance.alignmentPowerCalibration}";

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
                CancellationToken ct = tokenSource2.Token;

                AlignmentButton.Visibility = Visibility.Collapsed;
                progressPanel.Visibility = Visibility.Visible;
                w.IsHitTestVisible = false;

                var task = Task.Run(() =>
                {
                    while (iterCount < 3 && popCt < 0.72)
                    {
                        firstLightPower = Instruments.instance.GetThorlabsPower();
                        Debug.Print($"Input power: {firstLightPower/Instruments.instance.alignmentPowerCalibration}");

                        if (iterCount == 0)
                        {
                            Instruments.instance.FindCentroid1(firstLightPower * 0.85, 0.001);
                        }
                        
                        if (iterCount == 1)
                        {
                            Instruments.instance.FindCentroid2(firstLightPower * 0.85, 0.0005);
                        }

                        if (iterCount == 2)
                        {
                            Instruments.instance.FindCentroid3(firstLightPower * 0.85, 0.00025);
                        }
                       
                        var powerAfterAlignment = Instruments.instance.GetThorlabsPower();
                        Debug.Print("Power after alignment: {0}", powerAfterAlignment/Instruments.instance.alignmentPowerCalibration);

                        pFC = powerAfterAlignment / Instruments.instance.alignmentPowerCalibration;
                        popCt = pFC / o.P_TO;

                        Debug.Print($"Iteration {iterCount} popCT: {popCt}");
                        iterCount++;
                    }
                });

                await task;

                w.IsHitTestVisible = true;
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

                if (o.POPCT > 0.85)
                {
                    ErrorMessages.Add("POPCT > Max Possible");
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

            w.output = new TOSAOutput
            {
                Part_Number = w.device.Part_Number,
                Passed = false,
                Job_Number = job,
                Operator = op,
                Unit_Number = unitNumber + 1
            };

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

        private async void retryFirstLightButton_Click(object sender, RoutedEventArgs e)
        {
            var pow = Instruments.instance.GetThorlabsPower();
            var found = displayFirstLightErrors(pow, tosaFirstLightThreshold);

            if (found) return;

            Mouse.OverrideCursor = Cursors.Wait;
            firstLightVoltage.Text = "Finding first light";

            var w = Window.GetWindow(this) as MainWindow;

            progressPanel.Visibility = Visibility.Visible;
            w.IsHitTestVisible = false;

            var t = Task.Run(() =>
            {
                Instruments.instance.FindFirstLight(false);
            });

            await t;

            progressPanel.Visibility = Visibility.Collapsed;
            w.IsHitTestVisible = true;

            Mouse.OverrideCursor = Cursors.Arrow;
           
            // displayFirstLightErrors should return false here
            var firstLightPower = Instruments.instance.GetThorlabsPower();
            displayFirstLightErrors(firstLightPower, tosaFirstLightThreshold);
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

            w.output = new TOSAOutput
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
