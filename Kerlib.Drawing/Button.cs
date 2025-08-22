using Kerlib.Interfaces;
using Kerlib.Native;

namespace Kerlib.Drawing;

public class Button : IButton, IDisposable
{
    private Point _position;
    private int _width, _height;
    private bool _hovered;
    private bool _pressed;
    private bool _disposed;

    public event EventHandler? Changed;
    public event EventHandler? Clicked;
    public event EventHandler? MouseEnter;
    public event EventHandler? MouseLeave;
    public event EventHandler? MouseDown;
    public event EventHandler? MouseUp;

    public Point Position
    {
        get => _position;
        set
        {
            if (ReferenceEquals(_position, value)) return;
            _position.Changed -= OnPositionChanged;
            _position = value ?? throw new ArgumentNullException(nameof(value));
            _position.Changed += OnPositionChanged;
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

    public Color BackgroundNormal { get; set; }
    public Color BackgroundHover { get; set; }
    public Color BackgroundPressed { get; set; }
    public Color Foreground { get; set; }
    public string Text { get; set; }

    public bool IsHovered => _hovered;
    public bool IsPressed => _pressed;

    public Button(Point position, int width, int height, string text)
    {
        _position = position ?? throw new ArgumentNullException(nameof(position));
        _position.Changed += OnPositionChanged;
        _width = width;
        _height = height;
        Text = text;

        BackgroundNormal = new Color(200, 200, 200);
        BackgroundHover = new Color(180, 180, 180);
        BackgroundPressed = new Color(160, 160, 160);
        Foreground = new Color(0, 0, 0);
    }

    private void OnPositionChanged(object? sender, EventArgs e) => Changed?.Invoke(this, EventArgs.Empty);

    public void Draw(IntPtr hdc)
    {
        if (_disposed) return;

        var bgColor = _pressed ? BackgroundPressed : _hovered ? BackgroundHover : BackgroundNormal;

        var brush = GdiCache.GetOrCreateBrush(NativeMethods.Rgb(bgColor));
        var oldBrush = NativeMethods.SelectObject(hdc, brush);

        var pen = GdiCache.GetOrCreatePen(1, NativeMethods.Rgb(new Color(0,0,0)));
        var oldPen = NativeMethods.SelectObject(hdc, pen);

        NativeMethods.Rectangle(hdc, _position.X, _position.Y, _position.X + _width, _position.Y + _height);

        NativeMethods.SelectObject(hdc, oldBrush);
        NativeMethods.SelectObject(hdc, oldPen);

        var rect = new NativeMethods.Rect
        {
            left = _position.X,
            top = _position.Y,
            right = _position.X + _width,
            bottom = _position.Y + _height
        };

        NativeMethods.SetTextColor(hdc, NativeMethods.Rgb(Foreground));
        NativeMethods.SetBkMode(hdc, 1); // TRANSPARENT
        NativeMethods.DrawText(hdc, Text, Text.Length, ref rect,
            NativeMethods.DtCenter | NativeMethods.DtVcenter | NativeMethods.DtSingleline);
    }

    public bool HandleMouseMove(int x, int y)
    {
        var inside = Contains(x, y);
        switch (inside)
        {
            case true when !_hovered:
                _hovered = true;
                MouseEnter?.Invoke(this, EventArgs.Empty);
                return true;
            case false when _hovered:
                _hovered = false;
                MouseLeave?.Invoke(this, EventArgs.Empty);
                return true;
            default:
                return false;
        }
    }

    public void HandleMouseDown(int x, int y)
    {
        if (!Contains(x, y)) return;
        _pressed = true;
        MouseDown?.Invoke(this, EventArgs.Empty);
    }

    public void HandleMouseUp(int x, int y)
    {
        if (!_pressed) return;
        _pressed = false;
        MouseUp?.Invoke(this, EventArgs.Empty);
        if (Contains(x, y))
            Clicked?.Invoke(this, EventArgs.Empty);
    }

    private bool Contains(int x, int y) =>
        x >= _position.X && x <= _position.X + _width && y >= _position.Y && y <= _position.Y + _height;

    public void Dispose()
    {
        if (_disposed) return;
        _position.Changed -= OnPositionChanged;

        Clicked = null;
        MouseEnter = null;
        MouseLeave = null;
        MouseDown = null;
        MouseUp = null;
        Changed = null;

        _disposed = true;
    }
}
