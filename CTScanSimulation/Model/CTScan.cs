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
        private const int pointSize = 10; //in pixels
        private int centerX;
        private int centerY;
        private int emitterDetectorSystemStep;
        private int emitterDetectorSystemWidth;
        private int numberOfDetectors;
        private Bitmap orginalImage;
        private int radius;

        public CTScan(Bitmap orginalImage, int emitterDetectorSystemStep, int numberOfDetectors, int emitterDetectorSystemWidth)
        {
            this.orginalImage = orginalImage;
            this.emitterDetectorSystemStep = emitterDetectorSystemStep;
            this.numberOfDetectors = numberOfDetectors;
            this.emitterDetectorSystemWidth = emitterDetectorSystemWidth;

            centerX = orginalImage.Width / 2;
            centerY = orginalImage.Height / 2;

            radius = orginalImage.Height > orginalImage.Width ? orginalImage.Width : orginalImage.Height;
            radius = radius / 2 - padding;
        }

        public Bitmap CreateSinogram()
        {
            throw new NotImplementedException();
        }

        public Bitmap RecreateImage()
        {
            throw new NotImplementedException();
        }

        public Bitmap DrawCTSystem(int n)
        {
            n--;
            int angle = n * emitterDetectorSystemStep;
            if (angle > 360)
            {
                throw new ArgumentOutOfRangeException(nameof(n), "angle>360");
            }
            var result = new Bitmap(orginalImage);
            using (Graphics g = Graphics.FromImage(result))
            using (Pen redPen = new Pen(Color.Red, 2))
            using (Pen bluePen = new Pen(Color.CadetBlue, 2))
            {
                double radian = angle * 2 * Math.PI / 360;
                float emitterX = centerX - (float)(Math.Sin(radian) * radius);
                float emitterY = centerY - (float)(Math.Cos(radian) * radius);
                //draw main frame
                g.DrawEllipse(redPen, centerX - radius, centerY - radius, 2 * radius, 2 * radius);
                //draw center
                g.FillEllipse(Brushes.Blue, centerX - pointSize / 2, centerY - pointSize / 2, pointSize, pointSize);
                //draw emitter
                g.FillEllipse(Brushes.Blue, emitterX - pointSize / 2, emitterY - pointSize / 2, pointSize, pointSize);

                double detectorStep = emitterDetectorSystemWidth / numberOfDetectors;
                for (int i = 0; i < numberOfDetectors; i++)
                {
                    double detectorAngle = (angle + (180 - emitterDetectorSystemWidth / 2)) + i * detectorStep;
                    double detectorRad = detectorAngle * 2 * Math.PI / 360;
                    float detectorX = centerX - (float)(Math.Sin(detectorRad) * radius);
                    float detectorY = centerY - (float)(Math.Cos(detectorRad) * radius);
                    //draw detector
                    g.FillEllipse(Brushes.Blue, detectorX - pointSize / 2, detectorY - pointSize / 2, pointSize, pointSize);
                    //draw ray
                    g.DrawLine(bluePen, emitterX, emitterY, detectorX, detectorY);
                }
            }
            return result;
        }

        ~CTScan()
        {
            orginalImage.Dispose();
        }
    }
}