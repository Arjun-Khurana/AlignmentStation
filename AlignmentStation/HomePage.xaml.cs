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
        public List<TOSADevice> TosaDevices { get; set; } = new List<TOSADevice>();
        public List<ROSADevice> RosaDevices { get; set; } = new List<ROSADevice>();

        public HomePage()
        {
            InitializeComponent();

            var devices = MainWindow.Conn.GetAllTOSADevices();
            TosaDevices.AddRange(devices);
        }

        
        private void StartButton(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Step1()); 
        }

        private void DeviceSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TOSADevice d = (sender as ComboBox).SelectedItem as TOSADevice;
            Debug.Print("Selected {0}", d.Part_Number);

            MainWindow w = Window.GetWindow(this) as MainWindow;
            w.tosaDevice = d;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Settings());
        }
    }
}
