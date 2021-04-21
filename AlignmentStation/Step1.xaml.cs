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
using System.Diagnostics;
using System.IO;
using Thorlabs.TLPM_64.Interop;
using System.Runtime.InteropServices;
using System.Globalization;
using System.Reflection;

namespace AlignmentStation
{
    /// <summary>
    /// Interaction logic for Step1.xaml
    /// </summary>
    public partial class Step1 : Page
    {
        public Step1()
        {
            InitializeComponent();
        }

        private void Do_Task_Click(object sender, RoutedEventArgs e)
        {
            double power = Instruments.instance.GetPowerMeasurement(); 
            Debug.WriteLine("Power: {0} W", power);
            powerText.Text = "Power: " + power + " W";
        }

        private static void autoset(TLPM device)
        {
            //Set to PEAK mode for peak search/ autoset
            device.setFreqMode(1);

            device.startPeakDetector();

            System.Threading.Thread.Sleep(1000);

            bool isRunning = true;

            while (isRunning)
            {
                device.isPeakDetectorRunning(out isRunning);
            }

            //Set to CW mode for normal measurement
            device.setFreqMode(0);
        }
    }
 }
