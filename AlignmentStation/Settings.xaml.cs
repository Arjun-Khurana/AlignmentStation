﻿using System;
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
            Instruments.instance.CalibrateAxes();
        }
    }
}
