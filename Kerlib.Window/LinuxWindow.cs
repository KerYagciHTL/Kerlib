using System.Runtime.InteropServices;
using Kerlib.Interfaces;
using Kerlib.Native;

namespace Kerlib.Window;

public sealed class LinuxWindow : INativeWindow
{
    private readonly string _title;
    private int _width;
    private int _height;
    private Color _backgroundColor = Color.White;
    
    private IntPtr _display = IntPtr.Zero;
    private IntPtr _window = IntPtr.Zero;
    private IntPtr _gc = IntPtr.Zero;
    private IntPtr _pixmap = IntPtr.Zero;
    private int _screen;
    private bool _isDestroyed;
    private readonly RenderStack _renderStack = new();
    private readonly HashSet<Key> _keysDown = [];
    private System.Timers.Timer? _tickTimer;

    public event Action? Resized;
    public event Action? Closed;
    public event Action<Key>? KeyDown;
    public event Action<Key>? KeyUp;
    public event Action<IReadOnlyCollection<Key>>? KeysDown;
    public event Action? Tick;
    public event Action<int, int>? MouseMove;
    public event Action<int, int>? MouseDown;
    public event Action<int, int>? MouseUp;
    public event Action<int, int, int>? MouseWheel;

    public LinuxWindow(string title, int width, int height, Color? bgColor = null)
    {
        _title = title;
        _width = width;
        _height = height;
        if (bgColor.HasValue) _backgroundColor = bgColor.Value;
        
        InitializeX11();
        
        // Setup timer for tick events
        _tickTimer = new System.Timers.Timer(16); // ~60 FPS
        _tickTimer.Elapsed += (s, e) => Tick?.Invoke();
        _tickTimer.Start();
    }

    private void InitializeX11()
    {
        _display = X11.XOpenDisplay(IntPtr.Zero);
        if (_display == IntPtr.Zero)
            throw new Exception("Cannot open X11 display");

        _screen = X11.XDefaultScreen(_display);
        var rootWindow = X11.XRootWindow(_display, _screen);
        var blackPixel = X11.XBlackPixel(_display, _screen);
        var whitePixel = X11.XWhitePixel(_display, _screen);

        _window = X11.XCreateSimpleWindow(_display, rootWindow, 0, 0, _width, _height, 1,
            blackPixel, whitePixel);

        X11.XStoreName(_display, _window, _title);

        // Select input events
        var eventMask = X11.ExposureMask | X11.KeyPressMask | X11.KeyReleaseMask |
                       X11.ButtonPressMask | X11.ButtonReleaseMask | X11.PointerMotionMask |
                       X11.StructureNotifyMask;
        X11.XSelectInput(_display, _window, eventMask);

        // Setup WM_DELETE_WINDOW protocol
        var wmDeleteWindow = X11.XInternAtom(_display, "WM_DELETE_WINDOW", false);
        X11.XSetWMProtocols(_display, _window, ref wmDeleteWindow, 1);

        // Create graphics context
        _gc = X11.XCreateGC(_display, _window, 0, IntPtr.Zero);
        
        // Create pixmap for double buffering
        _pixmap = X11.XCreatePixmap(_display, _window, _width, _height, 24);
    }

    public int GetWidth() => _width;
    public int GetHeight() => _height;
    public string GetTitle() => _title;
    
    public void SetBackgroundColor(Color color)
    {
        _backgroundColor = color;
        Invalidate();
    }

    public void Show() 
    {
        X11.XMapWindow(_display, _window);
        X11.XFlush(_display);
    }

    public void Destroy() 
    {
        if (_isDestroyed) return;
        
        _tickTimer?.Stop();
        _tickTimer?.Dispose();
        _tickTimer = null;
        
        if (_pixmap != IntPtr.Zero)
        {
            X11.XFreePixmap(_display, _pixmap);
            _pixmap = IntPtr.Zero;
        }
        
        if (_gc != IntPtr.Zero)
        {
            X11.XFreeGC(_display, _gc);
            _gc = IntPtr.Zero;
        }
        
        if (_window != IntPtr.Zero)
        {
            X11.XDestroyWindow(_display, _window);
            _window = IntPtr.Zero;
        }
        
        if (_display != IntPtr.Zero)
        {
            X11.XCloseDisplay(_display);
            _display = IntPtr.Zero;
        }
        
        _isDestroyed = true;
        Closed?.Invoke();
    }

