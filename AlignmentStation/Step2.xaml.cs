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

namespace AlignmentStation
{
    /// <summary>
    /// Interaction logic for Step2.xaml
    /// </summary>
    public partial class Step2 : Page
    {
        public Step2()
        {
            InitializeComponent();
        }

        private void AlignmentButtonClick(object sender, RoutedEventArgs e)
        {
            Instruments.instance.FindFirstLight();
            var firstLightPower = Instruments.instance.GetThorlabsPower();
            Debug.Print("First light power: {0}", firstLightPower);

            Instruments.instance.FindCentroid();
            var powerAfterAlignment = Instruments.instance.GetThorlabsPower();
            var alignmentPowerCalibration = 500.0;
            Debug.Print("Power after alignment: {0}", powerAfterAlignment);

            var w = Window.GetWindow(this) as MainWindow;
            var o = w.output as Models.TOSAOutput;
            o.P_FC = powerAfterAlignment / alignmentPowerCalibration;
            o.POPCT = powerAfterAlignment / o.P_TO;
        }
    }
}
