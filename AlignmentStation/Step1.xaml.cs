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
using NationalInstruments.Visa;
using Ivi.Visa;

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
            Instruments.instance.OpenArroyo();
            Instruments.instance.CallArroyo();


            Instruments.instance.GetPowerMeasurement();
        }

    }
 }
