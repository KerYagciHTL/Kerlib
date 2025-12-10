using System.Runtime.InteropServices;

namespace Kerlib.Native;

/// <summary>
/// Cross-platform graphics context wrapper
/// </summary>
public static class GraphicsContext
{
    private static readonly bool IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    private static readonly bool IsLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

    public static void DrawRectangle(IntPtr hdc, int left, int top, int right, int bottom, uint color)
    {
        if (IsWindows)
        {
            var pen = GdiCache.GetOrCreatePen(1, color);
            var oldPen = NativeMethods.SelectObject(hdc, pen);
            NativeMethods.Rectangle(hdc, left, top, right, bottom);
            NativeMethods.SelectObject(hdc, oldPen);
        }
        else if (IsLinux)
        {
            var ctx = X11NativeMethods.GetX11Context(hdc);
            X11NativeMethods.XSetForeground(ctx.Display, ctx.GC, X11NativeMethods.ToX11Color(color));
            X11NativeMethods.XDrawRectangle(ctx.Display, ctx.Drawable, ctx.GC, 
                left, top, (uint)(right - left), (uint)(bottom - top));
        }
    }

    public static void FillRectangle(IntPtr hdc, int left, int top, int right, int bottom, uint color)
    {
        if (IsWindows)
        {
            var brush = GdiCache.GetOrCreateBrush(color);
            var oldBrush = NativeMethods.SelectObject(hdc, brush);
            var rect = new NativeMethods.Rect { left = left, top = top, right = right, bottom = bottom };
            NativeMethods.FillRect(hdc, ref rect, brush);
            NativeMethods.SelectObject(hdc, oldBrush);
        }
        else if (IsLinux)
        {
            var ctx = X11NativeMethods.GetX11Context(hdc);
            X11NativeMethods.XSetForeground(ctx.Display, ctx.GC, X11NativeMethods.ToX11Color(color));
            X11NativeMethods.XFillRectangle(ctx.Display, ctx.Drawable, ctx.GC, 
                left, top, (uint)(right - left), (uint)(bottom - top));
        }
    }

    public static void DrawLine(IntPtr hdc, int x1, int y1, int x2, int y2, uint color, int width = 1)
    {
        if (IsWindows)
        {
            var pen = GdiCache.GetOrCreatePen(width, color);
            var oldPen = NativeMethods.SelectObject(hdc, pen);
            NativeMethods.MoveToEx(hdc, x1, y1, IntPtr.Zero);
            NativeMethods.LineTo(hdc, x2, y2);
            NativeMethods.SelectObject(hdc, oldPen);
        }
        else if (IsLinux)
        {
            var ctx = X11NativeMethods.GetX11Context(hdc);
            X11NativeMethods.XSetForeground(ctx.Display, ctx.GC, X11NativeMethods.ToX11Color(color));
            if (width > 1)
                X11NativeMethods.XSetLineAttributes(ctx.Display, ctx.GC, (uint)width, 0, 0, 0);
            X11NativeMethods.XDrawLine(ctx.Display, ctx.Drawable, ctx.GC, x1, y1, x2, y2);
            if (width > 1)
                X11NativeMethods.XSetLineAttributes(ctx.Display, ctx.GC, 1, 0, 0, 0);
        }
    }

    public static void DrawText(IntPtr hdc, int x, int y, string text, uint color, string fontName = "Arial", int fontSize = 16)
    {
        if (IsWindows)
        {
            NativeMethods.SetTextColor(hdc, color);
            NativeMethods.SetBkMode(hdc, 1); // TRANSPARENT
            IntPtr hFont = FontCache.GetOrCreateFont(fontName, fontSize);
            IntPtr oldFont = NativeMethods.SelectObject(hdc, hFont);
            NativeMethods.TextOutW(hdc, x, y, text, text.Length);
            NativeMethods.SelectObject(hdc, oldFont);
        }
        else if (IsLinux)
        {
            var ctx = X11NativeMethods.GetX11Context(hdc);
            X11NativeMethods.XSetForeground(ctx.Display, ctx.GC, X11NativeMethods.ToX11Color(color));
            
            // For now, use fixed font - full font support would require XFT
            X11NativeMethods.XDrawString(ctx.Display, ctx.Drawable, ctx.GC, x, y + fontSize, text, text.Length);
        }
    }

    public static void DrawTextInRect(IntPtr hdc, string text, int left, int top, int right, int bottom, 
        uint color, uint alignment, string fontName = "Arial", int fontSize = 16)
    {
        if (IsWindows)
        {
            NativeMethods.SetTextColor(hdc, color);
            NativeMethods.SetBkMode(hdc, 1); // TRANSPARENT
            IntPtr hFont = FontCache.GetOrCreateFont(fontName, fontSize);
            IntPtr oldFont = NativeMethods.SelectObject(hdc, hFont);
            
            var rect = new NativeMethods.Rect { left = left, top = top, right = right, bottom = bottom };
            NativeMethods.DrawText(hdc, text, text.Length, ref rect, alignment);
            NativeMethods.SelectObject(hdc, oldFont);
        }
        else if (IsLinux)
        {
            var ctx = X11NativeMethods.GetX11Context(hdc);
            X11NativeMethods.XSetForeground(ctx.Display, ctx.GC, X11NativeMethods.ToX11Color(color));
            
            // Simple center alignment approximation
            int textWidth = text.Length * (fontSize / 2); // Rough estimate
            int textHeight = fontSize;
            
            int textX = left;
            int textY = top + textHeight;
            
            // Center horizontally
            if ((alignment & NativeMethods.DtCenter) != 0)
            {
                textX = left + ((right - left) - textWidth) / 2;
            }
            
            // Center vertically
            if ((alignment & NativeMethods.DtVcenter) != 0)
            {
                textY = top + ((bottom - top) + textHeight) / 2;
            }
            
            X11NativeMethods.XDrawString(ctx.Display, ctx.Drawable, ctx.GC, textX, textY, text, text.Length);
        }
    }

    public static (int width, int height) MeasureText(IntPtr hdc, string text, string fontName = "Arial", int fontSize = 16)
    {
        if (IsWindows)
        {
            IntPtr hFont = FontCache.GetOrCreateFont(fontName, fontSize);
            IntPtr oldFont = NativeMethods.SelectObject(hdc, hFont);
            NativeMethods.GetTextExtentPoint32(hdc, text, text.Length, out var size);
            NativeMethods.SelectObject(hdc, oldFont);
            return (size.cx, size.cy);
        }
        else if (IsLinux)
        {
            // Rough approximation for text size on Linux
            int width = text.Length * (fontSize / 2);
            int height = fontSize;
            return (width, height);
        }
        
        return (0, 0);
    }
}

