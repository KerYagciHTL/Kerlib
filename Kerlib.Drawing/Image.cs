using Kerlib.Interfaces;
using Kerlib.Native;
using System.Drawing;
using Point = Kerlib.Native.Point;

namespace Kerlib.Drawing;

public class Image : IImage
{
    private int _width, _height;
    private Point _position;
    private IntPtr _hBitmap;
    private bool _disposed;

    public event EventHandler? Changed;

    public Point Position
    {
        get => _position;
        set
        {
            if (ReferenceEquals(_position, value)) return;
            _position.Changed -= PositionChanged;
            _position = value ?? throw new ArgumentNullException(nameof(value));
            _position.Changed += PositionChanged;
            Changed?.Invoke(this, EventArgs.Empty);
        }
    }

    private void PositionChanged(object? sender, EventArgs e)
    {
        Changed?.Invoke(this, EventArgs.Empty);
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
        _position = position ?? throw new ArgumentNullException(nameof(position));
        _position.Changed += PositionChanged;
        Path = path ?? throw new ArgumentNullException(nameof(path));

        using var img = System.Drawing.Image.FromFile(path);
        _width = width ?? img.Width;
        _height = height ?? img.Height;

        using var resized = new Bitmap(img, _width, _height);
        _hBitmap = resized.GetHbitmap();

        if (_hBitmap == IntPtr.Zero)
            throw new InvalidOperationException($"Picture could not be loaded: {Path}");
    }

    public void Draw(IntPtr hdc)
    {
        if (_disposed || _hBitmap == IntPtr.Zero) return;

        var hdcMem = NativeMethods.CreateCompatibleDC(hdc);
        var hBitmapOriginal = NativeMethods.SelectObject(hdcMem, _hBitmap);

        NativeMethods.BitBlt(
            hdc,
            _position.X, _position.Y, _width, _height,
            hdcMem,
            0, 0,
            NativeMethods.Srccopy);

        NativeMethods.SelectObject(hdcMem, hBitmapOriginal);
        NativeMethods.DeleteDC(hdcMem);
    }

    public void Dispose()
    {
        if (_disposed) return;

        _position.Changed -= PositionChanged;

        if (_hBitmap != IntPtr.Zero)
        {
            NativeMethods.DeleteObject(_hBitmap);
            _hBitmap = IntPtr.Zero;
        }

        Changed = null;
        _disposed = true;
    }
}
