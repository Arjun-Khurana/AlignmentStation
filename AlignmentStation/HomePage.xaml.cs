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
            if (DeviceSelector.SelectedItem == null)
            {
                Debug.Print("Pick one.");
                return;
            }

            if (String.IsNullOrEmpty(OperatorNameBox.Text))
            {
                Debug.Print("Enter operator name");
                return;
            }

            var w = Window.GetWindow(this) as MainWindow;
            w.output.Operator = OperatorNameBox.Text;

            if (w.device is ROSADevice)
            {
                if ((w.output as ROSAOutput).Fiber_Power == 0)
                {
                    NavigationService.Navigate(new RosaStep0());
                    return;
                }
            }

            NavigationService.Navigate(new Step1()); 
        }

        private void DeviceSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Device d = (sender as ComboBox).SelectedItem as Device;

            if (d == null)
            {
                return;
            }

            var job = JobNumberBox.Text;
            MainWindow w = Window.GetWindow(this) as MainWindow;

            w.device = d;
            if (d is TOSADevice)
            {
                w.output = new TOSAOutput
                {
                    Part_Number = d.Part_Number,
                    Job_Number = job,
                    Unit_Number = MainWindow.Conn.GetMaxTOSAUnitNumber(job) + 1
                };

                Instruments.instance.CloseRelay(2);
                Instruments.instance.CloseRelay(4);
            }
            else
            {
                w.output = new ROSAOutput
                {
                    Part_Number = d.Part_Number,
                    Job_Number = job,
                    Unit_Number = MainWindow.Conn.GetMaxROSAUnitNumber(job) + 1,
                };
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

                var fiberPower = MainWindow.Conn.GetLatestROSAFiberPower(job);
                
                if (fiberPower != null)
                {
                    (w.output as ROSAOutput).Fiber_Power = (double)fiberPower;
                }
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
    }
}
