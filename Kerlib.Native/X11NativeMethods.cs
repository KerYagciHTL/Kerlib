using System.Runtime.InteropServices;

namespace Kerlib.Native;

public static class X11NativeMethods
{
    public const int Expose = 12;
    public const int KeyPress = 2;
    public const int KeyRelease = 3;
    public const int ButtonPress = 4;
    public const int ButtonRelease = 5;
    public const int MotionNotify = 6;
    public const int ConfigureNotify = 22;
    public const int ClientMessage = 33;

    public const long ExposureMask = 1L << 15;
    public const long KeyPressMask = 1L << 0;
    public const long KeyReleaseMask = 1L << 1;
    public const long ButtonPressMask = 1L << 2;
    public const long ButtonReleaseMask = 1L << 3;
    public const long PointerMotionMask = 1L << 6;
    public const long StructureNotifyMask = 1L << 17;

    public struct X11GC
    {
        public IntPtr Display;
        public IntPtr GC;
        public IntPtr Drawable;
    }

    [StructLayout(LayoutKind.Explicit, Size = 192)]
    public struct XEvent
    {
        [FieldOffset(0)] public int type;
        [FieldOffset(0)] public XExposeEvent xexpose;
        [FieldOffset(0)] public XKeyEvent xkey;
        [FieldOffset(0)] public XButtonEvent xbutton;
        [FieldOffset(0)] public XMotionEvent xmotion;
        [FieldOffset(0)] public XConfigureEvent xconfigure;
        [FieldOffset(0)] public XClientMessageEvent xclient;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct XExposeEvent
    {
        public int type;
        public IntPtr serial;
        public bool send_event;
        public IntPtr display;
        public IntPtr window;
        public int x;
        public int y;
        public int width;
        public int height;
        public int count;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct XKeyEvent
    {
        public int type;
        public IntPtr serial;
        public bool send_event;
        public IntPtr display;
        public IntPtr window;
        public IntPtr root;
        public IntPtr subwindow;
        public IntPtr time;
        public int x;
        public int y;
        public int x_root;
        public int y_root;
        public uint state;
        public uint keycode;
        public bool same_screen;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct XButtonEvent
    {
        public int type;
        public IntPtr serial;
        public bool send_event;
        public IntPtr display;
        public IntPtr window;
        public IntPtr root;
        public IntPtr subwindow;
        public IntPtr time;
        public int x;
        public int y;
        public int x_root;
        public int y_root;
        public uint state;
        public uint button;
        public bool same_screen;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct XMotionEvent
    {
        public int type;
        public IntPtr serial;
        public bool send_event;
        public IntPtr display;
        public IntPtr window;
        public IntPtr root;
        public IntPtr subwindow;
        public IntPtr time;
        public int x;
        public int y;
        public int x_root;
        public int y_root;
        public uint state;
        public byte is_hint;
        public bool same_screen;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct XConfigureEvent
    {
        public int type;
        public IntPtr serial;
        public bool send_event;
        public IntPtr display;
        public IntPtr event_window;
        public IntPtr window;
        public int x;
        public int y;
        public int width;
        public int height;
        public int border_width;
        public IntPtr above;
        public bool override_redirect;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct XClientMessageEvent
    {
        public int type;
        public IntPtr serial;
        public bool send_event;
        public IntPtr display;
        public IntPtr window;
        public IntPtr message_type;
        public int format;
        public IntPtr data_l0;
        public IntPtr data_l1;
        public IntPtr data_l2;
        public IntPtr data_l3;
        public IntPtr data_l4;
    }

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
    public static extern int XCloseDisplay(IntPtr display);

    [DllImport("libX11.so.6")]
    public static extern int XDefaultScreen(IntPtr display);

    [DllImport("libX11.so.6")]
    public static extern IntPtr XRootWindow(IntPtr display, int screen);

    [DllImport("libX11.so.6")]
    public static extern ulong XBlackPixel(IntPtr display, int screen);

    [DllImport("libX11.so.6")]
    public static extern ulong XWhitePixel(IntPtr display, int screen);

    [DllImport("libX11.so.6")]
    public static extern IntPtr XCreateSimpleWindow(IntPtr display, IntPtr parent, int x, int y,
        int width, int height, int borderWidth, ulong border, ulong background);

    [DllImport("libX11.so.6")]
    public static extern int XStoreName(IntPtr display, IntPtr window, string name);

    [DllImport("libX11.so.6")]
    public static extern int XSelectInput(IntPtr display, IntPtr window, long eventMask);

    [DllImport("libX11.so.6")]
    public static extern int XMapWindow(IntPtr display, IntPtr window);

    [DllImport("libX11.so.6")]
    public static extern int XDestroyWindow(IntPtr display, IntPtr window);

    [DllImport("libX11.so.6")]
    public static extern IntPtr XCreateGC(IntPtr display, IntPtr drawable, ulong valuemask, IntPtr values);

    [DllImport("libX11.so.6")]
    public static extern int XFreeGC(IntPtr display, IntPtr gc);

    [DllImport("libX11.so.6")]
    public static extern IntPtr XCreatePixmap(IntPtr display, IntPtr drawable, int width, int height, int depth);

    [DllImport("libX11.so.6")]
    public static extern int XFreePixmap(IntPtr display, IntPtr pixmap);

    [DllImport("libX11.so.6")]
    public static extern int XFlush(IntPtr display);

    [DllImport("libX11.so.6")]
    public static extern int XPending(IntPtr display);

    [DllImport("libX11.so.6")]
    public static extern int XNextEvent(IntPtr display, out XEvent xevent);

    [DllImport("libX11.so.6")]
    public static extern int XCopyArea(IntPtr display, IntPtr src, IntPtr dest, IntPtr gc,
        int srcX, int srcY, int width, int height, int destX, int destY);

    [DllImport("libX11.so.6")]
    public static extern IntPtr XInternAtom(IntPtr display, string atomName, bool onlyIfExists);

    [DllImport("libX11.so.6")]
    public static extern int XSetWMProtocols(IntPtr display, IntPtr window, ref IntPtr protocols, int count);

    [DllImport("libX11.so.6")]
    public static extern IntPtr XLookupKeysym(ref XKeyEvent keyEvent, int index);

    [DllImport("libX11.so.6")]
    public static extern int XLookupString(ref XKeyEvent eventStruct, byte[] bufferReturn,
        int bytesBuffer, out IntPtr keysymReturn, IntPtr statusInOut);

    [DllImport("libX11.so.6")]
    public static extern int XSendEvent(IntPtr display, IntPtr window, bool propagate, long eventMask, ref XExposeEvent eventSend);

    [DllImport("libX11.so.6")]
    public static extern int XSetLineAttributes(IntPtr display, IntPtr gc, uint line_width, int line_style, int cap_style, int join_style);

    [ThreadStatic]
    private static X11GC _currentContext;

    public static X11GC GetX11Context(IntPtr hdc)
    {
        return _currentContext;
    }

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

