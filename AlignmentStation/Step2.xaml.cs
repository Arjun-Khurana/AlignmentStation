﻿using System;
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

        public Step2()
        {
            InitializeComponent();
        }

        private void Next_Step_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Step3());
        }

        private void AlignmentButtonClick(object sender, RoutedEventArgs e)
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

            attemptNumber++;

            Mouse.OverrideCursor = Cursors.Wait;

            ErrorMessages.Clear();

            var w = Window.GetWindow(this) as MainWindow;
            if (w.device is TOSADevice)
            {
                TosaStep2();
            }
            else
            {
                RosaStep2();
            }

            Mouse.OverrideCursor = Cursors.Arrow;
        }

        private void RosaStep2()
        {
            var w = Window.GetWindow(this) as MainWindow;
            var rosa = w.device as ROSADevice;
            var output = w.output as ROSAOutput;

            var voltage = Instruments.instance.GetAerotechAnalogVoltage();
            var current = voltage / Instruments.instance.seriesResistance;

            if (voltage < 0.02)
            {
                Debug.Print("Voltage is too low: {0}", voltage);
                Debug.Print("Finding first light");
                Instruments.instance.FindFirstLight();
                voltage = Instruments.instance.GetAerotechAnalogVoltage();
            }

            if (voltage < 0.02)
            {
                Debug.Print("First light voltage: {0}", voltage);
                Debug.Print("Could not find first light");

                ErrorMessages.Clear();
                ErrorMessages.Add("Could not find first light.");
                errorList.ItemsSource = ErrorMessages;
                errorPanel.Visibility = Visibility.Visible;
                AlignmentButton.Visibility = Visibility.Visible;
                AlignmentButton.Content = "Go home";
                attemptNumber = 3;
                barrelReplaced = true;

                nextDeviceButton.Visibility = Visibility.Visible;
                firstLightFail = true;

                return;
            }

            Instruments.instance.FindCentroid(voltage * 0.90, 0.00025 * 5);

            var voltageAfterAlignment = Instruments.instance.GetAerotechAnalogVoltage();
            var currentAfterAlignment = voltageAfterAlignment / Instruments.instance.seriesResistance;
            var responsivityAfterAlignment = currentAfterAlignment / output.Fiber_Power;

            if (responsivityAfterAlignment < rosa.Resp_Min) // get min responsivity from device
            {
                ErrorMessages.Add($"Responsivity is too low: {responsivityAfterAlignment}");
            }

            if (ErrorMessages.Count == 0) 
            {
                output.Resp = responsivityAfterAlignment;
                this.errorPanel.Visibility = Visibility.Collapsed;
                this.failedMessage.Visibility = Visibility.Collapsed;
                this.successMessage.Visibility = Visibility.Visible;

                this.NextStepButton.Visibility = Visibility.Visible;
                this.AlignmentButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                this.errorList.ItemsSource = ErrorMessages;
                this.errorPanel.Visibility = Visibility.Visible;
                this.failedMessage.Visibility = Visibility.Visible;
                this.failedMessage.Text = $"Test attempt {attemptNumber} failed, check TO and lens";
                this.successMessage.Visibility = Visibility.Collapsed;

                this.NextStepButton.Visibility = Visibility.Collapsed;

                if (attemptNumber > 3)
                {
                    this.AlignmentButton.Content = "End job";

                    MainWindow.Conn.SaveROSAOutput(output);
                    this.AlignmentButton.Content = "End job";
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

                    this.AlignmentButton.Content = "Retry";
                }
            }
        }

        private void TosaStep2()
        {
            var w = Window.GetWindow(this) as MainWindow;
            var firstLightPower = Instruments.instance.GetThorlabsPower();
            if (firstLightPower < 0.01)
            {
                Debug.Print("Power is too low: {0}", firstLightPower);
                Debug.Print("Finding first light");
                Instruments.instance.FindFirstLight();
                firstLightPower = Instruments.instance.GetThorlabsPower();
            }

            if (firstLightPower < 0.01)
            {
                Debug.Print("First light power: {0}", firstLightPower);
                Debug.Print("Could not find first light");

                ErrorMessages.Clear();
                ErrorMessages.Add("Could not find first light.");
                errorList.ItemsSource = ErrorMessages;
                errorPanel.Visibility = Visibility.Visible;
                AlignmentButton.Visibility = Visibility.Visible;
                AlignmentButton.Content = "End job";
                attemptNumber = 3;
                barrelReplaced = true;

                nextDeviceButton.Visibility = Visibility.Visible;
                firstLightFail = true;

                return;
            }

            var o = w.output as TOSAOutput;
            var d = w.device as TOSADevice;

            double popCt = 0;
            double pFC = 0;
            int iterCount = 0;
            while (iterCount < 3 && popCt < 0.7)
            {
                firstLightPower = Instruments.instance.GetThorlabsPower();
                Debug.Print($"First light power: {firstLightPower}");

                Instruments.instance.FindCentroid(firstLightPower * 0.85, 0.00025);

                var powerAfterAlignment = Instruments.instance.GetThorlabsPower();
                Debug.Print("Power after alignment: {0}", powerAfterAlignment);

                pFC = powerAfterAlignment / Instruments.instance.alignmentPowerCalibration; 
                popCt = pFC / o.P_TO;

                Debug.Print($"Iteration {iterCount} popCT: {popCt}");
                iterCount++;
            }

            o.P_FC = pFC;
            o.POPCT = popCt;

            if (o.POPCT < d.POPCT_Min)
            {
                ErrorMessages.Add("POPCT < POPCT_MIN");
                this.failedMessage.Visibility = Visibility.Visible;
                this.successMessage.Visibility = Visibility.Collapsed;
            }

            if (ErrorMessages.Count == 0) 
            {
                this.errorPanel.Visibility = Visibility.Collapsed;
                this.failedMessage.Visibility = Visibility.Collapsed;
                this.successMessage.Visibility = Visibility.Visible;

                this.NextStepButton.Visibility = Visibility.Visible;
                this.AlignmentButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                this.errorList.ItemsSource = ErrorMessages;
                this.errorPanel.Visibility = Visibility.Visible;
                this.failedMessage.Visibility = Visibility.Visible;
                this.failedMessage.Text = $"Test attempt {attemptNumber} failed, check TO and lens";
                this.successMessage.Visibility = Visibility.Collapsed;

                this.NextStepButton.Visibility = Visibility.Collapsed;

                if (attemptNumber > 3)
                {
                    MainWindow.Conn.SaveTOSAOutput(o);
                    this.AlignmentButton.Content = "End job";
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

                    this.AlignmentButton.Content = "Retry";
                }
            }
        }

        private void Next_Device_Click(object sender, RoutedEventArgs e)
        {
            var w = MainWindow.GetWindow(this) as MainWindow;
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
    }
}
