using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;

namespace CTScanSimulation.Model
{
    /// <summary>
    /// Fast wrapper for <c>Bitmap</c>.
    /// </summary>
    public class DirectBitmap : IDisposable
    {
        private const int NUMBER_OF_CHANNELS = 4;
        private readonly Bitmap Bitmap;
        private readonly int[] Bits;
        private readonly GCHandle BitsHandle;

        /// <summary>
        /// Creates new instance of <c>DirectBitmap</c> with blank <c>Bitmap</c>.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public DirectBitmap(int width, int height)
        {
            Width = width;
            Height = height;
            Bits = new int[width * height];
            BitsHandle = GCHandle.Alloc(Bits, GCHandleType.Pinned);
            Bitmap = new Bitmap(width, height, width * NUMBER_OF_CHANNELS, PixelFormat.Format32bppPArgb, BitsHandle.AddrOfPinnedObject());
        }

        public DirectBitmap(Bitmap bitmap, bool dispose = false)
        {
            const PixelFormat pixelFormat = PixelFormat.Format32bppPArgb;

            int bitsPerPixel = Image.GetPixelFormatSize(pixelFormat);
            int bytesPerPixel = bitsPerPixel / 8;
            if (bytesPerPixel > 0) {
                Width = bitmap.Width;
                Height = bitmap.Height;
                Bits = new int[Width * Height];
                BitsHandle = GCHandle.Alloc(Bits, GCHandleType.Pinned);
                Bitmap = new Bitmap(Width, Height, Width * bytesPerPixel, pixelFormat, BitsHandle.AddrOfPinnedObject());
                var bitmapData = bitmap.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadOnly, pixelFormat);
                NativeMethods.CopyMemory(BitsHandle.AddrOfPinnedObject(), bitmapData.Scan0, Width * Height * bytesPerPixel);
                bitmap.UnlockBits(bitmapData);
                if (dispose)
                {
                    bitmap.Dispose();
                }
            }
            else throw new ArgumentException(@"The source PixelFormat is not supported.", nameof(bitmap));
        }

        public bool Disposed { get; private set; }
        public int Height { get; }
        public int Width { get; }

        /// <summary>
        /// Release resources.
        /// </summary>
        public void Dispose()
        {
            if (Disposed) return;
            Disposed = true;
            Bitmap.Dispose();
            BitsHandle.Free();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Gets <c>Color</c> of the specified pixel.
        /// </summary>
        /// <param name="x">Horizontal location.</param>
        /// <param name="y">Vertical location.</param>
        /// <returns>Color of pixel at (x,y).</returns>
        public Color GetPixel(int x, int y)
        {
            int index = y * Width + x;
            return Color.FromArgb(Bits[index]);
        }

        /// <summary>
        /// Save underlying <c>Bitmap</c> to file.
        /// </summary>
        /// <param name="filename">Path to file.</param>
        public void SaveToFile(string filename)
        {
            Bitmap.Save(filename);
        }

        /// <summary>
        /// Sets <c>Color</c> of the specified pixel.
        /// </summary>
        /// <param name="x">Horizontal location.</param>
        /// <param name="y">Vertical location.</param>
        /// <param name="color">Color to set.</param>
        public void SetPixel(int x, int y, Color color)
        {
            int index = y * Width + x;
            Bits[index] = color.ToArgb();
        }

        /// <summary>
        /// Return underlying <c>Bitmap</c> as a new <c>BitmapImage</c>.
        /// </summary>
        /// <returns></returns>
        public BitmapImage ToBitmapImage()
        {
            return BitmapToBitmapImage(Bitmap);
        }

        /// <summary>
        /// Creates new <c>Graphics</c> for underlying <c>Bitmap</c>.
        /// </summary>
        /// <returns><c>Graphics</c> for underlying <c>Bitmap</c>.</returns>
        public Graphics GetGraphics()
        {
            return Graphics.FromImage(Bitmap);
        }

        /// <summary>
        /// Converts provided <c>Bitmap</c> to <c>BitmapImage</c>
        /// </summary>
        /// <param name="bitmap"><c>Bitmap</c> to convert.</param>
        /// <returns><c>BitmapImage</c> created from provided bitmap.</returns>
        public static BitmapImage BitmapToBitmapImage(Image bitmap)
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

    internal static class NativeMethods {

        /// <summary>
        /// Copies memory fragment.
        /// </summary>
        /// <param name="dest">Destination pointer.</param>
        /// <param name="src">Source pointer.</param>
        /// <param name="count">Number of bytes to copy.</param>
        [DllImport("kernel32.dll", EntryPoint = "CopyMemory", SetLastError = false)]
        public static extern void CopyMemory(IntPtr dest, IntPtr src, int count);

    }


}