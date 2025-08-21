using Kerlib.Interfaces;
using Kerlib.Native;

namespace Kerlib.Drawing
{
    [Obsolete("Only support for bmp files")]
    public class Image : IRenderable, IDisposable
    {
        private readonly int _x, _y;
        private readonly int _width, _height;
        private IntPtr _hBitmap;
        private IntPtr _hBitmapOriginal = IntPtr.Zero;

        public string Path { get; }

        public Image(Point position, string path, int? width = null, int? height = null)
        {
            _x = position.X;
            _y = position.Y;
            Path = path;

            _hBitmap = NativeMethods.LoadImage(
                IntPtr.Zero,
                Path,
                NativeMethods.ImageBitmap,
                0,
                0,
                NativeMethods.LrLoadfromfile);

            if (_hBitmap == IntPtr.Zero)
                throw new InvalidOperationException($"Image could not be found: {Path}");

            NativeMethods.Bitmap bmp;
            NativeMethods.GetObject(_hBitmap, System.Runtime.InteropServices.Marshal.SizeOf(typeof(NativeMethods.Bitmap)), out bmp);

            _width = width ?? bmp.bmWidth;
            _height = height ?? bmp.bmHeight;
        }

        public void Draw(IntPtr hdc)
        {
            if (_hBitmap == IntPtr.Zero) return;

            IntPtr hdcMem = NativeMethods.CreateCompatibleDC(hdc);
            _hBitmapOriginal = NativeMethods.SelectObject(hdcMem, _hBitmap);

            NativeMethods.StretchBlt(
                hdc,
                _x, _y, _width, _height,
                hdcMem,
                0, 0,
                _width, _height,
                NativeMethods.Srccopy);

            NativeMethods.SelectObject(hdcMem, _hBitmapOriginal);
            NativeMethods.DeleteDC(hdcMem);
        }

        public void Dispose()
        {
            if (_hBitmap != IntPtr.Zero)
            {
                NativeMethods.DeleteObject(_hBitmap);
                _hBitmap = IntPtr.Zero;
            }
        }
    }
}
