using AlignmentStation.Models;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace AlignmentStation
{
    /// <summary>
    /// Interaction logic for Step3.xaml
    /// </summary>
    public partial class RosaStep3 : Page
    {
        List<string> ErrorMessages = new();
        List<string> SuccessMessages = new();
        private bool testComplete = false;

        public RosaStep3()
        {
            InitializeComponent();
            Instruments.instance.SetArroyoLaserOff();
        }

        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            if (testComplete)
            {
                NavigationService.Navigate(new HomePage());
                return;
            }

            var w = Window.GetWindow(this) as MainWindow;
            //  if (w.output is ROSAOutput)
            {
                Instruments.instance.SetArroyoLaserOff();
                ROSAStep3();
            }
            //   else
            //   {
            //       TosaStep3();
            //   }
        }

        private void ROSAStep3()
        {
            var w = Window.GetWindow(this) as MainWindow;
            var o = w.output as ROSAOutput;
            var d = w.device as ROSADevice;

            var voltage = Instruments.instance.GetAerotechAnalogVoltage();
            var current = (voltage * 0.596) - 0.006;
            var responsivity = current / o.Fiber_Power;

            var resp_shift = 10 * Math.Log(o.Resp / responsivity);

            o.Resp_Post_Cure = responsivity;
            o.Resp_Shift = resp_shift;

            if (resp_shift > 1 || responsivity < d.Resp_Min)
            {

                ErrorMessages.Add($"Responsivity: {String.Format("{0:0.00}", responsivity)}, Shift: {String.Format("{0:0.00}", resp_shift)} dB ");
                successMessage.Visibility = Visibility.Collapsed;
                failedMessage.Visibility = Visibility.Visible;
                errorList.ItemsSource = ErrorMessages;
                errorPanel.Visibility = Visibility.Visible;
                successPanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                o.Passed = true;
                SuccessMessages.Add($"Responsivity: {String.Format("{0:0.00}", responsivity)}, Shift: {String.Format("{0:0.00}", resp_shift)} dB ");
                successMessage.Visibility = Visibility.Visible;
                failedMessage.Visibility = Visibility.Collapsed;
                successList.ItemsSource = SuccessMessages;
                errorPanel.Visibility = Visibility.Collapsed;
                successPanel.Visibility = Visibility.Visible;
            }

            MainWindow.Conn.SaveROSAOutput(o);

            if (w.ReferenceMode)
            {
                SaveRefUnits();
            }

            testComplete = true;
            TestButton.Content = "End job";
            nextDeviceButton.Visibility = Visibility.Visible;
        }

        private void Next_Device_Click(object sender, RoutedEventArgs e)
        {
            var w = Window.GetWindow(this) as MainWindow;

            var d = w.device as ROSADevice;
            var currentOutput = w.output as ROSAOutput;
            var fib = currentOutput.Fiber_Power;
            var job = currentOutput.Job_Number;

            w.output = new ROSAOutput
            {
                Part_Number = d.Part_Number,
                Passed = false,
                Job_Number = job,
                Operator = currentOutput.Operator,
                Unit_Number = currentOutput.Unit_Number + 1,
                Fiber_Power = fib
            };

            NavigationService.Navigate(new RosaStep1());
        }

        private void SaveRefUnits()
        {
            var w = Window.GetWindow(this) as MainWindow;
            var o = w.output;

            var position = Instruments.instance.GetAerotechPosition();
            (string X, string Y, string Z) formatted = (
                String.Format("{0:0.000}", position.X),
                String.Format("{0:0.000}", position.Y),
                String.Format("{0:0.000}", position.Z)
                );

            RefUnitPanel.Visibility = Visibility.Visible;
            RefUnitText.Text = $"Saving reference units: ({formatted.X}, {formatted.Y}, {formatted.Z}";

            var refUnits = new ReferenceUnits
            {
                X = position.X,
                Y = position.Y,
                Z = position.Z,
                Part_Number = o.Part_Number
            };

            MainWindow.Conn.SaveROSAReferenceUnits(refUnits);
        }

        private void Quit_Button_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult messageBoxResult =
                MessageBox.Show("Are you sure you want to quit?", "Quit Confirmation", MessageBoxButton.YesNo);


            if (messageBoxResult == MessageBoxResult.Yes)
            {
                NavigationService.Navigate(new HomePage());
            }
        }
    }
}
