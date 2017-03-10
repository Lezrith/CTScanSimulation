using System;
using System.Windows;

namespace CTScanSimulation.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        private void emitterDetectorSystemStepSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sinogramLoopStepSlider != null)
            {
                sinogramLoopStepSlider.Maximum = Math.Floor(360 / emitterDetectorSystemStepSlider.Value);
                recreationLoopStepSlider.Maximum = Math.Floor(360 / emitterDetectorSystemStepSlider.Value);
            }
        }
    }
}