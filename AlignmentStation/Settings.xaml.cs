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

namespace AlignmentStation
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Page
    {
        public bool showNewPartForm = false;
        public BooleanToVisibilityConverter btvc = new BooleanToVisibilityConverter();
        private Controller c;

        public Settings()
        {
            InitializeComponent();
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void NewDeviceButtonClick(object sender, RoutedEventArgs e)
        {
            this.showNewPartForm = true;
        }

        private void Calibrate_Limits_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                c = Controller.Connect();
            }
            catch(A3200Exception ex)
            {
                Console.WriteLine("Error connecting to controller");
                return;
            }

            c.Commands.Axes["Y"].Motion.Enable();
            c.Commands.Axes["X"].Motion.Enable();
            c.Commands.Axes["Z"].Motion.Enable();

            try
            {
                c.Commands.Motion.Linear("Y", -100);
            }
            catch(A3200Exception ex)
            {
                Console.WriteLine("Error: {0}", ex.Message);
                c.Parameters.Axes["Y"].Limits.LimitDebounceDistance.Value = 0;
                c.Commands.Axes["Y"].Motion.FaultAck();
            }

            try
            {
                c.Commands.Motion.Linear("Z", 100);
            }
            catch(A3200Exception ex)
            {
                Console.WriteLine("Error: {0}", ex.Message);
                c.Parameters.Axes["Z"].Limits.LimitDebounceDistance.Value = 0;
                c.Commands.Axes["Z"].Motion.FaultAck();
            }

            try
            {
                c.Commands.Motion.Linear("X", 100);
            }
            catch(A3200Exception ex)
            {
                Console.WriteLine("Error: {0}", ex.Message);
                c.Parameters.Axes["X"].Limits.LimitDebounceDistance.Value = 0;
                c.Commands.Axes["X"].Motion.FaultAck();
            }

        }
    }
}
