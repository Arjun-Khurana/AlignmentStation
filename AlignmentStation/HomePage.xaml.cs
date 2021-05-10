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

            NavigationService.Navigate(new Step1()); 
        }

        private void DeviceSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Device d = (sender as ComboBox).SelectedItem as Device;

            MainWindow w = Window.GetWindow(this) as MainWindow;

            w.device = d;
            if (d is TOSADevice)
            {
                w.output = new TOSAOutput();
                w.output.Part_Number = d.Part_Number;
                w.output.Job_Number = "job 1";
                w.output.Unit_Number = MainWindow.Conn.GetMaxTOSAUnitNumber("job 1") + 1;
            }
            else
            {
                w.output = new ROSAOutput();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Settings());
        }

        private void TOSA_Radio_Checked(object sender, RoutedEventArgs e)
        {
            DeviceSelector.ItemsSource = TosaDevices;
        }

        private void ROSA_Radio_Checked(object sender, RoutedEventArgs e)
        {
            DeviceSelector.ItemsSource = RosaDevices;
        }

        private void DeviceSelector_MouseEnter(object sender, EventArgs e)
        {
            if ((bool) TOSA_Radio.IsChecked)
            {
                TosaDevices.Clear();
                TosaDevices.AddRange(MainWindow.Conn.GetAllTOSADevices());
                DeviceSelector.ItemsSource = new List<TOSADevice>();
                DeviceSelector.ItemsSource = TosaDevices;
            }
            else if ((bool) ROSA_Radio.IsChecked)
            {
                RosaDevices.Clear();
                RosaDevices.AddRange(MainWindow.Conn.GetAllROSADevices());
                DeviceSelector.ItemsSource = new List<ROSADevice>();
                DeviceSelector.ItemsSource = RosaDevices;
            }
        }
    }
}
