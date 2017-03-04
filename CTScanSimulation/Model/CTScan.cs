using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;

namespace CTScanSimulation.Model
{
    public class CtScan
    {
        private const int padding = 5;
        private const int pointSize = 10; //in pixels
        private readonly int centerX;
        private readonly int centerY;
        private readonly float detectorStep;
        private readonly float emitterDetectorSystemStep;
        private readonly int emitterDetectorSystemWidth;
        private readonly int numberOfDetectors;
        private readonly Bitmap orginalImage;
        private readonly int radius;
        private readonly Bitmap sinogram;
        private readonly Bitmap recreatedImage;

        public CtScan(Bitmap orginalImage, float emitterDetectorSystemStep, int numberOfDetectors, int emitterDetectorSystemWidth)
        {
            ConvertToGreyscale(orginalImage);
            this.orginalImage = orginalImage;
            this.emitterDetectorSystemStep = emitterDetectorSystemStep;
            this.numberOfDetectors = numberOfDetectors;
            this.emitterDetectorSystemWidth = emitterDetectorSystemWidth;
            detectorStep = (float)emitterDetectorSystemWidth / numberOfDetectors;
            sinogram = new Bitmap((int)Math.Floor(360 / emitterDetectorSystemStep), numberOfDetectors);

            centerX = orginalImage.Width / 2;
            centerY = orginalImage.Height / 2;

            radius = orginalImage.Height > orginalImage.Width ? orginalImage.Width : orginalImage.Height;
            radius = radius / 2 - padding;
        }

        ~CtScan()
        {
            orginalImage.Dispose();
            sinogram.Dispose();
        }

        public BitmapImage CreateSinogram(int n)
        {
            CreateSinogramRow(n);
            return BitmapToBitmapImage(sinogram);
        }

        public BitmapImage CreateSinogram()
        {
            int steps = (int)Math.Floor(360 / emitterDetectorSystemStep);
            for (int i = 0; i < steps; i++)
            {
                CreateSinogramRow(i);
            }
            return BitmapToBitmapImage(sinogram);
        }

