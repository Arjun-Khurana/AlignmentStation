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

namespace AlignmentStation
{
    /// <summary>
    /// Interaction logic for Step3.xaml
    /// </summary>
    public partial class Step3 : Page
    {
        private List<string> ErrorMessages;
        private bool testComplete = false;

        public Step3()
        {
            InitializeComponent();
        }

        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            if (testComplete)
            {
                NavigationService.Navigate(new HomePage());
                return;
            }

            var w = Window.GetWindow(this) as MainWindow;
            var o = w.output as TOSAOutput;

            var pFC = Instruments.instance.GetThorlabsPower() / Instruments.instance.alignmentPowerCalibration;
            var popCT = pFC / o.P_TO;

            var popCT_Shift = 10 * Math.Log(o.POPCT / popCT);
            o.POPCT_Shift = popCT_Shift;

            if (popCT > 0.7)
            {
                successMessage.Visibility = Visibility.Visible;
                errorPanel.Visibility = Visibility.Collapsed;
                MainWindow.Conn.SaveTOSAOutput(o);
            }
            else
            {
                failedMessage.Visibility = Visibility.Visible;

                ErrorMessages.Add("popCT <= 0.7");
                errorPanel.Visibility = Visibility.Visible;
                errorList.ItemsSource = ErrorMessages;

                MainWindow.Conn.SaveTOSAOutput(o);
            }

            testComplete = true;
            TestButton.Content = "Go home";
        }
    }
}
