using Kerlib.Interfaces;
using Kerlib.Native;

public class Rectangle : IRenderable
{
    private readonly int _left, _top, _right, _bottom;
    private readonly uint _color;

    public Rectangle(Point position, int width, int height, Color color)
    {
        _left = position.X;
        _top = position.Y;
        _right = position.X + width;
        _bottom = position.Y + height;
        _color = NativeMethods.Rgb(color);
    }

    public void Draw(IntPtr hdc)
    {
        var pen = GdiCache.GetOrCreatePen(1, _color);
        var oldPen = NativeMethods.SelectObject(hdc, pen);
        NativeMethods.Rectangle(hdc, _left, _top, _right, _bottom);
        NativeMethods.SelectObject(hdc, oldPen);
    }
}