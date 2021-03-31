using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Diagnostics;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Media.Animation;

namespace AlignmentStation
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Step1 : Page
    {
        private int NumTries = 0;

        public Step1()
        {
            this.InitializeComponent();
            ApplicationView.PreferredLaunchViewSize = new Size(800, 600);
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MainPage));
        }

        private void Do_Task_Click(object sender, RoutedEventArgs e)
        {
            NumTries++;

            bool testPassed = true;
            
            if (testPassed)
            {
                this.Frame.Navigate(typeof(Step2));
            }
            
            //TODO: do tests and check whether it passes or fails
            if (NumTries == 3)
            {
                this.Frame.Navigate(typeof(MainPage));
            }
        }
    }
}
