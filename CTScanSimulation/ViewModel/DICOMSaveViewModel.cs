using CTScanSimulation.Command;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace CTScanSimulation.ViewModel
{
    internal class DICOMSaveViewModel : INotifyPropertyChanged
    {
        private DateTime examinationDate = DateTime.Today;

        private Sex patientSex = Sex.Male;

        public DICOMSaveViewModel(BitmapImage recreatedImage)
        {
            this.recreatedImage = recreatedImage;
            SaveButtonCommand = new RelayCommand(SaveToDICOM);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public enum Sex
        {
            Male, Female, Other
        }

        public string Comment { get; set; }

        public DateTime ExaminationDate
        {
            get
            {
                return examinationDate;
            }

            set
            {
                examinationDate = value;
                OnPropertyChanged(nameof(ExaminationDate));
            }
        }

        public string OutputFilePath { get; set; }

        public DateTime PatientDateOfBirth { get; set; } = DateTime.Today;

        public string PatientName { get; set; }

        public Sex PatientSex
        {
            get { return patientSex; }
            set { patientSex = value; OnPropertyChanged(nameof(PatientSex)); }
        }

        private readonly BitmapImage recreatedImage;
        public ICommand SaveButtonCommand { get; set; }

        public Array Sexes
        {
            get
            {
                return Enum.GetValues(typeof(Sex));
            }
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void SaveToDICOM(object obj)
        {
        }
    }
}