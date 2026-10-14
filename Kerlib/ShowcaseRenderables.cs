using System.Diagnostics;
using System.Runtime.InteropServices;
using Kerlib.Interfaces;
using Kerlib.Native;

namespace Kerlib;

internal static class Gdi
{
    [DllImport("gdi32.dll")]
    public static extern bool RoundRect(IntPtr hdc, int left, int top, int right, int bottom, int width, int height);

    [DllImport("gdi32.dll")]
    public static extern bool Ellipse(IntPtr hdc, int left, int top, int right, int bottom);
}

/// <summary>Filled, optionally rounded and bordered rectangle.</summary>
internal sealed class Panel(int x, int y, int width, int height, Color fill, Color? border = null, int radius = 0) : IRenderable
{
    public void Draw(IntPtr hdc)
    {
        var brush = GdiCache.GetOrCreateBrush(NativeMethods.Rgb(fill));
        var pen = GdiCache.GetOrCreatePen(1, NativeMethods.Rgb(border ?? fill));
        var oldBrush = NativeMethods.SelectObject(hdc, brush);
        var oldPen = NativeMethods.SelectObject(hdc, pen);

        if (radius > 0)
            Gdi.RoundRect(hdc, x, y, x + width, y + height, radius, radius);
        else
            NativeMethods.Rectangle(hdc, x, y, x + width, y + height);

        NativeMethods.SelectObject(hdc, oldBrush);
        NativeMethods.SelectObject(hdc, oldPen);
    }
}

/// <summary>
/// Selects a font and pen into the DC without restoring them. Every renderable drawn after it
/// that relies on the DC defaults (Button / InputField text, the InputField caret) picks them up.
/// </summary>
internal sealed class DcStyle(string fontName, int fontSize, Color penColor) : IRenderable
{
    public void Draw(IntPtr hdc)
    {
        NativeMethods.SelectObject(hdc, FontCache.GetOrCreateFont(fontName, fontSize));
        NativeMethods.SelectObject(hdc, GdiCache.GetOrCreatePen(1, NativeMethods.Rgb(penColor)));
    }
}

/// <summary>Counts how often the render stack was actually painted.</summary>
internal sealed class FrameCounter : IRenderable
{
    public long Frames { get; private set; }

    public void Draw(IntPtr hdc) => Frames++;
}

/// <summary>Three layered, tapered sine waves that react to the mouse.</summary>
internal sealed class Wave(int left, int top, int width, int height, Color[] colors, Color gridColor) : IRenderable
{
    private const int Step = 3;

    private readonly Stopwatch _clock = Stopwatch.StartNew();
    private readonly int _centerY = top + height / 2;
    private double _amplitude = 32;
    private double _targetAmplitude = 32;
    private int? _focusX;

    public double Frequency { get; set; } = 2.0;

    public bool Contains(int x, int y) => x >= left && x <= left + width && y >= top && y <= top + height;

    public void Focus(int x, int y)
    {
        _focusX = x;
        _targetAmplitude = Math.Clamp(Math.Abs(y - _centerY) * 1.1, 8, height / 2.0 - 6);
    }

    public void Unfocus()
    {
        _focusX = null;
        _targetAmplitude = 32;
    }

    public void Draw(IntPtr hdc)
    {
        _amplitude += (_targetAmplitude - _amplitude) * 0.12;
        var t = _clock.Elapsed.TotalSeconds;

        var gridPen = GdiCache.GetOrCreatePen(1, NativeMethods.Rgb(gridColor));
        var oldPen = NativeMethods.SelectObject(hdc, gridPen);
        NativeMethods.MoveToEx(hdc, left, _centerY, IntPtr.Zero);
        NativeMethods.LineTo(hdc, left + width, _centerY);

        for (var i = colors.Length - 1; i >= 0; i--)
        {
            var amplitude = _amplitude * (1.0 - i * 0.28);
            var phase = t * (2.2 + i * 0.6) + i * 1.3;

            NativeMethods.SelectObject(hdc, GdiCache.GetOrCreatePen(2, NativeMethods.Rgb(colors[i])));
            NativeMethods.MoveToEx(hdc, left, YAt(left, amplitude, phase), IntPtr.Zero);
            for (var x = left + Step; x <= left + width; x += Step)
                NativeMethods.LineTo(hdc, x, YAt(x, amplitude, phase));
        }

        if (_focusX is { } fx)
        {
            var fy = YAt(fx, _amplitude, t * 2.2);
            var dotPen = GdiCache.GetOrCreatePen(2, NativeMethods.Rgb(colors[0]));
            var dotBrush = GdiCache.GetOrCreateBrush(NativeMethods.Rgb(gridColor));
            NativeMethods.SelectObject(hdc, dotPen);
            var oldBrush = NativeMethods.SelectObject(hdc, dotBrush);
            Gdi.Ellipse(hdc, fx - 6, fy - 6, fx + 7, fy + 7);
            NativeMethods.SelectObject(hdc, oldBrush);
        }

        NativeMethods.SelectObject(hdc, oldPen);
    }

    private int YAt(int x, double amplitude, double phase)
    {
        var u = (x - left) / (double)width;
        var envelope = Math.Sin(Math.PI * u);
        return _centerY + (int)(amplitude * envelope * Math.Sin(u * Math.PI * 2 * Frequency - phase));
    }
}
