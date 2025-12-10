using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace Kerlib.Native;

/// <summary>
/// X11 native methods for Linux rendering
/// </summary>
public static class X11NativeMethods
{
    private static bool _isLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
    
    // X11 Graphics Context handle wrapper
    public struct X11GC
    {
        public IntPtr Display;
        public IntPtr GC;
        public IntPtr Drawable;
    }

    [DllImport("libX11.so.6")]
    public static extern int XSetForeground(IntPtr display, IntPtr gc, ulong foreground);

    [DllImport("libX11.so.6")]
    public static extern int XSetBackground(IntPtr display, IntPtr gc, ulong background);

    [DllImport("libX11.so.6")]
    public static extern int XFillRectangle(IntPtr display, IntPtr drawable, IntPtr gc, int x, int y, uint width, uint height);

    [DllImport("libX11.so.6")]
    public static extern int XDrawRectangle(IntPtr display, IntPtr drawable, IntPtr gc, int x, int y, uint width, uint height);

    [DllImport("libX11.so.6")]
    public static extern int XDrawLine(IntPtr display, IntPtr drawable, IntPtr gc, int x1, int y1, int x2, int y2);

    [DllImport("libX11.so.6")]
    public static extern int XDrawString(IntPtr display, IntPtr drawable, IntPtr gc, int x, int y, string str, int len);

    [DllImport("libX11.so.6")]
    public static extern int XTextExtents(IntPtr font_struct, string str, int nchars, 
        out int direction_return, out int font_ascent_return, out int font_descent_return,
        out XCharStruct overall_return);

    [DllImport("libX11.so.6")]
    public static extern IntPtr XLoadQueryFont(IntPtr display, string name);

    [DllImport("libX11.so.6")]
    public static extern int XSetFont(IntPtr display, IntPtr gc, IntPtr font);

    [DllImport("libX11.so.6")]
    public static extern int XUnloadFont(IntPtr display, IntPtr font);

    [DllImport("libX11.so.6")]
    public static extern IntPtr XOpenDisplay(IntPtr display);

    [DllImport("libX11.so.6")]
    public static extern IntPtr XCreateGC(IntPtr display, IntPtr drawable, ulong valuemask, IntPtr values);

    [DllImport("libX11.so.6")]
    public static extern int XSetLineAttributes(IntPtr display, IntPtr gc, uint line_width, int line_style, int cap_style, int join_style);

    [StructLayout(LayoutKind.Sequential)]
    public struct XCharStruct
    {
        public short lbearing;
        public short rbearing;
        public short width;
        public short ascent;
        public short descent;
        public ushort attributes;
    }

    // Helper method to get X11 context from HDC (pixmap handle on Linux)
    public static X11GC GetX11Context(IntPtr hdc)
    {
        // On Linux, the hdc is actually a pixmap handle
        // We need to extract display and GC from thread-local storage
        // This is set by LinuxWindow during rendering
        return _currentContext;
    }

    [ThreadStatic]
    private static X11GC _currentContext;

    public static void SetCurrentContext(IntPtr display, IntPtr gc, IntPtr drawable)
    {
        _currentContext = new X11GC
        {
            Display = display,
            GC = gc,
            Drawable = drawable
        };
    }

    public static void ClearCurrentContext()
    {
        _currentContext = default;
    }

    // Color conversion for X11
    public static ulong ToX11Color(Color color)
    {
        return (ulong)((color.R << 16) | (color.G << 8) | color.B);
    }

    public static ulong ToX11Color(uint rgb)
    {
        var r = rgb & 0xFF;
        var g = (rgb >> 8) & 0xFF;
        var b = (rgb >> 16) & 0xFF;
        return (r << 16) | (g << 8) | b;
    }
}