    public void Add(IRenderable renderable)
    {
        if (renderable is INotifyRenderableChanged notifyRenderable)
            notifyRenderable.Changed += OnNotifyRenderable;
        _renderStack.Add(renderable);
        Invalidate();
    }

    public void Add(RenderStack stack)
    {
        foreach (var drawable in stack)
            if (drawable is IRenderable renderable)
                Add(renderable);
    }

    public void Remove(IRenderable renderable)
    {
        if (renderable is INotifyRenderableChanged notifyRenderable)
            notifyRenderable.Changed -= OnNotifyRenderable;
        if (renderable is IImage image)
            image.Dispose();
        _renderStack.Remove(renderable);
        Invalidate();
    }

    private void OnNotifyRenderable(object? sender, EventArgs e) => Invalidate();

    private void Invalidate()
    {
        if (_window != IntPtr.Zero && !_isDestroyed)
        {
            var exposeEvent = new X11.XExposeEvent
            {
                type = X11.Expose,
                display = _display,
                window = _window,
                x = 0,
                y = 0,
                width = _width,
                height = _height,
                count = 0
            };
            X11.XSendEvent(_display, _window, false, X11.ExposureMask, ref exposeEvent);
            X11.XFlush(_display);
        }
    }

    public bool ProcessMessages()
    {
        if (_display == IntPtr.Zero || _isDestroyed)
            return false;

        while (X11.XPending(_display) > 0)
        {
            X11.XNextEvent(_display, out var xevent);
            
            switch (xevent.type)
            {
                case X11.Expose:
                    OnPaint();
                    break;
                    
                case X11.ConfigureNotify:
                    if (xevent.xconfigure.width != _width || xevent.xconfigure.height != _height)
                    {
                        _width = xevent.xconfigure.width;
                        _height = xevent.xconfigure.height;
                        
                        // Recreate pixmap for new size
                        if (_pixmap != IntPtr.Zero)
                            X11.XFreePixmap(_display, _pixmap);
                        _pixmap = X11.XCreatePixmap(_display, _window, _width, _height, 24);
                        
                        Resized?.Invoke();
                    }
                    break;
                    
                case X11.KeyPress:
                    {
                        var keysym = X11.XLookupKeysym(ref xevent.xkey, 0);
                        var key = ConvertX11KeyToKey(keysym);
                        if (!_keysDown.Contains(key))
                        {
                            _keysDown.Add(key);
                            KeyDown?.Invoke(key);
                            KeysDown?.Invoke(GetPressedKeys());
                        }
                        
                        // Handle text input for InputFields
                        var buffer = new byte[32];
                        var count = X11.XLookupString(ref xevent.xkey, buffer, buffer.Length, out _, IntPtr.Zero);
                        if (count > 0)
                        {
                            var text = System.Text.Encoding.UTF8.GetString(buffer, 0, count);
                            foreach (var ch in text)
                            {
                                foreach (var inputField in _renderStack.OfType<IInputField>())
                                    inputField.HandleKeyPress(ch);
                            }
                        }
                    }
                    break;
                    
                case X11.KeyRelease:
                    {
                        var keysym = X11.XLookupKeysym(ref xevent.xkey, 0);
                        var key = ConvertX11KeyToKey(keysym);
                        if (_keysDown.Remove(key))
                        {
                            KeyUp?.Invoke(key);
                            KeysDown?.Invoke(GetPressedKeys());
                        }
                    }
                    break;
                    
                case X11.ButtonPress:
                    {
                        var x = xevent.xbutton.x;
                        var y = xevent.xbutton.y;
                        
                        if (xevent.xbutton.button == 1) // Left button
                        {
                            foreach (var button in _renderStack.OfType<IButton>())
                                button.HandleMouseDown(x, y);
                            foreach (var inputField in _renderStack.OfType<IInputField>())
                                inputField.HandleMouseDown(x, y);
                            MouseDown?.Invoke(x, y);
                        }
                        else if (xevent.xbutton.button == 4) // Scroll up
                        {
                            MouseWheel?.Invoke(x, y, 120);
                        }
                        else if (xevent.xbutton.button == 5) // Scroll down
                        {
                            MouseWheel?.Invoke(x, y, -120);
                        }
                        Invalidate();
                    }
                    break;
                    
                case X11.ButtonRelease:
                    {
                        var x = xevent.xbutton.x;
                        var y = xevent.xbutton.y;
                        
                        if (xevent.xbutton.button == 1) // Left button
                        {
                            foreach (var button in _renderStack.OfType<IButton>())
                                button.HandleMouseUp(x, y);
                            MouseUp?.Invoke(x, y);
                        }
                        Invalidate();
                    }
                    break;
                    
                case X11.MotionNotify:
                    {
                        var x = xevent.xmotion.x;
                        var y = xevent.xmotion.y;
                        var needsInvalidate = false;
                        
                        foreach (var button in _renderStack.OfType<IButton>())
                            if (button.HandleMouseMove(x, y))
                                needsInvalidate = true;
                        foreach (var inputField in _renderStack.OfType<IInputField>())
                            if (inputField.HandleMouseMove(x, y))
                                needsInvalidate = true;
                        
                        MouseMove?.Invoke(x, y);
                        if (needsInvalidate)
                            Invalidate();
                    }
                    break;
                    
                case X11.ClientMessage:
                    if (xevent.xclient.data_l0 == X11.XInternAtom(_display, "WM_DELETE_WINDOW", false))
                    {
                        return false; // Window close requested
                    }
                    break;
            }
        }

        KeysDown?.Invoke(GetPressedKeys());
        return !_isDestroyed;
    }

