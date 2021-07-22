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
using System.Windows.Shapes;
using System.Threading;
using System.ComponentModel; // CancelEventArgs

namespace AlignmentStation
{
    /// <summary>
    /// Interaction logic for AlignmentProgress.xaml
    /// </summary>
    public partial class AlignmentProgress : Window
    {
        private CancellationTokenSource ct;

        public AlignmentProgress(CancellationTokenSource ct)
        {
            InitializeComponent();
            this.ct = ct;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Instruments.instance.AerotechAbort();
            ct.Cancel();
            DialogResult = false;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
        }
    }
}
