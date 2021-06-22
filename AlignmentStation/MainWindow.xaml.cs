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
using AlignmentStation.Data;
using System.Diagnostics;
using System.Runtime.InteropServices;
using AlignmentStation.Models;

namespace AlignmentStation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static SQLiteDeviceRepository Conn = new SQLiteDeviceRepository();
        public Device device;
        public TestOutput output;

        public bool ReferenceMode = false;
        public ReferenceUnits ReferenceUnits;

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            _mainFrame.Navigate(new HomePage());
        }
    }
}
