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

            var t = MainWindow.Conn.GetTOSADevice(1);
            var i = new ComboBoxItem();
            i.Content = t.Part_Number;

            var t1 = MainWindow.Conn.GetTOSADevice(2);
            var i1 = new ComboBoxItem();
            i1.Content = t1.Part_Number;

            TosaDevices.Add(t);
            TosaDevices.Add(t1);
        }

        
        private void StartButton(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Step1()); 
        }

        private void DeviceSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
