using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using CTScanSimulation.Command;
using CTScanSimulation.Model;
using Microsoft.Win32;

namespace CTScanSimulation.ViewModel
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private bool canCreateSinogram;
        private bool canRecreateImage;
        private CtScan ctScan;
        private BitmapImage imageWithCt;
        private int loopStep;
        private string orginalImagePath;
        private BitmapImage recreatedImage;
        private BitmapImage sinogram;

        public MainWindowViewModel()
        {
            FilePickerButtonCommand = new RelayCommand(PickFile);
            CreateSinogramButtonCommand = new RelayCommand(CreateSinogram);
            RecreateImageButtonCommand = new RelayCommand(RecreateImage);
            UpdateOrginalImageCommand = new RelayCommand(UpdateOrginalImage);

            EmitterDetectorSystemStep = 0.1f;
            LoopStep = 1;
            NumberOfDetectors = 2;
            EmitterDetectorSystemWidth = 10;

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
        public float EmitterDetectorSystemStep { get; set; }
        public int EmitterDetectorSystemWidth { get; set; }
        public ICommand FilePickerButtonCommand { get; set; }

        public BitmapImage ImageWithCt
        {
            get { return imageWithCt; }
            set { imageWithCt = value; OnPropertyChanged(nameof(ImageWithCt)); }
        }

        public int LoopStep
        {
            get { return loopStep; }
            set { loopStep = value; OnPropertyChanged(nameof(LoopStep)); }
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
            set { recreatedImage = value; OnPropertyChanged(nameof(RecreatedImage)); }
        }

        public ICommand RecreateImageButtonCommand { get; set; }

        public BitmapImage Sinogram
        {
            get { return sinogram; }
            set { sinogram = value; OnPropertyChanged(nameof(Sinogram)); }
        }

        public ICommand UpdateOrginalImageCommand { get; set; }

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void CreateSinogram(object obj)
        {
            try
            {
                Sinogram = ctScan.CreateSinogram();
                CanRecreateImage = true;
            }
            catch (FileNotFoundException ex)
            {
                MessageBox.Show(ex.Message, "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void PickFile(object obj)
        {
            // Create the OpenFIleDialog object
            var openPicker = new OpenFileDialog
            {
                // Add file filters
                DefaultExt = ".png",
                Filter = "Obrazy |*.png;*.jpg;*.jpeg;*.bmp"
            };

            // Display the OpenFileDialog by calling ShowDialog method
            bool? result = openPicker.ShowDialog();

            // Check to see if we have a result
            if (result != true) return;
            // Application now has read/write access to the picked file
            // I am saving the file path to a textbox in the UI to display to the user
            LoopStep = 1;
            OrginalImagePath = openPicker.FileName;
            CanCreateSiogram = true;
            var orginalImage = new Bitmap(orginalImagePath);
            ctScan = new CtScan(orginalImage, EmitterDetectorSystemStep, NumberOfDetectors, EmitterDetectorSystemWidth);
            UpdateOrginalImage(null);
        }

        private void RecreateImage(object obj)
        {
            RecreatedImage = ctScan.RecreateImage();
        }

        private void UpdateOrginalImage(object obj)
        {
            if (ctScan == null) return;
            ImageWithCt = ctScan.DrawCtSystem(loopStep - 1);
            Sinogram = ctScan.CreateSinogram(loopStep - 1);
            imageWithCt = Sinogram;
        }
    }
}