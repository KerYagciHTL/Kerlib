using Kerlib.Interfaces;
using Kerlib.Native;
using Kerlib.Window;

namespace Kerlib.Drawing;

public class Text : IRenderable
{
    private readonly int _x, _y;
    private readonly string _content;
    private readonly uint _color;
    private readonly string _fontName;
    private readonly int _fontSize;
    
    public Text(Point position, string content, Color color, string fontName = "Arial", int fontSize = 16)
    {
        _x = position.X;
        _y = position.Y;
        _content = content;
        _color = NativeMethods.Rgb(color.R, color.G, color.B);
        _fontName = fontName;
        _fontSize = fontSize;
    }

    public void Draw(IntPtr hdc)
    {
        NativeMethods.SetTextColor(hdc, _color);
        
        // Transparent background for text
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