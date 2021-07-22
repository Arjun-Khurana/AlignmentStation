using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Diagnostics;
using AlignmentStation.Models;

namespace AlignmentStation
{
    /// <summary>
    /// Interaction logic for HomePage.xaml
    /// </summary>
    public partial class HomePage : Page
    {
        public List<Device> TosaDevices { get; set; } = new List<Device>();
        public List<ROSADevice> RosaDevices { get; set; } = new List<ROSADevice>();

        public HomePage()
        {
            InitializeComponent();
            TosaDevices.AddRange(MainWindow.Conn.GetAllTOSADevices());
            RosaDevices.AddRange(MainWindow.Conn.GetAllROSADevices());
        }

        private void StartButton(object sender, RoutedEventArgs e)
        {
            var w = Window.GetWindow(this) as MainWindow;
            w.ReferenceMode = false;

            this.Start();
        }

        private void ReferenceStartButton(object sender, RoutedEventArgs e)
        {
            var w = Window.GetWindow(this) as MainWindow;
            w.ReferenceMode = true;

            this.Start();
        }

        private void Start()
        {
            if (DeviceSelector.SelectedItem == null)
            {
                Debug.Print("Pick one.");
                ErrorText.Text = "Pick a device";
                return;
            }

            if (String.IsNullOrEmpty(OperatorNameBox.Text))
            {
                Debug.Print("Enter operator name");
                OperatorNameBox.Style = (Style)Application.Current.Resources["ErrorTextField"];
                ErrorText.Text = "Fill out all info";
                return;
            }

            if (String.IsNullOrEmpty(JobNumberBox.Text))
            {
                Debug.Print("Enter job number");
                JobNumberBox.Style = (Style)Application.Current.Resources["ErrorTextField"];
                ErrorText.Text = "Fill out all info";
                return;
            }

            var w = Window.GetWindow(this) as MainWindow;
            w.output.Operator = OperatorNameBox.Text;
            w.output.Job_Number = JobNumberBox.Text.Trim();

            if (w.device is ROSADevice)
            {
                Instruments.instance.SetArroyoLaserOff();

                var fiberPower = MainWindow.Conn.GetLatestROSAFiberPower(JobNumberBox.Text.Trim());

                if (fiberPower != null)
                {
                    (w.output as ROSAOutput).Fiber_Power = (double)fiberPower;
                    Debug.Print($"Using previous fiber power: {fiberPower}");
                }
                else
                {
                    NavigationService.Navigate(new RosaStep0());
                    return;
                }


                Instruments.instance.SetPowerMeterWavelength(850);

                NavigationService.Navigate(new RosaStep1());

            }
            else
            {
                Instruments.instance.SetPowerMeterWavelength(850);

                NavigationService.Navigate(new Step1());
            }

        }

        private void DeviceSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateRelays();
            UpdateOutputAndDevice();
        }

        private void UpdateRelays()
        {

            Device d = DeviceSelector.SelectedItem as Device;

            if (d == null)
            {
                return;
            }

            Mouse.OverrideCursor = Cursors.Wait;
            if (d is TOSADevice)
            {
                Instruments.instance.CloseRelay(2);
                Instruments.instance.CloseRelay(4);
            }
            else {
                var r = d as ROSADevice;

                if (r.VPD_RSSI == "vpd")
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
            }

            Mouse.OverrideCursor = Cursors.Arrow;
        }

        private void UpdateOutputAndDevice()
        {

            Device d = DeviceSelector.SelectedItem as Device;

            if (d == null)
            {
                return;
            }

            var job = JobNumberBox.Text;
            int unit;

            MainWindow w = Window.GetWindow(this) as MainWindow;

            w.device = d;
            if (d is TOSADevice)
            {
                unit = MainWindow.Conn.GetMaxTOSAUnitNumber(job) + 1;
                w.output = new TOSAOutput
                {
                    Part_Number = d.Part_Number,
                    Passed = false,
                    Unit_Number = unit 
                };
            }
            else
            {
                unit = MainWindow.Conn.GetMaxROSAUnitNumber(job) + 1;
                w.output = new ROSAOutput
                {
                    Part_Number = d.Part_Number,
                    Passed = false,
                    Unit_Number = unit
                };
            }

            if (!String.IsNullOrEmpty(job))
            {
                NextNumberLabel.Text = $"Next unit number: {unit}";
                w.output.Job_Number = job;
            }
        }
        
        private void Settings_Button_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Settings());
        }

        private void TOSA_Radio_Checked(object sender, RoutedEventArgs e)
        {
            TosaDevices.Clear();
            TosaDevices.AddRange(MainWindow.Conn.GetAllTOSADevices());
            DeviceSelector.ItemsSource = new List<TOSADevice>();
            DeviceSelector.ItemsSource = TosaDevices;
        }

        private void ROSA_Radio_Checked(object sender, RoutedEventArgs e)
        {
            RosaDevices.Clear();
            RosaDevices.AddRange(MainWindow.Conn.GetAllROSADevices());
            DeviceSelector.ItemsSource = new List<ROSADevice>();
            DeviceSelector.ItemsSource = RosaDevices;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new About());
        }

        private void JobNumberBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateOutputAndDevice();

            var t = sender as TextBox;
            if (String.IsNullOrEmpty(t.Text))
            {
                t.Style = (Style)Application.Current.Resources["ErrorTextField"];
            } else
            {
                t.Style = (Style)Application.Current.Resources["RegularTextField"];
            }
        }

        private void OperatorNameBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var t = sender as TextBox;
            if (String.IsNullOrEmpty(t.Text))
            {
                t.Style = (Style)Application.Current.Resources["ErrorTextField"];
            } else
            {
                t.Style = (Style)Application.Current.Resources["RegularTextField"];
            }
        }
    }
}
