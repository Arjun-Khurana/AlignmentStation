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
using Thorlabs.TLPM_64.Interop;
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

            TOSADevice tosa = (Window.GetWindow(this) as MainWindow).tosaDevice;
            TOSAOutput output = (Window.GetWindow(this) as MainWindow).tosaOutput;
            
            Instruments.instance.SetArroyoLaserOn();
            Instruments.instance.SetArroyoCurrent(tosa.I_Align);

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
            powerText.Text = "Power: " + power * 1000 + " mW";

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

            if (power * 1000 < tosa.P_Min_TO)
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
                Debug.Print("OK! Go to next step.");
            }
            else
            {
                ShowErrorPanels();
                
                if (attemptNumber >= 3)
                {
                    startButton.Content = "Go home";
                }
            }

            Mouse.OverrideCursor = Cursors.Arrow;
        }

        private void Next_Step_Click(object sender, RoutedEventArgs e)
        {

        }
    }
 }
