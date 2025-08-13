using System.ComponentModel;
using Kerlib.Native;
using System.Runtime.InteropServices;
using Kerlib.Interfaces;

namespace Kerlib.Window;

public class Win32Window : IDisposable
{
    protected readonly string _className;
    protected readonly IntPtr _hInstance;
    protected IntPtr _hwnd;
    protected readonly int _width;
    protected readonly int _height;

    protected readonly NativeMethods.WndProcDelegate _wndProcDelegate;

    protected bool _disposed;
    protected bool _isDestroyed;

    protected readonly RenderStack _renderStack = new();
    protected static readonly Dictionary<string, bool> RegisteredClasses = new();

    public event Action? OnResize;
    public event Action? OnClose;

    public Win32Window(string title, int width, int height)
    {
        _width = width;
        _height = height;
        _className = $"Win32Window_{Guid.NewGuid()}";
        _hInstance = NativeMethods.GetModuleHandle(null!);
        _wndProcDelegate = WndProc;

        RegisterWindowClass();
        CreateNativeWindow(title);
    }

    protected virtual void RegisterWindowClass()
    {
        if (RegisteredClasses.ContainsKey(_className)) return;

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
            hbrBackground = NativeMethods.CreateSolidBrush(NativeMethods.Rgb(240, 240, 240)),
            lpszMenuName = null!,
            lpszClassName = _className,
            hIconSm = IntPtr.Zero
        };

        var regResult = NativeMethods.RegisterClassExW(in wndClassEx);
        if (regResult == 0)
            ThrowLastWin32Error("RegisterClassEx failed");

        RegisteredClasses[_className] = true;
    }

    protected virtual void CreateNativeWindow(string title)
    {
        _hwnd = NativeMethods.CreateWindowExW(
            0,
            _className,
            title,
            NativeMethods.WsOverlappedwindow | NativeMethods.WsVisible,
            100,
            100,
            _width,
            _height,
            IntPtr.Zero,
            IntPtr.Zero,
            _hInstance,
            IntPtr.Zero);

        if (_hwnd == IntPtr.Zero)
            ThrowLastWin32Error("CreateWindowEx failed");
    }

    public virtual void Show()
    {
        NativeMethods.ShowWindow(_hwnd, NativeMethods.SwShowdefault);
        NativeMethods.UpdateWindow(_hwnd);
        Invalidate();
    }
    
    public virtual void Destroy()
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

    protected void Invalidate()
    {
        if (_hwnd != IntPtr.Zero)
            NativeMethods.InvalidateRect(_hwnd, IntPtr.Zero, true);
    }

    protected virtual IntPtr WndProc(IntPtr hwnd, uint msg, UIntPtr wParam, IntPtr lParam)
    {
        if (_isDestroyed) 
            return NativeMethods.DefWindowProcW(hwnd, msg, wParam, lParam);
            
        switch (msg)
        {
            case NativeMethods.WmPaint:
                OnPaint(hwnd);
                return IntPtr.Zero;

            case NativeMethods.WmSize:
                OnResize?.Invoke();
                return IntPtr.Zero;

            case NativeMethods.WmDestroy:
                OnClose?.Invoke();
                ClearEvents();
                _isDestroyed = true;
                _hwnd = IntPtr.Zero;
                return IntPtr.Zero;

            case NativeMethods.WmMouseMove:
                bool needsInvalidate = false;
                foreach (var r in _renderStack.OfType<IButton>())
                {
                    if (r.HandleMouseMove(GET_X_LPARAM(lParam), GET_Y_LPARAM(lParam)))
                        needsInvalidate = true;
                }

                if (needsInvalidate) Invalidate();
                return IntPtr.Zero;

            case NativeMethods.WmLButtonDown:
                foreach (var r in _renderStack.OfType<IButton>())
                {
                    r.HandleMouseDown(GET_X_LPARAM(lParam), GET_Y_LPARAM(lParam));
                }
                Invalidate();
                return IntPtr.Zero;

            case NativeMethods.WmLButtonUp:
                foreach (var r in _renderStack.OfType<IButton>())
                {
                    r.HandleMouseUp(GET_X_LPARAM(lParam), GET_Y_LPARAM(lParam));
                }
                Invalidate();
                return IntPtr.Zero;

            default:
                return NativeMethods.DefWindowProcW(hwnd, msg, wParam, lParam);
        }
    }

    private void OnPaint(IntPtr hwnd)
    {
        var ps = new NativeMethods.Paintstruct();
        var hdc = NativeMethods.BeginPaint(hwnd, out ps);
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

    protected static void ThrowLastWin32Error(string msg)
    {
        throw new Win32Exception(Marshal.GetLastWin32Error(), msg);
    }

    protected static int GET_X_LPARAM(IntPtr lParam) => (short)(lParam.ToInt32() & 0xFFFF);
    protected static int GET_Y_LPARAM(IntPtr lParam) => (short)((lParam.ToInt32() >> 16) & 0xFFFF);

    private void ClearEvents()
    {
        OnResize = null;
        OnClose = null;
    }

    public virtual void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        
        if (!_isDestroyed && _hwnd != IntPtr.Zero)
        {
            Destroy();
        }
        
        ClearEvents();
    }
}