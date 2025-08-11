using System.ComponentModel;
using Kerlib.Native;
using System.Runtime.InteropServices;
using Kerlib.Interfaces;

namespace Kerlib.Window;

public sealed class Win32Window : IDisposable
{
    private readonly string _className = "Win32Window";
    private readonly IntPtr _hInstance;
    private IntPtr _hwnd;
    private readonly int _width;
    private readonly int _height;

    private readonly NativeMethods.WndProcDelegate _wndProcDelegate;

    private bool _disposed;

    private readonly RenderStack _renderStack = new();

    public event Action? OnResize;
    public event Action? OnClose;

    public Win32Window(string title, int width, int height)
    {
        _width = width;
        _height = height;
        _hInstance = NativeMethods.GetModuleHandle(null!);
        _wndProcDelegate = WndProc;

        RegisterWindowClass();
        CreateNativeWindow(title);
    }
    
    private void RegisterWindowClass()
    {
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
            hbrBackground = NativeMethods.ColorWindow + 1,
            lpszMenuName = null!,
            lpszClassName = _className,
            hIconSm = IntPtr.Zero
        };

        var regResult = NativeMethods.RegisterClassExW(in wndClassEx);
        if (regResult == 0)
            ThrowLastWin32Error("RegisterClassEx failed");
    }

    private void CreateNativeWindow(string title)
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

    public void Show()
    {
        NativeMethods.ShowWindow(_hwnd, NativeMethods.SwShowdefault);
        NativeMethods.UpdateWindow(_hwnd);
    }

    public void RunMessageLoop()
    {
        while (NativeMethods.GetMessage(out var msg, IntPtr.Zero, 0, 0))
        {
            NativeMethods.TranslateMessage(ref msg);
            NativeMethods.DispatchMessage(ref msg);
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
            if (drawable is IRenderable renderable) Add(renderable);
            else ThrowLastWin32Error("Object not type IRenderable wanted to be added to render stack");
        }
    }

    public void Remove(IRenderable drawable)
    {
        _renderStack.Remove(drawable);
        Invalidate();
    }

    private void Invalidate()
    {
        NativeMethods.InvalidateRect(_hwnd, IntPtr.Zero, true);
    }
    private IntPtr WndProc(IntPtr hwnd, uint msg, UIntPtr wParam, IntPtr lParam)
    {
        switch (msg)
        {
            case NativeMethods.WmPaint:
                OnPaint(hwnd);
                break;

            case NativeMethods.WmSize:
                OnResize?.Invoke();
                break;

            case NativeMethods.WmDestroy:
                OnClose?.Invoke();
                NativeMethods.PostQuitMessage(0);
                break;

            case NativeMethods.WmMouseMove:
                var needsInvalidate = false;
                foreach (var r in _renderStack.OfType<IButton>())
                {
                    if (r.HandleMouseMove(GET_X_LPARAM(lParam), GET_Y_LPARAM(lParam)))
                        needsInvalidate = true;
                }
                if (needsInvalidate)
                    Invalidate();
                break;


            case NativeMethods.WmLButtonDown:
                foreach (var r in _renderStack.OfType<IButton>())
                {
                    r.HandleMouseDown(GET_X_LPARAM(lParam), GET_Y_LPARAM(lParam));
                    Invalidate();
                }

                break;

            case NativeMethods.WmLButtonUp:
                foreach (var r in _renderStack.OfType<IButton>())
                {
                    r.HandleMouseUp(GET_X_LPARAM(lParam), GET_Y_LPARAM(lParam));
                    Invalidate();
                }

                break;

            default:
                return NativeMethods.DefWindowProcW(hwnd, msg, wParam, lParam);
        }

        return IntPtr.Zero;
    }
    private void OnPaint(IntPtr hwnd)
    {
        var hdc = NativeMethods.BeginPaint(hwnd, out var ps);
        if (hdc == IntPtr.Zero)
            return;

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
        var errCode = Marshal.GetLastWin32Error();
        throw new Win32Exception(errCode, msg);
    }
    private static int GET_X_LPARAM(IntPtr lParam) => (short)(lParam.ToInt32() & 0xFFFF);
    private static int GET_Y_LPARAM(IntPtr lParam) => (short)((lParam.ToInt32() >> 16) & 0xFFFF);
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        if (_hwnd == IntPtr.Zero) return;
        NativeMethods.PostQuitMessage(0);
        _hwnd = IntPtr.Zero;
    }
}
