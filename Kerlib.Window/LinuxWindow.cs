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
        

        _tickTimer = new System.Timers.Timer(16);
        _tickTimer.Elapsed += (_, _) => Tick?.Invoke();
        _tickTimer.Start();
    }

    private void InitializeX11()
    {
        _display = X11NativeMethods.XOpenDisplay(IntPtr.Zero);
        if (_display == IntPtr.Zero)
            throw new Exception("Cannot open X11 display");

        _screen = X11NativeMethods.XDefaultScreen(_display);
        var rootWindow = X11NativeMethods.XRootWindow(_display, _screen);
        var blackPixel = X11NativeMethods.XBlackPixel(_display, _screen);
        var whitePixel = X11NativeMethods.XWhitePixel(_display, _screen);

        _window = X11NativeMethods.XCreateSimpleWindow(_display, rootWindow, 0, 0, _width, _height, 1,
            blackPixel, whitePixel);

        X11NativeMethods.XStoreName(_display, _window, _title);

        var eventMask = X11NativeMethods.ExposureMask | X11NativeMethods.KeyPressMask | X11NativeMethods.KeyReleaseMask |
                       X11NativeMethods.ButtonPressMask | X11NativeMethods.ButtonReleaseMask | X11NativeMethods.PointerMotionMask |
                       X11NativeMethods.StructureNotifyMask;
        X11NativeMethods.XSelectInput(_display, _window, eventMask);

        var wmDeleteWindow = X11NativeMethods.XInternAtom(_display, "WM_DELETE_WINDOW", false);
        X11NativeMethods.XSetWMProtocols(_display, _window, ref wmDeleteWindow, 1);

        _gc = X11NativeMethods.XCreateGC(_display, _window, 0, IntPtr.Zero);
        

        _pixmap = X11NativeMethods.XCreatePixmap(_display, _window, _width, _height, 24);
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
        X11NativeMethods.XMapWindow(_display, _window);
        X11NativeMethods.XFlush(_display);
    }

    public void Destroy() 
    {
        if (_isDestroyed) return;
        
        _tickTimer?.Stop();
        _tickTimer?.Dispose();
        _tickTimer = null;
        
        if (_pixmap != IntPtr.Zero)
        {
            X11NativeMethods.XFreePixmap(_display, _pixmap);
            _pixmap = IntPtr.Zero;
        }
        
        if (_gc != IntPtr.Zero)
        {
            X11NativeMethods.XFreeGC(_display, _gc);
            _gc = IntPtr.Zero;
        }
        
        if (_window != IntPtr.Zero)
        {
            X11NativeMethods.XDestroyWindow(_display, _window);
            _window = IntPtr.Zero;
        }
        
        if (_display != IntPtr.Zero)
        {
            X11NativeMethods.XCloseDisplay(_display);
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
        {
            if (drawable is { } renderable)
                Add(renderable);
        }
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
            var exposeEvent = new X11NativeMethods.XExposeEvent
            {
                type = X11NativeMethods.Expose,
                display = _display,
                window = _window,
                x = 0,
                y = 0,
                width = _width,
                height = _height,
                count = 0
            };
            X11NativeMethods.XSendEvent(_display, _window, false, X11NativeMethods.ExposureMask, ref exposeEvent);
            X11NativeMethods.XFlush(_display);
        }
    }

    public bool ProcessMessages()
    {
        if (_display == IntPtr.Zero || _isDestroyed)
            return false;

        while (X11NativeMethods.XPending(_display) > 0)
        {
            X11NativeMethods.XNextEvent(_display, out var xevent);
            
            switch (xevent.type)
            {
                case X11NativeMethods.Expose:
                    OnPaint();
                    break;
                    
                case X11NativeMethods.ConfigureNotify:
                    if (xevent.xconfigure.width != _width || xevent.xconfigure.height != _height)
                    {
                        _width = xevent.xconfigure.width;
                        _height = xevent.xconfigure.height;
                        

                        if (_pixmap != IntPtr.Zero)
                            X11NativeMethods.XFreePixmap(_display, _pixmap);
                        _pixmap = X11NativeMethods.XCreatePixmap(_display, _window, _width, _height, 24);
                        
                        Resized?.Invoke();
                    }
                    break;
                    
                case X11NativeMethods.KeyPress:
                    {
                        var keysym = X11NativeMethods.XLookupKeysym(ref xevent.xkey, 0);
                        var key = ConvertX11KeyToKey(keysym);
                        if (!_keysDown.Contains(key))
                        {
                            _keysDown.Add(key);
                            KeyDown?.Invoke(key);
                            KeysDown?.Invoke(GetPressedKeys());
                        }
                        
                        var buffer = new byte[32];
                        var count = X11NativeMethods.XLookupString(ref xevent.xkey, buffer, buffer.Length, out _, IntPtr.Zero);
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
                    
                case X11NativeMethods.KeyRelease:
                    {
                        var keysym = X11NativeMethods.XLookupKeysym(ref xevent.xkey, 0);
                        var key = ConvertX11KeyToKey(keysym);
                        if (_keysDown.Remove(key))
                        {
                            KeyUp?.Invoke(key);
                            KeysDown?.Invoke(GetPressedKeys());
                        }
                    }
                    break;
                    
                case X11NativeMethods.ButtonPress:
                    {
                        var x = xevent.xbutton.x;
                        var y = xevent.xbutton.y;
                        
                        if (xevent.xbutton.button == 1)
                        {
                            foreach (var button in _renderStack.OfType<IButton>())
                                button.HandleMouseDown(x, y);
                            foreach (var inputField in _renderStack.OfType<IInputField>())
                                inputField.HandleMouseDown(x, y);
                            MouseDown?.Invoke(x, y);
                        }
                        else if (xevent.xbutton.button == 4)
                        {
                            MouseWheel?.Invoke(x, y, 120);
                        }
                        else if (xevent.xbutton.button == 5)
                        {
                            MouseWheel?.Invoke(x, y, -120);
                        }
                        Invalidate();
                    }
                    break;
                    
                case X11NativeMethods.ButtonRelease:
                    {
                        var x = xevent.xbutton.x;
                        var y = xevent.xbutton.y;
                        
                        if (xevent.xbutton.button == 1)
                        {
                            foreach (var button in _renderStack.OfType<IButton>())
                                button.HandleMouseUp(x, y);
                            MouseUp?.Invoke(x, y);
                        }
                        Invalidate();
                    }
                    break;
                    
                case X11NativeMethods.MotionNotify:
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
                    
                case X11NativeMethods.ClientMessage:
                    if (xevent.xclient.data_l0 == X11NativeMethods.XInternAtom(_display, "WM_DELETE_WINDOW", false))
                    {
                        return false;
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

        var colorValue = (_backgroundColor.R << 16) | (_backgroundColor.G << 8) | _backgroundColor.B;
        X11NativeMethods.XSetForeground(_display, _gc, (ulong)colorValue);
        X11NativeMethods.XFillRectangle(_display, _pixmap, _gc, 0, 0, (uint)_width, (uint)_height);

        X11NativeMethods.SetCurrentContext(_display, _gc, _pixmap);
        
        try
        {

            _renderStack.DrawAll(_pixmap);
        }
        finally
        {

            X11NativeMethods.ClearCurrentContext();
        }

        X11NativeMethods.XCopyArea(_display, _pixmap, _window, _gc, 0, 0, _width, _height, 0, 0);
        X11NativeMethods.XFlush(_display);
    }

    private IReadOnlyCollection<Key> GetPressedKeys() => _keysDown.ToArray();

    private static Key ConvertX11KeyToKey(IntPtr keysym)
    {
        var sym = keysym.ToInt64();

        return sym switch
        {
            >= 0x61 and <= 0x7A => Key.FromVirtualCode((int)(sym - 0x61 + 0x41)),
            >= 0x41 and <= 0x5A or >= 0x30 and <= 0x39 => Key.FromVirtualCode((int)sym),
            _ => sym switch
            {
                0xFF0D => Key.FromVirtualCode(0x0D),
                0xFF1B => Key.FromVirtualCode(0x1B),
                0xFF08 => Key.FromVirtualCode(0x08),
                0xFF09 => Key.FromVirtualCode(0x09),
                0x0020 => Key.FromVirtualCode(0x20),
                0xFFBE => Key.FromVirtualCode(0x70),
                0xFFBF => Key.FromVirtualCode(0x71),
                0xFFC0 => Key.FromVirtualCode(0x72),
                0xFFC1 => Key.FromVirtualCode(0x73),
                0xFFC2 => Key.FromVirtualCode(0x74),
                0xFFC3 => Key.FromVirtualCode(0x75),
                0xFFC4 => Key.FromVirtualCode(0x76),
                0xFFC5 => Key.FromVirtualCode(0x77),
                0xFFC6 => Key.FromVirtualCode(0x78),
                0xFFC7 => Key.FromVirtualCode(0x79),
                0xFFC8 => Key.FromVirtualCode(0x7A),
                0xFFC9 => Key.FromVirtualCode(0x7B),
                0xFF51 => Key.FromVirtualCode(0x25),
                0xFF52 => Key.FromVirtualCode(0x26),
                0xFF53 => Key.FromVirtualCode(0x27),
                0xFF54 => Key.FromVirtualCode(0x28),
                _ => Key.FromVirtualCode(0)
            }
        };
    }

    public void Dispose() 
    {
        Destroy();
    }
}
