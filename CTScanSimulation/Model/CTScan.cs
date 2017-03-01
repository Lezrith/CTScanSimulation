using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace CTScanSimulation.Model
{
    public class CTScan
    {
        private const int padding = 5;
        private int centerX;
        private int centerY;
        private int emiterDetectorSystemStep;
        private int emiterDetectorSystemWidth;
        private int numberOfDetectors;
        private Bitmap orginalImage;
        private int radius;

        public CTScan(Bitmap orginalImage, int emiterDetectorSystemStep, int numberOfDetectors, int emiterDetectorSystemWidth)
        {
            this.orginalImage = orginalImage;
            this.emiterDetectorSystemStep = emiterDetectorSystemStep;
            this.numberOfDetectors = numberOfDetectors;
            this.emiterDetectorSystemWidth = emiterDetectorSystemWidth;

            centerX = orginalImage.Width / 2;
            centerY = orginalImage.Height / 2;

            radius = orginalImage.Height < orginalImage.Width ? orginalImage.Width : orginalImage.Height;
            radius = radius / 2 - padding;
        }

        public BitmapImage CreateSinogram()
        {
            throw new NotImplementedException();
        }

        public BitmapImage RecreateImage()
        {
            throw new NotImplementedException();
        }
    }
}