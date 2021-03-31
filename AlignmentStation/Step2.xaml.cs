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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace AlignmentStation
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Step2 : Page
    {
        private int NumTries;

        public Step2()
        {
            this.InitializeComponent();
        }

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MainPage));
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            NumTries++;

            bool passedTest = false;

            //TODO perform test and check if test is passed
            
            if (passedTest)
            {
                this.Frame.Navigate(typeof(Step3));
                return;
            }

            if (NumTries != 3)
            {
                // TODO try test again

                ContentDialog dialog = new ContentDialog
                {
                    Title = "Hey",
                    PrimaryButtonText = "Ok",
                    Content = "Check TO and try again"
                };

                await dialog.ShowAsync();
            }
            else
            {
                ContentDialog dialog = new ContentDialog
                {
                    Title = "Remove Lens barrel",
                    PrimaryButtonText = "Ok"
                };

                await dialog.ShowAsync();
            }
        }
    }
}
