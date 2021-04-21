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
            HandleRef Instrument_Handle = new HandleRef();

            TLPM searchDevice = new TLPM(Instrument_Handle.Handle);

            uint count = 0;

            string firstPowermeterFound = "";

            try
            {
                int pInvokeResult = searchDevice.findRsrc(out count);

                if (count > 0)
                {
                    StringBuilder descr = new StringBuilder(1024);

                    searchDevice.getRsrcName(0, descr);

                    firstPowermeterFound = descr.ToString();
                }
            }
            catch { }

            if (count == 0)
            {
                searchDevice.Dispose();
                Debug.WriteLine("No power meter could be found.");
                return;
            }

            //always use true for ID Query
            TLPM device = new TLPM(firstPowermeterFound, true, false);  //  For valid Ressource_Name see NI-Visa documentation.
            Debug.WriteLine("Power meter found: {0}", firstPowermeterFound);

            double measuredPower = 0;
            short unit = 0;
            
            device.getPowerUnit(out unit);
            device.measPower(out measuredPower);

            Debug.WriteLine("Measured Power: {0}", measuredPower);
            Debug.WriteLine("Measured Power unit: {0}", unit);

            device.Dispose();
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
