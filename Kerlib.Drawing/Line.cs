using Kerlib.Native;
using Kerlib.Window;

namespace Kerlib.Drawing;

public class Line : IRenderable
{
    private readonly int _x1, _y1, _x2, _y2;
    private readonly uint _color;

    public Line(Point a, Point b, Color color)
    {
        _x1 = a.X; _y1 = a.Y; _x2 = b.X; _y2 = b.Y;
        _color = NativeMethods.RGB(color.R, color.G, color.B);
    }

    public void Draw(IntPtr hdc)
    {
        var pen = NativeMethods.CreatePen(0, 1, _color);
        var oldPen = NativeMethods.SelectObject(hdc, pen);
        NativeMethods.MoveToEx(hdc, _x1, _y1, IntPtr.Zero);
        NativeMethods.LineTo(hdc, _x2, _y2);
        NativeMethods.SelectObject(hdc, oldPen);
        NativeMethods.DeleteObject(pen);
    }
}