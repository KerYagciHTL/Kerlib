﻿using Kerlib.Interfaces;
using Kerlib.Native;

public sealed class Rectangle : IRenderable
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
        GraphicsContext.DrawRectangle(hdc, _left, _top, _right, _bottom, _color);
    }
}