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
                double maximum = Math.Floor(360 / emitterDetectorSystemStepSlider.Value);
                sinogramLoopStepSlider.Maximum = maximum;
                recreationLoopStepSlider.Maximum = maximum;
            }
        }
    }
}