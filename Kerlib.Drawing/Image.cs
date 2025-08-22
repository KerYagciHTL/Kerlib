using Kerlib.Interfaces;
using Kerlib.Native;
using System.Drawing;
using Point = Kerlib.Native.Point;

namespace Kerlib.Drawing
{
    public class Image : IImage
    {
        private int _x, _y;
        private int _width, _height;
        private IntPtr _hBitmap;
        private IntPtr _hBitmapOriginal = IntPtr.Zero;

        public event EventHandler? Changed;
        
        public Point Position
        {
            get => new(_x, _y);
            set
            {
                if (_x == value.X && _y == value.Y) return;
                _x = value.X;
                _y = value.Y;
                Changed?.Invoke(this, EventArgs.Empty);
            }
        }
        public int Width 
        {
            get => _width;
            set
            {
                if (_width == value) return;
                _width = value;
                Changed?.Invoke(this, EventArgs.Empty);
            }
        }
        public int Height 
        {
            get => _height;
            set
            {
                if (_height == value) return;
                _height = value;
                Changed?.Invoke(this, EventArgs.Empty);
            }
        }

        public string Path { get; }

        public Image(Point position, string path, int? width = null, int? height = null)
        {
            _x = position.X;
            _y = position.Y;
            Path = path;

            using (var img = System.Drawing.Image.FromFile(path))
            {
                _width = width ?? img.Width;
                _height = height ?? img.Height;

                using (var resized = new Bitmap(img, _width, _height))
                {
                    _hBitmap = resized.GetHbitmap();
                }
            }

            if (_hBitmap == IntPtr.Zero)
                throw new InvalidOperationException($"Konnte Bild nicht laden: {Path}");
        }

        public void Draw(IntPtr hdc)
        {
            if (_hBitmap == IntPtr.Zero) return;

            var hdcMem = NativeMethods.CreateCompatibleDC(hdc);
            _hBitmapOriginal = NativeMethods.SelectObject(hdcMem, _hBitmap);

            NativeMethods.BitBlt(
                hdc,
                _x, _y, _width, _height,
                hdcMem,
                0, 0,
                NativeMethods.Srccopy);

            NativeMethods.SelectObject(hdcMem, _hBitmapOriginal);
            NativeMethods.DeleteDC(hdcMem);
        }

        public void Dispose()
        {
            if (_hBitmap == IntPtr.Zero) return;
            NativeMethods.DeleteObject(_hBitmap);
            _hBitmap = IntPtr.Zero;
        }
    }
}