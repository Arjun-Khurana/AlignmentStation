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
        private Controller c;

        public Settings()
        {
            InitializeComponent();
        }

        private void BackClick(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void NewTOSAButtonClick(object sender, RoutedEventArgs e)
        {
            newTOSAPanel.Visibility = Visibility.Visible;
            addNewROSAButton.Visibility = Visibility.Collapsed;
            addNewTOSAButton.Visibility = Visibility.Collapsed;
            controlPanelContainer.Visibility = Visibility.Collapsed;
        }

        private void NewROSAButtonClick(object sender, RoutedEventArgs e)
        {
            newROSAPanel.Visibility = Visibility.Visible;
            addNewROSAButton.Visibility = Visibility.Collapsed;
            addNewTOSAButton.Visibility = Visibility.Collapsed;
            controlPanelContainer.Visibility = Visibility.Collapsed;
        }

        private void CalibrateLimitsClick(object sender, RoutedEventArgs e)
        {
            Instruments.instance.CalibrateAxes();
        }

        private void SetWavelengthClick(object sender, RoutedEventArgs e)
        {
            Instruments.instance.SetPowerMeterWavelength(850);
        }

        private void SaveROSADeviceButtonClick(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrEmpty(ROSAPartNumberInput.Text))
            {
                var device = new Models.ROSADevice
                {
                    Part_Number = ROSAPartNumberInput.Text,
                    VPD_RSSI = ROSA_VPD_RSSI_Input.Text
                };

                MainWindow.Conn.SaveROSADevice(device);

                newROSAPanel.Visibility = Visibility.Collapsed;
                addNewROSAButton.Visibility = Visibility.Visible;
                addNewTOSAButton.Visibility = Visibility.Visible;
                controlPanelContainer.Visibility = Visibility.Visible;
            }
        }

        private void SaveTOSADeviceButtonClick(object sender, RoutedEventArgs e)
        {
            var device = new Models.TOSADevice
            {
                Part_Number = TOSAPartNumberInput.Text,
                I_Align = Double.Parse(TOSA_I_Align_Input.Text),
                I_Align_Tol = Double.Parse(TOSA_I_Align_Tol_Input.Text),
                P_Min_TO = Double.Parse(TOSA_P_Min_TO_Input.Text),
                P_Min_FC = Double.Parse(TOSA_P_Min_FC_Input.Text),
                V_Max = Double.Parse(TOSA_V_Max_Input.Text),
                POPCT_Min = Double.Parse(TOSA_POPCT_Min_Input.Text),
                P_FC_Shift_Max = Double.Parse(TOSA_P_FC_Shift_Max_Input.Text) 
            };

            MainWindow.Conn.SaveTOSADevice(device);

            newTOSAPanel.Visibility = Visibility.Collapsed;
            addNewROSAButton.Visibility = Visibility.Visible;
            addNewTOSAButton.Visibility = Visibility.Visible;
            controlPanelContainer.Visibility = Visibility.Visible;
        }

        private void CancelTOSAClick(object sender, RoutedEventArgs e)
        {
            TOSAPartNumberInput.Text = null; 
            TOSA_I_Align_Input.Text = null; 
            TOSA_I_Align_Tol_Input.Text = null; 
            TOSA_P_Min_TO_Input.Text = null; 
            TOSA_P_Min_FC_Input.Text = null; 
            TOSA_V_Max_Input.Text = null; 
            TOSA_POPCT_Min_Input.Text = null; 
            TOSA_P_FC_Shift_Max_Input.Text = null; 

            TOSAPartNumberInput.Style = (Style)Application.Current.Resources["RegularTextField"]; 
            TOSA_I_Align_Input.Style = (Style)Application.Current.Resources["RegularTextField"]; 
            TOSA_I_Align_Tol_Input.Style = (Style)Application.Current.Resources["RegularTextField"]; 
            TOSA_P_Min_TO_Input.Style = (Style)Application.Current.Resources["RegularTextField"]; 
            TOSA_P_Min_FC_Input.Style = (Style)Application.Current.Resources["RegularTextField"]; 
            TOSA_V_Max_Input.Style = (Style)Application.Current.Resources["RegularTextField"]; 
            TOSA_POPCT_Min_Input.Style = (Style)Application.Current.Resources["RegularTextField"]; 
            TOSA_P_FC_Shift_Max_Input.Style = (Style)Application.Current.Resources["RegularTextField"]; 

            newTOSAPanel.Visibility = Visibility.Collapsed;
            addNewROSAButton.Visibility = Visibility.Visible;
            addNewTOSAButton.Visibility = Visibility.Visible;
            controlPanelContainer.Visibility = Visibility.Visible;
        }

        private void CancelROSAClick(object sender, RoutedEventArgs e)
        {
            ROSAPartNumberInput.Text = null;
            ROSA_VPD_RSSI_Input.Text = null;

            ROSAPartNumberInput.Style = (Style)Application.Current.Resources["RegularTextField"];
            ROSA_VPD_RSSI_Input.Style = (Style)Application.Current.Resources["RegularTextField"];
            
            newROSAPanel.Visibility = Visibility.Collapsed;
            addNewROSAButton.Visibility = Visibility.Visible;
            addNewTOSAButton.Visibility = Visibility.Visible;
            controlPanelContainer.Visibility = Visibility.Visible;
        }

        private void String_Input_Text_Changed(object sender, TextChangedEventArgs e)
        {
            TextBox t = (TextBox)sender;
            var empty = String.IsNullOrEmpty(t.Text);
            
            if (empty)
            {
                t.Style = (Style)Application.Current.Resources["ErrorTextField"];
            } 
            else
            {
                t.Style = (Style)Application.Current.Resources["RegularTextField"];
            }
        }

        private void Double_Input_Text_Changed(object sender, TextChangedEventArgs e)
        {
            double vpd;
            TextBox t = (TextBox)sender;
            var parsed = Double.TryParse(t.Text, out vpd);
            
            if (parsed)
            {
                t.Style = (Style)Application.Current.Resources["RegularTextField"];
            } 
            else
            {
                t.Style = (Style)Application.Current.Resources["ErrorTextField"];
            }
        }

        private void FindFirstLightClick(object sender, RoutedEventArgs e)
        {
            Instruments.instance.FindFirstLight();
        }

        private void FindCentroidClick(object sender, RoutedEventArgs e)
        {
            var v = Instruments.instance.GetAerotechAnalogVoltage();
            Instruments.instance.FindCentroid(v * 0.75, 0.00025);
        }

        private void AerotechVoltage_Button_Click(object sender, RoutedEventArgs e)
        {
            Instruments.instance.GetAerotechAnalogVoltage();
        }
    }
}
