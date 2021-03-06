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
        List<string> ErrorMessages = new();
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
         //   if (w.output is ROSAOutput)
          //  {
         //       RosaStep3();
         //   }
         //   else
            {
                TosaStep3();
            }
        }

     //   private void RosaStep3()
     //   {
     //       var w = Window.GetWindow(this) as MainWindow;
     //       var o = w.output as ROSAOutput;
      //      var d = w.device as ROSADevice;

      //      var voltage = Instruments.instance.GetAerotechAnalogVoltage();
      //      var current = (voltage * 0.596) - 0.006;
      //      var responsivity = current / o.Fiber_Power;

      //      var resp_shift = 10 * Math.Log(o.Resp / responsivity);

      //      o.Resp_Post_Cure = responsivity;
     //       o.Resp_Shift = resp_shift;

       //     if (resp_shift > 1 || responsivity < d.Resp_Min)
      //      {
      //          ErrorMessages.Add($"Responsivity: {responsivity}, shift: {resp_shift}");
      //          successMessage.Visibility = Visibility.Collapsed;
       //         errorList.ItemsSource = ErrorMessages;
       //         errorPanel.Visibility = Visibility.Visible;
       //     }
       //     else
        //    {
        //        o.Passed = true;
       //         successMessage.Visibility = Visibility.Visible;
       //         errorPanel.Visibility = Visibility.Collapsed;
       //     }

       //     MainWindow.Conn.SaveROSAOutput(o);

       //     if (w.ReferenceMode)
        //    {
       //         SaveRefUnits();
       //     }

       //     testComplete = true;
       //     TestButton.Content = "End job";
       //     nextDeviceButton.Visibility = Visibility.Visible;
    //    }

        private void TosaStep3()
        {
            var w = Window.GetWindow(this) as MainWindow;
            var o = w.output as TOSAOutput;
            var d = w.device as TOSADevice;

            var pFC = Instruments.instance.GetThorlabsPower();
            pFC /= Instruments.instance.alignmentPowerCalibration;
            var popCT = pFC / o.P_TO;

            var popCT_Shift = 10 * Math.Log(o.POPCT / popCT);
            o.POPCT_Post_Cure = popCT;
            o.POPCT_Shift = popCT_Shift;

            if (popCT >= d.POPCT_Min) 
            {
                o.Passed = true;
                Finalresult.Visibility = Visibility.Visible;
                Finalresulttext.Text = $"Final Coupling Eff: {String.Format("{0:0.00}", (popCT*100))}";
                Finalresulttext2.Text = $"Shift: {String.Format("{0:0.00}",popCT_Shift)}";
                successMessage.Visibility = Visibility.Visible;
                errorPanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                failedMessage.Visibility = Visibility.Visible;
                Finalresult.Visibility = Visibility.Visible;
                Finalresulttext.Text = $"Final Coupling Eff: {String.Format("{0:0.00}", (popCT * 100))}%";
                Finalresulttext2.Text = $"Shift: {String.Format("{0:0.00}", popCT_Shift)}dB";
                ErrorMessages.Add($"Final Coupling Eff < {d.POPCT_Min}");
                errorPanel.Visibility = Visibility.Visible;
                errorList.ItemsSource = ErrorMessages;
            }

            MainWindow.Conn.SaveTOSAOutput(o);

            //TODO: Query aerotech position and save in reference units
            if (w.ReferenceMode)
            {
                SaveRefUnits();
            }

            testComplete = true;
            TestButton.Content = "End job";

            nextDeviceButton.Visibility = Visibility.Visible;

            Instruments.instance.SetArroyoLaserOff();
        }

        private void Next_Device_Click(object sender, RoutedEventArgs e)
        {
            var w = Window.GetWindow(this) as MainWindow;

            var d = w.device as TOSADevice;
            var currentOutput = w.output as TOSAOutput;
            var job = currentOutput.Job_Number;

            w.output = new TOSAOutput
            {
                Part_Number = d.Part_Number,
                Passed = false,
                Job_Number = job,
                Operator = currentOutput.Operator,
                Unit_Number = currentOutput.Unit_Number + 1
            };

            NavigationService.Navigate(new Step1());
        }

        private void SaveRefUnits()
        {
            var w = Window.GetWindow(this) as MainWindow;
            var o = w.output;
            
            var position = Instruments.instance.GetAerotechPosition();
            (string X, string Y, string Z) formatted = (
                String.Format("{0:0.000}", position.X),
                String.Format("{0:0.000}", position.Y),
                String.Format("{0:0.000}", position.Z)
                );

            RefUnitPanel.Visibility = Visibility.Visible;
            RefUnitText.Text = $"Saving reference units: ({formatted.X}, {formatted.Y}, {formatted.Z}";

            var refUnits = new ReferenceUnits {
                X = position.X,
                Y = position.Y,
                Z = position.Z,
                Part_Number = o.Part_Number
            };

            MainWindow.Conn.SaveTOSAReferenceUnits(refUnits);
        }

        private void Quit_Button_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult messageBoxResult = 
                MessageBox.Show("Are you sure you want to quit?", "Quit Confirmation", MessageBoxButton.YesNo);


            if (messageBoxResult == MessageBoxResult.Yes)
            {
                NavigationService.Navigate(new HomePage());
            }
        }
    }
}
