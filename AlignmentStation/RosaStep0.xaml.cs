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
using AlignmentStation.Models;
using System.Diagnostics;

namespace AlignmentStation
{
    /// <summary>
    /// Interaction logic for RosaStep0.xaml
    /// </summary>
    public partial class RosaStep0 : Page
    {
        public RosaStep0()
        {
            InitializeComponent();
            Instruments.instance.SetArroyoLaserOff();
        }

        void OnLoad(object sender, RoutedEventArgs e)
        {
            var w = MainWindow.GetWindow(this) as MainWindow;
            UnitNumberText.Text = $"Unit number: {w.output.Unit_Number}";
        }

        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            var w = Window.GetWindow(this) as MainWindow;
            
            ROSADevice rosa = w.device as ROSADevice;
            ROSAOutput output = w.output as ROSAOutput;

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

            Instruments.instance.SetArroyoLaserOff();
            var power = Instruments.instance.GetThorlabsPower();
            Debug.Print($"Fiber power: {power}");
            output.Fiber_Power = 1000 * power / Instruments.instance.alignmentPowerCalibration;

            powerText.Text = $"Fiber power: {String.Format("{0:0.00}",(1000 * power / Instruments.instance.alignmentPowerCalibration))}mW";
            successMessage.Visibility = Visibility.Visible;
            powerText.Visibility = Visibility.Visible;

            startButton.Visibility = Visibility.Collapsed;
            nextStepButton.Visibility = Visibility.Visible;
        }

        private void Next_Step_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new RosaStep1());
        }
    }
}
