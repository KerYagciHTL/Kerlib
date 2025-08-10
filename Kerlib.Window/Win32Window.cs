using Kerlib.Native;
using System.Runtime.InteropServices;

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
        _hInstance = NativeMethods.GetModuleHandle(null);
        _wndProcDelegate = WndProc;

        RegisterWindowClass();
        CreateNativeWindow(title);
    }

    private void RegisterWindowClass()
    {
        var wndClassEx = new NativeMethods.WNDCLASSEXW
        {
            cbSize = (uint)Marshal.SizeOf<NativeMethods.WNDCLASSEXW>(),
            style = NativeMethods.CS_HREDRAW | NativeMethods.CS_VREDRAW,
            lpfnWndProc = Marshal.GetFunctionPointerForDelegate(_wndProcDelegate),
            cbClsExtra = 0,
            cbWndExtra = 0,
            hInstance = _hInstance,
            hIcon = IntPtr.Zero,
            hCursor = IntPtr.Zero,
            hbrBackground = (IntPtr)(NativeMethods.COLOR_WINDOW + 1),
            lpszMenuName = null,
            lpszClassName = _className,
            hIconSm = IntPtr.Zero
        };

        ushort regResult = NativeMethods.RegisterClassExW(in wndClassEx);
        if (regResult == 0)
            ThrowLastWin32Error("RegisterClassEx failed");
    }

    private void CreateNativeWindow(string title)
    {
        _hwnd = NativeMethods.CreateWindowExW(
            0,
            _className,
            title,
            NativeMethods.WS_OVERLAPPEDWINDOW | NativeMethods.WS_VISIBLE,
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
        NativeMethods.ShowWindow(_hwnd, NativeMethods.SW_SHOWDEFAULT);
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
            case NativeMethods.WM_PAINT:
                OnPaint(hwnd);
                break;

            case NativeMethods.WM_SIZE:
                OnResize?.Invoke();
                break;

            case NativeMethods.WM_DESTROY:
                OnClose?.Invoke();
                NativeMethods.PostQuitMessage(0);
                break;

            default:
                return NativeMethods.DefWindowProcW(hwnd, msg, wParam, lParam);
        }

        return IntPtr.Zero;
    }

    private void OnPaint(IntPtr hwnd)
    {
        var ps = new NativeMethods.PAINTSTRUCT();
        IntPtr hdc = NativeMethods.BeginPaint(hwnd, out ps);
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
        throw new System.ComponentModel.Win32Exception(errCode, msg);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        if (_hwnd != IntPtr.Zero)
        {
            NativeMethods.PostQuitMessage(0);
            _hwnd = IntPtr.Zero;
        }
    }
}