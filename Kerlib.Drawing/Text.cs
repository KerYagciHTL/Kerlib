using Kerlib.Interfaces;
using Kerlib.Native;
using Kerlib.Window;

namespace Kerlib.Drawing;

public class Text : IRenderable, INotifyRenderableChanged
{
    public event EventHandler? Changed;

    public string Content
    { 
        get => _content;
        set
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException("Content cannot be null or empty.", nameof(value));
            _content = value;
            Changed?.Invoke(this, EventArgs.Empty);
        }
    }

    private readonly int _x, _y;
    private string _content;
    private readonly uint _color;
    private readonly string _fontName;
    private readonly int _fontSize;
    
    public Text(Point position, string content, Color color, string fontName = "Arial", int fontSize = 16)
    {
        _x = position.X;
        _y = position.Y;
        _content = content;
        _color = NativeMethods.Rgb(color);
        _fontName = fontName;
        _fontSize = fontSize;
    }

    public void Draw(IntPtr hdc)
    {
        NativeMethods.SetTextColor(hdc, _color);
        NativeMethods.SetBkMode(hdc, 1);

        IntPtr hFont = NativeMethods.CreateFont(
            -_fontSize, 0, 0, 0, 400,
            0, 0, 0, 1, 0, 0, 0, 0,
            _fontName);

        IntPtr oldFont = NativeMethods.SelectObject(hdc, hFont);

        NativeMethods.TextOutW(hdc, _x, _y, _content, _content.Length);

        NativeMethods.SelectObject(hdc, oldFont);
        NativeMethods.DeleteObject(hFont);
    }
}