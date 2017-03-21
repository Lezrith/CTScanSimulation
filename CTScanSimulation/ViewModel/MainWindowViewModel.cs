using CTScanSimulation.Command;
using CTScanSimulation.Model;
using CTScanSimulation.View;
using Microsoft.Win32;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace CTScanSimulation.ViewModel
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private bool canCreateSinogram;
        private bool canRecreateImage;
        private CtScan ctScan;
        private BitmapImage imageWithCt;
        private double meanSquaredError;
        private string orginalImagePath;
        private BitmapImage recreatedImage;
        private int recreationLoopStep;
        private BitmapImage sinogram;
        private int sinogramLoopStep;

        public MainWindowViewModel()
        {
            FilePickerButtonCommand = new RelayCommand(PickFile);
            CreateSinogramButtonCommand = new RelayCommand(CreateSinogram);
            RecreateImageButtonCommand = new RelayCommand(RecreateImage);
            UpdateOrginalImageCommand = new RelayCommand(UpdateOrginalImage);
            UpdateRecreatedImageCommand = new RelayCommand(UpdateRecreatedImage);
            OpenSavingDialogButtonCommand = new RelayCommand(OpenSavingDialog);

            EmitterDetectorSystemStep = 0.1f;
            SinogramLoopStep = 0;
            RecreationLoopStep = 0;
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
        public bool Filtering { get; set; }
        private bool canOpenSavingDialog;

        public bool CanOpenSavingDialog
        {
            get { return canOpenSavingDialog; }
            set { canOpenSavingDialog = value; OnPropertyChanged(nameof(CanOpenSavingDialog)); }
        }

        public BitmapImage ImageWithCt
        {
            get { return imageWithCt; }
            set { imageWithCt = value; OnPropertyChanged(nameof(ImageWithCt)); }
        }

        public double MeanSquaredError
        {
            get { return meanSquaredError; }
            set { meanSquaredError = value; OnPropertyChanged(nameof(MeanSquaredError)); }
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

        public int RecreationLoopStep
        {
            get { return recreationLoopStep; }
            set { recreationLoopStep = value; OnPropertyChanged(nameof(RecreationLoopStep)); }
        }

        public BitmapImage Sinogram
        {
            get { return sinogram; }
            set { sinogram = value; OnPropertyChanged(nameof(Sinogram)); }
        }

        public int SinogramLoopStep
        {
            get { return sinogramLoopStep; }
            set { sinogramLoopStep = value; OnPropertyChanged(nameof(SinogramLoopStep)); }
        }

        public ICommand UpdateOrginalImageCommand { get; set; }

        public ICommand UpdateRecreatedImageCommand { get; set; }
        public ICommand OpenSavingDialogButtonCommand { get; set; }

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
            SinogramLoopStep = 0;
            RecreationLoopStep = 0;
            OrginalImagePath = openPicker.FileName;
            CanRecreateImage = false;
            CanOpenSavingDialog = false;
            CanCreateSiogram = true;
            var originalImage = new Bitmap(orginalImagePath);
            ctScan = new CtScan(originalImage, EmitterDetectorSystemStep, NumberOfDetectors, EmitterDetectorSystemWidth, Filtering);
            ImageWithCt = ctScan.DrawCtSystem(0);
        }

        private void RecreateImage(object obj)
        {
            RecreatedImage = ctScan.RecreateImage();
            MeanSquaredError = ctScan.CalculateMeanSquaredError();
            CanOpenSavingDialog = true;
        }

        private void UpdateOrginalImage(object obj)
        {
            if (ctScan == null || sinogramLoopStep < 1) return;
            ImageWithCt = ctScan.DrawCtSystem(sinogramLoopStep - 1);
            Sinogram = ctScan.CreateSinogram(sinogramLoopStep - 1);
            imageWithCt = Sinogram;
        }

        private void UpdateRecreatedImage(object obj)
        {
            if (ctScan == null || recreationLoopStep < 1) return;
            ImageWithCt = ctScan.DrawCtSystem(recreationLoopStep - 1);
            RecreatedImage = ctScan.RecreateImage(recreationLoopStep - 1);
        }

        private void OpenSavingDialog(object obj)
        {
            var dICOMSaveWindow = new DICOMSaveWindow();
            dICOMSaveWindow.mainGrid.DataContext = new DICOMSaveViewModel(recreatedImage);
            dICOMSaveWindow.ShowDialog();
        }
    }
}