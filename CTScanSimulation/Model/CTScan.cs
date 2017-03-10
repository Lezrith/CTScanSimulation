using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;

namespace CTScanSimulation.Model
{
    public struct Pixel
    {
        private readonly int x;
        public int X { get { return x; } }

        private readonly int y;
        public int Y { get { return y; } }

        public Pixel(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    public struct PixelColor
    {
        public byte B;
        public byte G;
        public byte R;
        public byte A;

        public PixelColor(byte r, byte g, byte b, byte a)
        {
            this.R = r;
            this.G = g;
            this.B = b;
            this.A = a;
        }
    }

    public class CtScan
    {
        private const int padding = 5;
        private const int pointSize = 10; // In pixels
        private readonly int centerX;
        private readonly int centerY;
        private readonly float detectorStep;
        private readonly float emitterDetectorSystemStep;
        private readonly int emitterDetectorSystemWidth;
        private readonly int numberOfDetectors;
        private readonly int radius;
        private readonly Bitmap orginalImage;
        private readonly Bitmap sinogram;
        private Bitmap recreatedImage;
        private readonly long[,] rawData;

        public CtScan(Bitmap orginalImage, float emitterDetectorSystemStep, int numberOfDetectors, int emitterDetectorSystemWidth)
        {
            ConvertToGreyscale(orginalImage);
            this.orginalImage = orginalImage;
            this.emitterDetectorSystemStep = emitterDetectorSystemStep;
            this.numberOfDetectors = numberOfDetectors;
            this.emitterDetectorSystemWidth = emitterDetectorSystemWidth;
            recreatedImage = new Bitmap(orginalImage.Width, orginalImage.Height);
            detectorStep = (float)emitterDetectorSystemWidth / (numberOfDetectors - 1);
            sinogram = new Bitmap(numberOfDetectors, (int)Math.Floor(360 / emitterDetectorSystemStep));
            rawData = new long[orginalImage.Width, orginalImage.Height];

            centerX = orginalImage.Width / 2;
            centerY = orginalImage.Height / 2;

            radius = orginalImage.Height > orginalImage.Width ? orginalImage.Width : orginalImage.Height;
            radius = radius / 2 - padding;
        }

        ~CtScan()
        {
            orginalImage.Dispose();
            sinogram.Dispose();
            recreatedImage.Dispose();
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

        [SuppressMessage("ReSharper", "PossibleLossOfFraction")]
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
                float emitterX = centerX - (float)(Math.Cos(radian) * radius);
                float emitterY = centerY - (float)(Math.Sin(radian) * radius);
                // Draw main frame
                g.DrawEllipse(redPen, centerX - radius, centerY - radius, 2 * radius, 2 * radius);
                // Draw center
                g.FillEllipse(Brushes.Blue, centerX - pointSize / 2, centerY - pointSize / 2, pointSize, pointSize);
                // Draw emitter
                g.FillEllipse(Brushes.Blue, emitterX - pointSize / 2, emitterY - pointSize / 2, pointSize, pointSize);

                for (int i = 0; i < numberOfDetectors; i++)
                {
                    double detectorAngle = angle + (180 - emitterDetectorSystemWidth / 2) + i * detectorStep;
                    double detectorRad = detectorAngle * 2 * Math.PI / 360;
                    float detectorX = centerX - (float)(Math.Cos(detectorRad) * radius);
                    float detectorY = centerY - (float)(Math.Sin(detectorRad) * radius);
                    // Draw detector
                    g.FillEllipse(Brushes.Blue, detectorX - pointSize / 2, detectorY - pointSize / 2, pointSize, pointSize);
                    // Draw ray
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

        public BitmapImage RecreateImage(int n)
        {
            RestoreImageBySinogramRow(n);
            return BitmapToBitmapImage(recreatedImage);
        }

        private static BitmapImage BitmapToBitmapImage(Image bitmap)
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

        private static Color RgbToGreyscale(Color c)
        {
            int grey = (c.R + c.G + c.B) / 3;
            return Color.FromArgb(grey, grey, grey);
        }

        public static long Scale(long originalStart, long originalEnd, // Original range
                                 long newStart, long newEnd,           // Desired range
                                 long value)                           // Value to convert
        {
            double scale = (double)(newEnd - newStart) / (originalEnd - originalStart);
            return (long)(newStart + (value - originalStart) * scale);
        }

        private void CreateSinogramRow(int row)
        {
            double angle = row * emitterDetectorSystemStep;
            double radian = angle * 2 * Math.PI / 360;
            int emitterX = (int)(centerX - Math.Cos(radian) * radius);
            int emitterY = (int)(centerY - Math.Sin(radian) * radius);

            for (int detectorNo = 0; detectorNo < numberOfDetectors; detectorNo++)
            {
                double detectorAngle = angle + (180 - emitterDetectorSystemWidth / 2) + detectorNo * detectorStep;
                double detectorRad = detectorAngle * 2 * Math.PI / 360;
                int detectorX = (int)(centerX - Math.Cos(detectorRad) * radius);
                int detectorY = (int)(centerY - Math.Sin(detectorRad) * radius);

                IEnumerable<Pixel> pixelsToSum = GetPixelsFromBresenhamLine(emitterX, emitterY, detectorX, detectorY);
                var toSum = pixelsToSum as IList<Pixel> ?? pixelsToSum.ToList();
                long sum = toSum.AsParallel().Aggregate(0, (current, pixel) => current + orginalImage.GetPixel(pixel.X, pixel.Y).R);
                // Normalization
                int normalizedSum = (int)(sum / toSum.ToList().Count);
                if (row < sinogram.Height)
                    sinogram.SetPixel(detectorNo, row, Color.FromArgb(normalizedSum, normalizedSum, normalizedSum));
            }
        }

        private void CreateBitmapFromRawData()
        {
            // Create new bitmap
            recreatedImage.Dispose();
            recreatedImage = new Bitmap(orginalImage.Width, orginalImage.Height);

            // Find the maximum value in the array
            long maxValue = rawData.Cast<long>().Concat(new long[] { 0 }).Max();

            BitmapData locked = recreatedImage.LockBits(new Rectangle(0, 0, recreatedImage.Width, recreatedImage.Height),
                                                        ImageLockMode.ReadWrite,
                                                        PixelFormat.Format32bppArgb);
            unsafe
            {
                PixelColor* pixelPtr = (PixelColor*)(void*)locked.Scan0;

                // Normalize
                for (int x = 0; x < orginalImage.Width; x++)
                {
                    for (int y = 0; y < orginalImage.Height; y++)
                    {
                        byte color = (byte)Scale(0, maxValue, 0, 255, rawData[x, y]);
                        if (color != 0)
                        {
                            Console.WriteLine(color + @"at " + x + @"," + y);
                        }
                        *pixelPtr = new PixelColor(color, color, color, 255);
                        pixelPtr++;
                    }
                }
            }

            recreatedImage.UnlockBits(locked);
        }

        private void RestoreImageBySinogramRow(int row)
        {
            double angle = row * emitterDetectorSystemStep;
            double radian = angle * 2 * Math.PI / 360;
            int emitterX = (int)(centerX - Math.Cos(radian) * radius);
            int emitterY = (int)(centerY - Math.Sin(radian) * radius);

            for (int detector = 0; detector < numberOfDetectors; detector++)
            {
                double detectorAngle = angle + (180 - emitterDetectorSystemWidth / 2) + detector * detectorStep;
                double detectorRad = detectorAngle * 2 * Math.PI / 360;
                int detectorX = (int)(centerX - Math.Cos(detectorRad) * radius);
                int detectorY = (int)(centerY - Math.Sin(detectorRad) * radius);

                Color colorToApply = sinogram.GetPixel(detector, row);
                if (colorToApply.R != 0)
                {
                    Console.WriteLine("Color " + colorToApply.R + " at row " + row + " detector " + detector);
                }
                IEnumerable<Pixel> pixels = GetPixelsFromBresenhamLine(emitterX, emitterY, detectorX, detectorY);

                // Add colors
                foreach (Pixel pixel in pixels)
                {
                    rawData[pixel.X, pixel.Y] += colorToApply.R;
                }
            }

            CreateBitmapFromRawData();
        }

        private static IEnumerable<Pixel> GetPixelsFromBresenhamLine(int x1, int y1, int x2, int y2)
        {
            // Zmienne pomocnicze
            List<Pixel> pixels = new List<Pixel>();
            int d, dx, dy, ai, bi, xi, yi;
            int x = x1, y = y1;
            // Ustalenie kierunku rysowania
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
            // Ustalenie kierunku rysowania
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
            // Pierwszy piksel
            pixels.Add(new Pixel(x, y));
            // Oś wiodąca OX
            if (dx > dy)
            {
                ai = (dy - dx) * 2;
                bi = dy * 2;
                d = bi - dx;
                // Pętla po kolejnych X
                while (x != x2)
                {
                    // Test współczynnika
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
                    pixels.Add(new Pixel(x, y));
                }
            }
            // Oś wiodąca OY
            else
            {
                ai = (dx - dy) * 2;
                bi = dx * 2;
                d = bi - dy;
                //Ppętla po kolejnych Y
                while (y != y2)
                {
                    // Test współczynnika
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
                    pixels.Add(new Pixel(x, y));
                }
            }
            return pixels;
        }
    }
}