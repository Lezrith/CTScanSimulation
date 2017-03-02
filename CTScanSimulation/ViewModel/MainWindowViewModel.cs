using CTScanSimulation.Command;
using CTScanSimulation.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace CTScanSimulation.ViewModel
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private bool canCreateSinogram;
        private bool canRecreateImage;
        private CTScan cTScan;
        private BitmapImage imageWithCT;
        private string orginalImagePath;
        private BitmapImage recreatedImage;
        private BitmapImage sinogram;

        public MainWindowViewModel()
        {
            FilePickerButtonCommand = new RelayCommand(PickFile);
            CreateSinogramButtonCommand = new RelayCommand(CreateSinogram);
            RecreateImageButtonCommand = new RelayCommand(RecreateImage);
            CanCreateSiogram = false;
            CanRecreateImage = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public bool CanCreateSiogram
        {
            get { return canCreateSinogram; }
            set { canCreateSinogram = value; OnPropertyChanged(nameof(CanCreateSiogram)); }
        }

        public bool CanRecreateImage
        {
            get { return canRecreateImage; }
            set { canRecreateImage = value; OnPropertyChanged(nameof(CanRecreateImage)); }
        }

        public ICommand CreateSinogramButtonCommand { get; set; }
        public int EmiterDetectorSystemStep { get; set; }
        public int EmiterDetectorSystemWidth { get; set; }
        public ICommand FilePickerButtonCommand { get; set; }
        public ICommand RecreateImageButtonCommand { get; set; }

        public BitmapImage ImageWithCT
        {
            get { return imageWithCT; }
            set { imageWithCT = value; OnPropertyChanged(nameof(ImageWithCT)); }
        }

        public int NumberOfDetectors { get; set; }

        public string OrginalImagePath
        {
            get { return orginalImagePath; }
            set { orginalImagePath = value; OnPropertyChanged(nameof(OrginalImagePath)); }
        }

        public BitmapImage RecreatedImage
        {
            get { return recreatedImage; }
            set { recreatedImage = value; OnPropertyChanged(nameof(recreatedImage)); }
        }

        public BitmapImage Sinogram
        {
            get { return sinogram; }
            set { sinogram = value; OnPropertyChanged(nameof(sinogram)); }
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        private void CreateSinogram(object obj)
        {
            try
            {
                var orginalImage = new Bitmap(orginalImagePath);
                cTScan = new CTScan(orginalImage, EmiterDetectorSystemStep, NumberOfDetectors, EmiterDetectorSystemWidth);
                Sinogram = BitmapToBitmapImage(cTScan.CreateSinogram());
                CanRecreateImage = true;
            }
            catch (FileNotFoundException ex)
            {
                MessageBox.Show(ex.Message, "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void RecreateImage(object obj)
        {
            this.RecreatedImage = BitmapToBitmapImage(cTScan.RecreateImage());
        }

        private void PickFile(object obj)
        {
            // Create the OpenFIleDialog object
            var openPicker = new Microsoft.Win32.OpenFileDialog
            {
                // Add file filters
                DefaultExt = ".png",
                Filter = "Obrazy |*.png;*.jpg;*.jpeg;*.bmp"
            };

            // Display the OpenFileDialog by calling ShowDialog method
            bool? result = openPicker.ShowDialog();

            // Check to see if we have a result
            if (result == true)
            {
                // Application now has read/write access to the picked file
                // I am saving the file path to a textbox in the UI to display to the user
                OrginalImagePath = openPicker.FileName.ToString();
                CanCreateSiogram = true;
            }
        }

        private BitmapImage BitmapToBitmapImage(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                return bitmapImage;
            }
        }
    }
}