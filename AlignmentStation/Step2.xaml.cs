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
    /// Interaction logic for Step2.xaml
    /// </summary>
    public partial class Step2 : Page
    {
        List<string> ErrorMessages = new();
        int attemptNumber = 0;

        public Step2()
        {
            InitializeComponent();
        }

        private void AlignmentButtonClick(object sender, RoutedEventArgs e)
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
                TosaStep2();
            }

            Mouse.OverrideCursor = Cursors.Arrow;
        }

        private void TosaStep2()
        {
            var w = Window.GetWindow(this) as MainWindow;
            if ((w.output as TOSAOutput).P_TO < 0.3)
            {
                Instruments.instance.FindFirstLight();
            }

            var firstLightPower = Instruments.instance.GetThorlabsPower();
            Debug.Print("First light power: {0}", firstLightPower);

            Instruments.instance.FindCentroid();

            var powerAfterAlignment = Instruments.instance.GetThorlabsPower();
            var alignmentPowerCalibration = 500.0;
            Debug.Print("Power after alignment: {0}", powerAfterAlignment);

            var o = w.output as TOSAOutput;
            var d = w.device as TOSADevice;

            o.P_FC = powerAfterAlignment / alignmentPowerCalibration;
            o.POPCT = o.P_FC / o.P_TO;

            if (o.POPCT < d.POPCT_Min)
            {
                ErrorMessages.Add("POPCT > POPCT_MIN");
                this.failedMessage.Visibility = Visibility.Visible;
                this.successMessage.Visibility = Visibility.Collapsed;
            }
            else
            {
                this.failedMessage.Visibility = Visibility.Collapsed;
                this.successMessage.Visibility = Visibility.Visible;

                this.NextStepButton.Visibility = Visibility.Visible;
                this.AlignmentButton.Visibility = Visibility.Collapsed;
            }
        }

    }
}