    private void OnPaint()
    {
        if (_pixmap == IntPtr.Zero || _gc == IntPtr.Zero)
            return;

        // Clear pixmap with background color
        var colorValue = (_backgroundColor.R << 16) | (_backgroundColor.G << 8) | _backgroundColor.B;
        X11.XSetForeground(_display, _gc, (ulong)colorValue);
        X11.XFillRectangle(_display, _pixmap, _gc, 0, 0, _width, _height);

        // Set X11 context for cross-platform drawing
        X11NativeMethods.SetCurrentContext(_display, _gc, _pixmap);
        
        try
        {
            // Draw all renderables to pixmap
            _renderStack.DrawAll(_pixmap);
        }
        finally
        {
            // Clear context after rendering
            X11NativeMethods.ClearCurrentContext();
        }

        // Copy pixmap to window
        X11.XCopyArea(_display, _pixmap, _window, _gc, 0, 0, _width, _height, 0, 0);
        X11.XFlush(_display);
    }

    private IReadOnlyCollection<Key> GetPressedKeys() => _keysDown.ToArray();

    private static Key ConvertX11KeyToKey(IntPtr keysym)
    {
        var sym = keysym.ToInt64();
        
        // Letters
        if (sym >= 0x61 && sym <= 0x7A) // a-z
            return Key.FromVirtualCode(sym - 0x61 + 0x41);
        if (sym >= 0x41 && sym <= 0x5A) // A-Z
            return Key.FromVirtualCode(sym);
            
        // Numbers
        if (sym >= 0x30 && sym <= 0x39) // 0-9
            return Key.FromVirtualCode(sym);
            
        // Special keys
        return sym switch
        {
            0xFF0D => Key.FromVirtualCode(0x0D), // Return/Enter
            0xFF1B => Key.FromVirtualCode(0x1B), // Escape
            0xFF08 => Key.FromVirtualCode(0x08), // Backspace
            0xFF09 => Key.FromVirtualCode(0x09), // Tab
            0x0020 => Key.FromVirtualCode(0x20), // Space
            0xFFBE => Key.FromVirtualCode(0x70), // F1
            0xFFBF => Key.FromVirtualCode(0x71), // F2
            0xFFC0 => Key.FromVirtualCode(0x72), // F3
            0xFFC1 => Key.FromVirtualCode(0x73), // F4
            0xFFC2 => Key.FromVirtualCode(0x74), // F5
            0xFFC3 => Key.FromVirtualCode(0x75), // F6
            0xFFC4 => Key.FromVirtualCode(0x76), // F7
            0xFFC5 => Key.FromVirtualCode(0x77), // F8
            0xFFC6 => Key.FromVirtualCode(0x78), // F9
            0xFFC7 => Key.FromVirtualCode(0x79), // F10
            0xFFC8 => Key.FromVirtualCode(0x7A), // F11
            0xFFC9 => Key.FromVirtualCode(0x7B), // F12
            0xFF51 => Key.FromVirtualCode(0x25), // Left arrow
            0xFF52 => Key.FromVirtualCode(0x26), // Up arrow
            0xFF53 => Key.FromVirtualCode(0x27), // Right arrow
            0xFF54 => Key.FromVirtualCode(0x28), // Down arrow
            _ => Key.FromVirtualCode(0) // Unknown
        };
    }

