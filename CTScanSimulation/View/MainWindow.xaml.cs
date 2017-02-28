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

        private void filePickerButton_Click(object sender, RoutedEventArgs e)
        {
            // Create the OpenFIleDialog object
            Microsoft.Win32.OpenFileDialog openPicker = new Microsoft.Win32.OpenFileDialog();

            // Add file filters
            // We are using excel files in this example
            openPicker.DefaultExt = ".xslt";
            openPicker.Filter = "Obrazy |*.png;*.jpg;*.jpeg;*.bmp";

            // Display the OpenFileDialog by calling ShowDialog method
            Nullable<bool> result = openPicker.ShowDialog();

            // Check to see if we have a result
            if (result == true)
            {
                // Application now has read/write access to the picked file
                // I am saving the file path to a textbox in the UI to display to the user
                filePathTextBox.Text = openPicker.FileName.ToString();
            }
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