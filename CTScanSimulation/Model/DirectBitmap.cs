using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
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
            Bits = new Int32[width * height];
            BitsHandle = GCHandle.Alloc(Bits, GCHandleType.Pinned);
            Bitmap = new Bitmap(width, height, width * NUMBER_OF_CHANNELS, PixelFormat.Format32bppPArgb, BitsHandle.AddrOfPinnedObject());
        }

        /// <summary>
        /// Creates new instance of <c>DirectBitmap</c> with provided <c>Bitmap</c>.
        /// Do not call <c>Dispose()</c> on <c>Bitmap</c> you provided as parameter.
        /// </summary>
        /// <param name="bitmap"></param>
        public DirectBitmap(Bitmap bitmap)
        {
            Width = bitmap.Width;
            Height = bitmap.Height;
            Bits = new Int32[Width * Height];
            BitsHandle = GCHandle.Alloc(Bits, GCHandleType.Pinned);
            Bitmap = bitmap;
        }

        /// <summary>
        /// Creates new instance of <c>DirectBitmap</c> from file.
        /// </summary>
        /// <param name="filename">Path to file.</param>
        public DirectBitmap(string filename)
        {
            Bitmap = (Bitmap)Image.FromFile(filename);
            Width = Bitmap.Width;
            Height = Bitmap.Height;
            Bits = new Int32[Width * Height];
            BitsHandle = GCHandle.Alloc(Bits, GCHandleType.Pinned);
        }

        public bool Disposed { get; private set; }
        public int Height { get; private set; }
        public int Width { get; private set; }

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
        ///
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
        ///
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
            return CtScan.BitmapToBitmapImage(Bitmap);
        }
    }
}