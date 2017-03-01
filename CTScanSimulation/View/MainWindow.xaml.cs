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

namespace CTScanSimulation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
        }

        private void emiterDetectorSystemStepSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (loopStepSlider != null)
            {
                loopStepSlider.Maximum = Math.Floor(360 / emiterDetectorSystemStepSlider.Value);
            }
        }
    }
}