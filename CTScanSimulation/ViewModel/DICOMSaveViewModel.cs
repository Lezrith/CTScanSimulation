using CTScanSimulation.Command;
using Dicom;
using Dicom.Imaging;
using Dicom.IO.Buffer;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace CTScanSimulation.ViewModel
{
    internal class DICOMSaveViewModel : INotifyPropertyChanged
    {
        private readonly BitmapImage recreatedImage;
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

        public ICommand SaveButtonCommand { get; set; }

        public Array Sexes
        {
            get
            {
                return Enum.GetValues(typeof(Sex));
            }
        }

        public void ExportImage(Bitmap bitmap)
        {
            bitmap = GetValidImage(bitmap);
            int rows, columns;
            byte[] pixels = GetPixels(bitmap, out rows, out columns);
            MemoryByteBuffer buffer = new MemoryByteBuffer(pixels);
            DicomDataset dataset = new DicomDataset();
            FillDataset(dataset);
            dataset.Add(DicomTag.PhotometricInterpretation, PhotometricInterpretation.Rgb.Value);
            dataset.Add(DicomTag.Rows, (ushort)rows);
            dataset.Add(DicomTag.Columns, (ushort)columns);
            dataset.AddOrUpdate(DicomTag.BitsAllocated, (ushort)8);
            DicomPixelData pixelData = DicomPixelData.Create(dataset, true);
            pixelData.BitsStored = 8;
            pixelData.BitsAllocated = 8;
            pixelData.SamplesPerPixel = 3;
            pixelData.HighBit = 7;
            pixelData.PixelRepresentation = 0;
            pixelData.PlanarConfiguration = 0;
            pixelData.AddFrame(buffer);

            DicomFile dicomfile = new DicomFile(dataset);
            dicomfile.Save("dicomfile.dcm");
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private static DicomUID GenerateUid()
        {
            StringBuilder uid = new StringBuilder();
            uid.Append("1.08.1982.10121984.2.0.07").Append('.').Append(DateTime.UtcNow.Ticks);
            return new DicomUID(uid.ToString(), "SOP Instance UID", DicomUidType.SOPInstance);
        }

        private static byte[] GetPixels(Bitmap image, out int rows, out int columns)
        {
            rows = image.Height;
            columns = image.Width;

            if (rows % 2 != 0 && columns % 2 != 0)
                --columns;

            BitmapData data = image.LockBits(new Rectangle(0, 0, columns, rows), ImageLockMode.ReadOnly, image.PixelFormat);
            IntPtr bmpData = data.Scan0;
            try
            {
                int stride = columns * 3;
                int size = rows * stride;
                byte[] pixelData = new byte[size];
                for (int i = 0; i < rows; ++i)
                    Marshal.Copy(new IntPtr(bmpData.ToInt64() + i * data.Stride), pixelData, i * stride, stride);

                //swap BGR to RGB
                SwapRedBlue(pixelData);
                return pixelData;
            }
            finally
            {
                image.UnlockBits(data);
            }
        }

        private static Bitmap GetValidImage(Bitmap bitmap)
        {
            if (bitmap.PixelFormat != PixelFormat.Format24bppRgb)
            {
                Bitmap old = bitmap;
                using (old)
                {
                    bitmap = new Bitmap(old.Width, old.Height, PixelFormat.Format24bppRgb);
                    using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bitmap))
                    {
                        g.DrawImage(old, 0, 0, old.Width, old.Height);
                    }
                }
            }
            return bitmap;
        }

        private static void SwapRedBlue(byte[] pixels)
        {
            for (int i = 0; i < pixels.Length; i += 3)
            {
                byte temp = pixels[i];
                pixels[i] = pixels[i + 2];
                pixels[i + 2] = temp;
            }
        }

        private void FillDataset(Dicom.DicomDataset dataset)
        {
            //type 1 attributes.
            dataset.Add(DicomTag.SOPClassUID, DicomUID.SecondaryCaptureImageStorage);
            dataset.Add(DicomTag.StudyInstanceUID, GenerateUid());
            dataset.Add(DicomTag.SeriesInstanceUID, GenerateUid());
            dataset.Add(DicomTag.SOPInstanceUID, GenerateUid());

            //type 2 attributes
            dataset.Add(DicomTag.PatientID, "12345");
            dataset.Add(DicomTag.PatientName, PatientName);
            dataset.Add(DicomTag.PatientBirthDate, PatientDateOfBirth);
            switch (patientSex)
            {
                case Sex.Male:
                    dataset.Add(DicomTag.PatientSex, "M");
                    break;

                case Sex.Female:
                    dataset.Add(DicomTag.PatientSex, "F");
                    break;

                case Sex.Other:
                    dataset.Add(DicomTag.PatientSex, "O");
                    break;

                default:
                    break;
            }
            dataset.Add(DicomTag.StudyDate, examinationDate);
            dataset.Add(DicomTag.StudyTime, examinationDate);
            dataset.Add(DicomTag.AccessionNumber, string.Empty);
            dataset.Add(DicomTag.ReferringPhysicianName, string.Empty);
            dataset.Add(DicomTag.StudyID, "1");
            dataset.Add(DicomTag.SeriesNumber, "1");
            dataset.Add(DicomTag.ModalitiesInStudy, "CR");
            dataset.Add(DicomTag.Modality, "CR");
            dataset.Add(DicomTag.NumberOfStudyRelatedInstances, "1");
            dataset.Add(DicomTag.NumberOfStudyRelatedSeries, "1");
            dataset.Add(DicomTag.NumberOfSeriesRelatedInstances, "1");
            dataset.Add(DicomTag.PatientOrientation, "F/A");
            dataset.Add(DicomTag.ImageLaterality, "U");
            dataset.Add(DicomTag.StudyDescription, Comment);
        }

        private void SaveToDICOM(object obj)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(recreatedImage));
                enc.Save(outStream);
                var bitmap = new Bitmap(outStream);
                ExportImage(bitmap);
            }

            //Application.Current.MainWindow.Close();
        }
    }
}