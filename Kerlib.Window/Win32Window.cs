using System.ComponentModel;
using Kerlib.Native;
using System.Runtime.InteropServices;
using Kerlib.Interfaces;

namespace Kerlib.Window;

public sealed class Win32Window : IDisposable
{
    private readonly string _className;
    private readonly IntPtr _hInstance;
    private IntPtr _hwnd;
    private readonly string _title;
    private Color _backgroundColor = Color.White;
    private int _width, _height;

    private IntPtr _hBackgroundBrush = IntPtr.Zero;

    private readonly NativeMethods.WndProcDelegate _wndProcDelegate;

    private bool _disposed;
    private bool _isDestroyed;

    private readonly RenderStack _renderStack = new();
    private static readonly Dictionary<string, bool> RegisteredClasses = new();

    public event Action? Resized;
    public event Action? Closed;
    public event Action<Key>? KeyDown;

    public Win32Window(string title, int width, int height, Color? bgColor = null)
    {
        _title = title;
        _height = height;
        _width = width;
        _className = $"Win32Window_{Guid.NewGuid()}";
        _hInstance = NativeMethods.GetModuleHandle(null!);
        _wndProcDelegate = WndProc;
        _backgroundColor = bgColor ?? _backgroundColor;

        RegisterWindowClass();
        CreateNativeWindow(title, width, height);
    }

    private void RegisterWindowClass()
    {
        if (RegisteredClasses.ContainsKey(_className)) return;

        _hBackgroundBrush = NativeMethods.CreateSolidBrush(NativeMethods.Rgb(_backgroundColor));

        var wndClassEx = new NativeMethods.Wndclassexw
        {
            cbSize = (uint)Marshal.SizeOf<NativeMethods.Wndclassexw>(),
            style = NativeMethods.CsHredraw | NativeMethods.CsVredraw,
            lpfnWndProc = Marshal.GetFunctionPointerForDelegate(_wndProcDelegate),
            cbClsExtra = 0,
            cbWndExtra = 0,
            hInstance = _hInstance,
            hIcon = IntPtr.Zero,
            hCursor = IntPtr.Zero,
            hbrBackground = _hBackgroundBrush,
            lpszMenuName = null!,
            lpszClassName = _className,
            hIconSm = IntPtr.Zero
        };

        var regResult = NativeMethods.RegisterClassExW(in wndClassEx);
        if (regResult == 0)
            ThrowLastWin32Error("RegisterClassEx failed");

        RegisteredClasses[_className] = true;
    }

    public void SetBackgroundColor(Color color)
    {
        _backgroundColor = color;

        if (_hBackgroundBrush != IntPtr.Zero)
        {
            NativeMethods.DeleteObject(_hBackgroundBrush);
            _hBackgroundBrush = IntPtr.Zero;
        }

        _hBackgroundBrush = NativeMethods.CreateSolidBrush(NativeMethods.Rgb(color));
        NativeMethods.SetClassLongPtr(_hwnd, NativeMethods.GclpHbrbackground, _hBackgroundBrush);

        Invalidate();
    }

    private void CreateNativeWindow(string title, int clientWidth, int clientHeight)
    {
        var rect = new NativeMethods.Rect { left = 0, top = 0, right = clientWidth, bottom = clientHeight };

        if (!NativeMethods.AdjustWindowRectEx(ref rect,
                NativeMethods.WsOverlappedwindow,
                false,
                0))
        {
            ThrowLastWin32Error("AdjustWindowRectEx failed");
        }

        int winWidth = rect.right - rect.left;
        int winHeight = rect.bottom - rect.top;

        _hwnd = NativeMethods.CreateWindowExW(
            0,
            _className,
            title,
            NativeMethods.WsOverlappedwindow | NativeMethods.WsVisible,
            100,
            100,
            winWidth,
            winHeight,
            IntPtr.Zero,
            IntPtr.Zero,
            _hInstance,
            IntPtr.Zero);

        if (_hwnd == IntPtr.Zero)
            ThrowLastWin32Error("CreateWindowEx failed");
    }

    public void Show()
    {
        NativeMethods.ShowWindow(_hwnd, NativeMethods.SwShowdefault);
        NativeMethods.UpdateWindow(_hwnd);
        Invalidate();
    }

    public void Destroy()
    {
        if (_hwnd != IntPtr.Zero && !_isDestroyed)
        {
            NativeMethods.DestroyWindow(_hwnd);
            _isDestroyed = true;
            _hwnd = IntPtr.Zero;
        }
    }

    public void Add(IRenderable drawable)
    {
        if(drawable is INotifyRenderableChanged notifyRenderable)
        {
            notifyRenderable.Changed += OnNotifyRenderable;
        }
        
        _renderStack.Add(drawable);
        Invalidate();
    }
    public void Add(RenderStack stack)
    {
        foreach (var drawable in stack)
        {
            if (drawable is IRenderable renderable)
                Add(renderable);
        }
    }