        public BitmapImage DrawCtSystem(int n)
        {
            double angle = n * emitterDetectorSystemStep;
            if (angle > 360)
            {
                throw new ArgumentOutOfRangeException(nameof(n), @"angle>360");
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
            return BitmapToBitmapImage(result);
        }

        public BitmapImage RecreateImage()
        {
            for (int row = 0; row < sinogram.Height; row++)
            {
                RestoreImageBySinogramRow(row);
            }

            return BitmapToBitmapImage(recreatedImage);
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

        private static void ConvertToGreyscale(Bitmap bitmap)
        {
            for (int i = 0; i < bitmap.Height; i++)
            {
                for (int j = 0; j < bitmap.Width; j++)
                {
                    var grey = RgbToGreyscale(bitmap.GetPixel(j, i));
                    bitmap.SetPixel(j, i, grey);
                }
            }
        }

        private void CreateSinogramRow(int n)
        {
            double angle = n * emitterDetectorSystemStep;
            double radian = angle * 2 * Math.PI / 360;
            int emitterX = (int)(centerX - Math.Sin(radian) * radius);
            int emitterY = (int)(centerY - Math.Cos(radian) * radius);

            for (int i = 0; i < numberOfDetectors; i++)
            {
                double detectorAngle = angle + (180 - emitterDetectorSystemWidth / 2) + i * detectorStep;
                double detectorRad = detectorAngle * 2 * Math.PI / 360;
                int detectorX = (int)(centerX - Math.Sin(detectorRad) * radius);
                int detectorY = (int)(centerY - Math.Cos(detectorRad) * radius);

                int sum = SumAlongBresenhamLine(emitterX, emitterY, detectorX, detectorY);
                sum /= 2 * radius;
                if (n < sinogram.Width) sinogram.SetPixel(n, i, Color.FromArgb(sum, sum, sum));
            }
        }

        private void RestoreImageBySinogramRow(int row)
        {
            double angle = row * emitterDetectorSystemStep;
            double radian = angle * 2 * Math.PI / 360;
            int emitterX = (int)(centerX - Math.Sin(radian) * radius);
            int emitterY = (int)(centerY - Math.Cos(radian) * radius);

            for (int detector = 0; detector < numberOfDetectors; detector++)
            {
                double detectorAngle = angle + (180 - emitterDetectorSystemWidth / 2) + detector * detectorStep;
                double detectorRad = detectorAngle * 2 * Math.PI / 360;
                int detectorX = (int)(centerX - Math.Sin(detectorRad) * radius);
                int detectorY = (int)(centerY - Math.Cos(detectorRad) * radius);

                Color colorToApply = sinogram.GetPixel(row, detector);

                ColorAlongBresenhamLine(emitterX, emitterY, detectorX, detectorY, colorToApply);
            }
        }

        private static Color RgbToGreyscale(Color c)
        {
            int grey = (c.R + c.G + c.B) / 3;
            return Color.FromArgb(grey, grey, grey);
        }

        private int SumAlongBresenhamLine(int x1, int y1, int x2, int y2)
        {
            // zmienne pomocnicze

            int d, dx, dy, ai, bi, xi, yi;
            int x = x1, y = y1;
            int result = 0;
            // ustalenie kierunku rysowania
            if (x1 < x2)
            {
                xi = 1;
                dx = x2 - x1;
            }
            else
            {
                xi = -1;
                dx = x1 - x2;
            }

            // ustalenie kierunku rysowania
            if (y1 < y2)
            {
                yi = 1;
                dy = y2 - y1;
            }
            else
            {
                yi = -1;
                dy = y1 - y2;
            }
            // pierwszy piksel
            //glVertex2i(x, y)
            result += orginalImage.GetPixel(x, y).R;
            // oś wiodąca OX
            if (dx > dy)
            {
                ai = (dy - dx) * 2;
                bi = dy * 2;
                d = bi - dx;
                // pętla po kolejnych x
                while (x != x2)
                {
                    // test współczynnika
                    if (d >= 0)
                    {
                        x += xi;
                        y += yi;
                        d += ai;
                    }
                    else
                    {
                        d += bi;
                        x += xi;
                    }
                    //glVertex2i(x, y);
                    result += orginalImage.GetPixel(x, y).R;
                }
            }
            // oś wiodąca OY
            else
            {
                ai = (dx - dy) * 2;
                bi = dx * 2;
                d = bi - dy;
                // pętla po kolejnych y
                while (y != y2)
                {
                    // test współczynnika
                    if (d >= 0)
                    {
                        x += xi;
                        y += yi;
                        d += ai;
                    }
                    else
                    {
                        d += bi;
                        y += yi;
                    }
                    //glVertex2i(x, y);
                    result += orginalImage.GetPixel(x, y).R;
                }
            }
            return result;
        }

        private void ColorAlongBresenhamLine(int x1, int y1, int x2, int y2, Color colorToApply)
        {
            // zmienne pomocnicze

            int d, dx, dy, ai, bi, xi, yi;
            int x = x1, y = y1;
            // ustalenie kierunku rysowania
            if (x1 < x2)
            {
                xi = 1;
                dx = x2 - x1;
            }
            else
            {
                xi = -1;
                dx = x1 - x2;
            }

            // ustalenie kierunku rysowania
            if (y1 < y2)
            {
                yi = 1;
                dy = y2 - y1;
            }
            else
            {
                yi = -1;
                dy = y1 - y2;
            }
            // pierwszy piksel
            //glVertex2i(x, y)
            recreatedImage.SetPixel(x, y, colorToApply);
            // oś wiodąca OX
            if (dx > dy)
            {
                ai = (dy - dx) * 2;
                bi = dy * 2;
                d = bi - dx;
                // pętla po kolejnych x
                while (x != x2)
                {
                    // test współczynnika
                    if (d >= 0)
                    {
                        x += xi;
                        y += yi;
                        d += ai;
                    }
                    else
                    {
                        d += bi;
                        x += xi;
                    }
                    //glVertex2i(x, y);
                    recreatedImage.SetPixel(x, y, colorToApply);
                }
            }
            // oś wiodąca OY
            else
            {
                ai = (dx - dy) * 2;
                bi = dx * 2;
                d = bi - dy;
                // pętla po kolejnych y
                while (y != y2)
                {
                    // test współczynnika
                    if (d >= 0)
                    {
                        x += xi;
                        y += yi;
                        d += ai;
                    }
                    else
                    {
                        d += bi;
                        y += yi;
                    }
                    //glVertex2i(x, y);
                    recreatedImage.SetPixel(x, y, colorToApply);
                }
            }
        }

    }
}