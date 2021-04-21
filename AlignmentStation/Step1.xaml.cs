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
    /// Interaction logic for Step1.xaml
    /// </summary>
    public partial class Step1 : Page
    {
        Controller c;

        public Step1()
        {
            InitializeComponent();
            c = Controller.Connect();
        }

        private void Do_Task_Click(object sender, RoutedEventArgs e)
        {
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