    private void Invalidate()
    {
        if (_hwnd != IntPtr.Zero)
            NativeMethods.InvalidateRect(_hwnd, IntPtr.Zero, true);
    }

    private IntPtr WndProc(IntPtr hwnd, uint msg, UIntPtr wParam, IntPtr lParam)
    {
        if (_isDestroyed)
            return NativeMethods.DefWindowProcW(hwnd, msg, wParam, lParam);

        switch (msg)
        {
            case NativeMethods.WmPaint:
                OnPaint(hwnd);
                return IntPtr.Zero;

            case NativeMethods.WmKeyDown:
                var key = Key.FromVirtualCode((int)wParam);
                KeyDown?.Invoke(key);
                return IntPtr.Zero;
            
            case NativeMethods.WmSize:
                if (_hwnd != IntPtr.Zero)
                {
                    if (NativeMethods.GetClientRect(_hwnd, out var rect))
                    {
                        _width = rect.right - rect.left;
                        _height = rect.bottom - rect.top;
                    }
                }
                Resized?.Invoke();
                return IntPtr.Zero;

            case NativeMethods.WmDestroy:
                Closed?.Invoke();
                ClearEvents();
                _isDestroyed = true;
                _hwnd = IntPtr.Zero;
                return IntPtr.Zero;

            case NativeMethods.WmMouseMove:
                var needsInvalidate = false;
                foreach (var r in _renderStack.OfType<IButton>())
                    if (r.HandleMouseMove(GET_X_LPARAM(lParam), GET_Y_LPARAM(lParam)))
                        needsInvalidate = true;

                foreach (var r in _renderStack.OfType<IInputField>())
                    if (r.HandleMouseMove(GET_X_LPARAM(lParam), GET_Y_LPARAM(lParam)))
                        needsInvalidate = true;

                if (needsInvalidate) Invalidate();
                return IntPtr.Zero;

            case NativeMethods.WmLButtonDown:
                foreach (var r in _renderStack.OfType<IButton>())
                    r.HandleMouseDown(GET_X_LPARAM(lParam), GET_Y_LPARAM(lParam));

                foreach (var r in _renderStack.OfType<IInputField>())
                    r.HandleMouseDown(GET_X_LPARAM(lParam), GET_Y_LPARAM(lParam));

                Invalidate();
                return IntPtr.Zero;
            
            case NativeMethods.WmLButtonUp:
                foreach (var r in _renderStack.OfType<IButton>())
                {
                    r.HandleMouseUp(GET_X_LPARAM(lParam), GET_Y_LPARAM(lParam));
                }
                Invalidate();
                return IntPtr.Zero;
            
            case NativeMethods.WmKeyPress:
                foreach (var r in _renderStack.OfType<IInputField>())
                    r.HandleKeyPress((char)wParam);
                Invalidate();
                return IntPtr.Zero;
    
            default:
                return NativeMethods.DefWindowProcW(hwnd, msg, wParam, lParam);
        }
    }

    private void OnPaint(IntPtr hwnd)
    {
        var hdc = NativeMethods.BeginPaint(hwnd, out var ps);
        if (hdc == IntPtr.Zero) return;

        try
        {
            _renderStack.DrawAll(hdc);
        }
        finally
        {
            NativeMethods.EndPaint(hwnd, ref ps);
        }
    }

    private static void ThrowLastWin32Error(string msg)
    {
        throw new Win32Exception(Marshal.GetLastWin32Error(), msg);
    }

    private static int GET_X_LPARAM(IntPtr lParam) => (short)(lParam.ToInt32() & 0xFFFF);
    private static int GET_Y_LPARAM(IntPtr lParam) => (short)((lParam.ToInt32() >> 16) & 0xFFFF);

    public int GetHeight() => _height;
    public int GetWidth() => _width;
    public string GetTitle() => _title;

    private void OnNotifyRenderable(object? sender, EventArgs e)
    {
        Invalidate();
    }
    
    private void CleanRenderStack()
    {
        foreach (var r in _renderStack)
        {
            switch (r)
            {
                case INotifyRenderableChanged notifyRenderable:
                    notifyRenderable.Changed -= OnNotifyRenderable;
                    break;
                case IDisposable disposable:
                    disposable.Dispose();
                    break;
            }
        }
        
        _renderStack.Clear();
    }
    private void ClearEvents()
    {
        Resized = null;
        Closed = null;
        
        CleanRenderStack();
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        if (_hBackgroundBrush != IntPtr.Zero)
        {
            NativeMethods.DeleteObject(_hBackgroundBrush);
            _hBackgroundBrush = IntPtr.Zero;
        }

        if (!_isDestroyed && _hwnd != IntPtr.Zero)
        {
            Destroy();
        }

        ClearEvents();
    }
}