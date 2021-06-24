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
using Aerotech.A3200;
using Aerotech.A3200.Exceptions;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Globalization;
using System.Reflection;
using NationalInstruments.Visa;
using Ivi.Visa;
using AlignmentStation.Models;

namespace AlignmentStation
{
    /// <summary>
    /// Interaction logic for Step1.xaml
    /// </summary>
    public partial class Step1 : Page
    {
        List<string> ErrorMessages = new();
        int attemptNumber = 0;

        public Step1()
        {
            InitializeComponent();
        }

        void OnLoad(object sender, RoutedEventArgs e)
        {
            var w = MainWindow.GetWindow(this) as MainWindow;
            UnitNumberText.Text = $"Unit number: {w.output.Unit_Number}";

            if (w.device is ROSADevice)
            {
                secondInstruction.Text = "(2) Connect fiber from Alignment Interface Box to Alignent Tower Fiber";
                secondInstruction.Visibility = Visibility.Visible;
                thirdInstruction.Visibility = Visibility.Collapsed;
            }

        }

        private void ShowErrorPanels()
        {
            startButton.Content = "Retry test";
            failedMessage.Text = $"Test attempt {attemptNumber} failed";
            failedMessage.Visibility = Visibility.Visible;
            errorPanel.Visibility = Visibility.Visible;
            errorList.Visibility = Visibility.Visible;
            errorList.ItemsSource = ErrorMessages;

            successMessage.Visibility = Visibility.Collapsed;
            nextStepButton.Visibility = Visibility.Collapsed;
            startButton.Visibility = Visibility.Visible;
        }

        private void HideErrorPanels()
        {
            startButton.Content = "Start test";
            failedMessage.Visibility = Visibility.Collapsed;
            errorPanel.Visibility = Visibility.Collapsed;
            errorList.Visibility = Visibility.Collapsed;
            errorList.ItemsSource = null;

            successMessage.Visibility = Visibility.Visible;
            nextStepButton.Visibility = Visibility.Visible;
            startButton.Visibility = Visibility.Collapsed;
        }

        private void Do_Task_Click(object sender, RoutedEventArgs e)
        {
            if (attemptNumber == 3)
            {
                NavigationService.Navigate(new HomePage());
                return;
            }

            attemptNumber++;

            Mouse.OverrideCursor = Cursors.Wait;

            ErrorMessages.Clear();

            var w = Window.GetWindow(this) as MainWindow;
            if (w.device is TOSADevice)
            {
                TosaStep1();
            }
            else
            {
                RosaStep1();
            }

            Mouse.OverrideCursor = Cursors.Arrow;
        }

        private void Next_Step_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Step2());
        }

        private void RosaStep1()
        {
            var w = Window.GetWindow(this) as MainWindow;

            ROSADevice rosa = w.device as ROSADevice;
            ROSAOutput output = w.output as ROSAOutput;

            // use switches to set voltage

            // get voltage from aerotech,
            // multiply by calibration constant to get current 

            // step 1: turn on switches, get voltage going
            //          no measurement

            // measuer power once for the rosa - get the latest job output
            // with the same job number and use that
            // if this is the first of this job,
            // go to step 0 and measure power 

            if (rosa.VPD_RSSI == "vpd")
            {
                Instruments.instance.OpenRelay(1);
                Instruments.instance.OpenRelay(2);
                Instruments.instance.OpenRelay(4);
            } 
            else
            {
                Instruments.instance.CloseRelay(1);
                Instruments.instance.OpenRelay(2);
                Instruments.instance.OpenRelay(4);
            }


            NavigationService.Navigate(new Step2());
            // step 2: do the alignment, get voltage from aerotech
            //          multiply by constant - get from jim
            //          this voltage gets converted to I_Mon
            // step 3: cure the epoxy, retest voltage

        }
        
        private void TosaStep1()
        {
            var w = Window.GetWindow(this) as MainWindow;

            TOSADevice tosa = w.device as TOSADevice;
            TOSAOutput output = w.output as TOSAOutput;
            
            Instruments.instance.SetArroyoLaserOn();
            Debug.Print($"Tosa I_Align: {tosa.I_Align}");
            Instruments.instance.SetArroyoCurrent(tosa.I_Align);

            Instruments.instance.GetArroyoVoltage();
            Instruments.instance.GetArroyoCurrent();

            var voltage = Instruments.instance.GetArroyoVoltage();
            var current = Instruments.instance.GetArroyoCurrent();
            var power = Instruments.instance.GetThorlabsPower();

            Debug.WriteLine("Voltage from Arroyo:");
            Debug.WriteLine(voltage);

            Debug.WriteLine("Current from Arroyo:");
            Debug.WriteLine(current);

            Debug.WriteLine("Power from Thorlabs:");
            Debug.WriteLine(power);

            measurementPanel.Visibility = Visibility.Visible;

            currentText.Text = "Current: " + current + " mA";
            voltageText.Text = "Voltage: " + voltage + " V";
            powerText.Text = "Power: " + power / Instruments.instance.alignmentPowerCalibration + "W";

            //TODO: Compare these values to the ones retrieved from test config
            //      and either accept/reject

            if (current < tosa.I_Align)
            {
                Debug.Print("Current is below acceptable value.");
                ErrorMessages.Add("Current is below acceptable value.");
            }

            if (Math.Abs(current - tosa.I_Align) > tosa.I_Align_Tol)
            {
                Debug.Print("Current drop across Arroyo is greater than threshold");
                ErrorMessages.Add("Current drop across Arroyo is greater than threshold");
            }

            if (power / Instruments.instance.alignmentPowerCalibration * 1000 < tosa.P_Min_TO)
            {
                Debug.Print($"Power {power*1000} is below the acceptable value {tosa.P_Min_TO}.");
                ErrorMessages.Add($"Power is below the acceptable value: {tosa.P_Min_TO} mW.");
            }

            if (voltage > tosa.V_Max)
            {
                Debug.Print("Voltage is greater than acceptable value.");
                ErrorMessages.Add("Voltage is greater than acceptable value.");
            }

            if (ErrorMessages.Count == 0)
            {
                HideErrorPanels();

                output.P_TO = power / Instruments.instance.alignmentPowerCalibration;
                output.I_Align = current;

                Debug.Print("OK! Go to next step.");
            }
            else
            {
                ShowErrorPanels();
                
                if (attemptNumber >= 3)
                {
                    startButton.Content = "End job";
                    MainWindow.Conn.SaveTOSAOutput(output);
                    nextDeviceButton.Visibility = Visibility.Visible;
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
