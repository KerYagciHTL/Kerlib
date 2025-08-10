using Kerlib.Native;
using Kerlib.Window;

namespace Kerlib.Drawing;

public class Line : IRenderable
{
    private readonly int _x1, _y1, _x2, _y2;
    private readonly uint _color;

    public Line(int x1, int y1, int x2, int y2, byte r = 0, byte g = 0, byte b = 0)
    {
        _x1 = x1; _y1 = y1; _x2 = x2; _y2 = y2;
        _color = NativeMethods.RGB(r, g, b);
    }

    public void Draw(IntPtr hdc)
    {
        IntPtr pen = NativeMethods.CreatePen(0, 1, _color);
        IntPtr oldPen = NativeMethods.SelectObject(hdc, pen);
        NativeMethods.MoveToEx(hdc, _x1, _y1, IntPtr.Zero);
        NativeMethods.LineTo(hdc, _x2, _y2);
        NativeMethods.SelectObject(hdc, oldPen);
        NativeMethods.DeleteObject(pen);
    }
}