    public void Dispose() 
    {
        Destroy();
    }

    // X11 P/Invoke declarations
    private static class X11
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
            int width, int height, int border_width, ulong border, ulong background);

        [DllImport("libX11.so.6")]
        public static extern int XStoreName(IntPtr display, IntPtr window, string name);

        [DllImport("libX11.so.6")]
        public static extern int XSelectInput(IntPtr display, IntPtr window, long event_mask);

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
        public static extern int XSetForeground(IntPtr display, IntPtr gc, ulong foreground);

        [DllImport("libX11.so.6")]
        public static extern int XFillRectangle(IntPtr display, IntPtr drawable, IntPtr gc, int x, int y, int width, int height);

        [DllImport("libX11.so.6")]
        public static extern int XDrawRectangle(IntPtr display, IntPtr drawable, IntPtr gc, int x, int y, int width, int height);

        [DllImport("libX11.so.6")]
        public static extern int XDrawLine(IntPtr display, IntPtr drawable, IntPtr gc, int x1, int y1, int x2, int y2);

        [DllImport("libX11.so.6")]
        public static extern int XDrawString(IntPtr display, IntPtr drawable, IntPtr gc, int x, int y, string str, int len);

        [DllImport("libX11.so.6")]
        public static extern int XCopyArea(IntPtr display, IntPtr src, IntPtr dest, IntPtr gc,
            int src_x, int src_y, int width, int height, int dest_x, int dest_y);

        [DllImport("libX11.so.6")]
        public static extern IntPtr XInternAtom(IntPtr display, string atom_name, bool only_if_exists);

        [DllImport("libX11.so.6")]
        public static extern int XSetWMProtocols(IntPtr display, IntPtr window, ref IntPtr protocols, int count);

        [DllImport("libX11.so.6")]
        public static extern IntPtr XLookupKeysym(ref XKeyEvent key_event, int index);

        [DllImport("libX11.so.6")]
        public static extern int XLookupString(ref XKeyEvent event_struct, byte[] buffer_return,
            int bytes_buffer, out IntPtr keysym_return, IntPtr status_in_out);

        [DllImport("libX11.so.6")]
        public static extern int XSendEvent(IntPtr display, IntPtr window, bool propagate, long event_mask, ref XExposeEvent event_send);

        [StructLayout(LayoutKind.Explicit)]
        public struct XEvent
        {
            [FieldOffset(0)] public int type;
            [FieldOffset(0)] public XExposeEvent xexpose;
            [FieldOffset(0)] public XKeyEvent xkey;
            [FieldOffset(0)] public XButtonEvent xbutton;
            [FieldOffset(0)] public XMotionEvent xmotion;
            [FieldOffset(0)] public XConfigureEvent xconfigure;
            [FieldOffset(0)] public XClientMessageEvent xclient;
            [FieldOffset(0)] public unsafe fixed long pad[24];
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
    }
}
