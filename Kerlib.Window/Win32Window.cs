using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Kerlib.Interfaces;
using Kerlib.Native;

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
    private readonly HashSet<Key> _keysDown = [];

    public event Action? Resized;
    public event Action? Closed;
    public event Action<Key>? KeyDown;
    public event Action<Key>? KeyUp;
    public event Action<IReadOnlyCollection<Key>>? KeysDown;
    public event Action? Tick;

    public Win32Window(string title, int width, int height, Color? bgColor = null)
    {
        PlatformGuard.EnsureSupported();
        _title = title;
        _height = height;
        _width = width;
        _className = $"Win32Window_{Guid.NewGuid()}";
        _hInstance = NativeMethods.GetModuleHandle(null!);
        _wndProcDelegate = WndProc;
        _backgroundColor = bgColor ?? _backgroundColor;
        RegisterWindowClass();
        CreateNativeWindow(title, width, height);
        NativeMethods.SetTimer(_hwnd, 1, 16, IntPtr.Zero);
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
        if (NativeMethods.RegisterClassExW(in wndClassEx) == 0) ThrowLastWin32Error("RegisterClassEx failed");
        RegisteredClasses[_className] = true;
    }

    private void CreateNativeWindow(string title, int clientWidth, int clientHeight)
    {
        var rect = new NativeMethods.Rect { left = 0, top = 0, right = clientWidth, bottom = clientHeight };
        if (!NativeMethods.AdjustWindowRectEx(ref rect, NativeMethods.WsOverlappedwindow, false, 0)) ThrowLastWin32Error("AdjustWindowRectEx failed");
        _hwnd = NativeMethods.CreateWindowExW(0, _className, title, NativeMethods.WsOverlappedwindow | NativeMethods.WsVisible, 100, 100, rect.right - rect.left, rect.bottom - rect.top, IntPtr.Zero, IntPtr.Zero, _hInstance, IntPtr.Zero);
        if (_hwnd == IntPtr.Zero) ThrowLastWin32Error("CreateWindowEx failed");
    }

    public void Show()
    {
        NativeMethods.ShowWindow(_hwnd, NativeMethods.SwShowdefault);
        NativeMethods.UpdateWindow(_hwnd);
        Invalidate();
    }

    public void Destroy()
    {
        if (_hwnd == IntPtr.Zero || _isDestroyed) return;
        NativeMethods.DestroyWindow(_hwnd);
        _isDestroyed = true;
        _hwnd = IntPtr.Zero;
    }

    public void Add(IRenderable drawable)
    {
        if (drawable is INotifyRenderableChanged notifyRenderable) notifyRenderable.Changed += OnNotifyRenderable;
        _renderStack.Add(drawable);
        Invalidate();
    }

    public void Add(RenderStack stack)
    {
        foreach (var drawable in stack)
            Add(drawable);
    }

    public void Remove(IRenderable drawable)
    {
        if (drawable is INotifyRenderableChanged notifyRenderable) notifyRenderable.Changed -= OnNotifyRenderable;
        if(drawable is IImage image) image.Dispose();
        _renderStack.Remove(drawable);
        Invalidate();
    }
    private void Invalidate()
    {
        if (_hwnd != IntPtr.Zero) NativeMethods.InvalidateRect(_hwnd, IntPtr.Zero, true);
    }

    private IntPtr WndProc(IntPtr hwnd, uint msg, UIntPtr wParam, IntPtr lParam)
    {
        if (_isDestroyed) return NativeMethods.DefWindowProcW(hwnd, msg, wParam, lParam);
        switch (msg)
        {
            case NativeMethods.WmErasebkgnd:
                return new IntPtr(1);
            case NativeMethods.WmPaint:
                OnPaint(hwnd);
                return IntPtr.Zero;
            case NativeMethods.WmKeyDown:
            {
                var key = Key.FromVirtualCode((int)wParam);
                var wasDown = _keysDown.Contains(key);
                if (!wasDown)
                {
                    _keysDown.Add(key);
                    KeyDown?.Invoke(key);
                    KeysDown?.Invoke(GetPressedKeys());
                }
                Invalidate();
                return IntPtr.Zero;
            }
            case NativeMethods.WmKeyUp:
            {
                var key = Key.FromVirtualCode((int)wParam);
                if (!_keysDown.Remove(key)) return IntPtr.Zero;
                KeyUp?.Invoke(key);
                KeysDown?.Invoke(GetPressedKeys());
                Invalidate();
                return IntPtr.Zero;
            }
            case NativeMethods.WmTimer:
                KeysDown?.Invoke(GetPressedKeys());
                Tick?.Invoke();
                Invalidate();
                return IntPtr.Zero;
            case NativeMethods.WmSize:
                if (_hwnd != IntPtr.Zero && NativeMethods.GetClientRect(_hwnd, out var rect)) { _width = rect.right - rect.left; _height = rect.bottom - rect.top; }
                Resized?.Invoke();
                return IntPtr.Zero;
            case NativeMethods.WmKillFocus:
                ResetKeyState(fireKeyUp: true);
                Invalidate();
                return IntPtr.Zero;
            case NativeMethods.WmDestroy:
                Closed?.Invoke();
                ResetKeyState(fireKeyUp: true);
                ClearEvents();
                _isDestroyed = true;
                _hwnd = IntPtr.Zero;
                return IntPtr.Zero;
            case NativeMethods.WmMouseMove:
            {
                var needsInvalidate = false;
                var buttons = _renderStack.SnapshotOfType<IButton>();
                for (int i = 0; i < buttons.Length; i++) if (buttons[i].HandleMouseMove(GET_X_LPARAM(lParam), GET_Y_LPARAM(lParam))) needsInvalidate = true;
                var inputs = _renderStack.SnapshotOfType<IInputField>();
                for (int i = 0; i < inputs.Length; i++) if (inputs[i].HandleMouseMove(GET_X_LPARAM(lParam), GET_Y_LPARAM(lParam))) needsInvalidate = true;
                if (needsInvalidate) Invalidate();
                return IntPtr.Zero;
            }
            case NativeMethods.WmLButtonDown:
            {
                var buttons = _renderStack.SnapshotOfType<IButton>();
                for (int i = 0; i < buttons.Length; i++) buttons[i].HandleMouseDown(GET_X_LPARAM(lParam), GET_Y_LPARAM(lParam));
                var inputs = _renderStack.SnapshotOfType<IInputField>();
                for (int i = 0; i < inputs.Length; i++) inputs[i].HandleMouseDown(GET_X_LPARAM(lParam), GET_Y_LPARAM(lParam));
                Invalidate();
                return IntPtr.Zero;
            }
            case NativeMethods.WmLButtonUp:
            {
                var buttons = _renderStack.SnapshotOfType<IButton>();
                for (int i = 0; i < buttons.Length; i++) buttons[i].HandleMouseUp(GET_X_LPARAM(lParam), GET_Y_LPARAM(lParam));
                Invalidate();
                return IntPtr.Zero;
            }
            case NativeMethods.WmKeyPress:
            {
                var inputs = _renderStack.SnapshotOfType<IInputField>();
                for (int i = 0; i < inputs.Length; i++) inputs[i].HandleKeyPress((char)wParam);
                Invalidate();
                return IntPtr.Zero;
            }
            default:
                return NativeMethods.DefWindowProcW(hwnd, msg, wParam, lParam);
        }
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
        if (_hwnd != IntPtr.Zero)
            NativeMethods.SetClassLongPtr(_hwnd, NativeMethods.GclpHbrbackground, _hBackgroundBrush);
        Invalidate();
    }

    private void OnPaint(IntPtr hwnd)
    {
        var hdc = NativeMethods.BeginPaint(hwnd, out var ps);
        if (hdc == IntPtr.Zero) return;

        try
        {
            if (!NativeMethods.GetClientRect(hwnd, out var rect)) return;
            int w = rect.right - rect.left;
            int h = rect.bottom - rect.top;

            var memDc = NativeMethods.CreateCompatibleDC(hdc);
            var hBmp = NativeMethods.CreateCompatibleBitmap(hdc, w, h);
            var oldBmp = NativeMethods.SelectObject(memDc, hBmp);

            IntPtr bgBrush = _hBackgroundBrush != IntPtr.Zero ? _hBackgroundBrush : GdiCache.GetOrCreateBrush(NativeMethods.Rgb(_backgroundColor));
            NativeMethods.FillRect(memDc, ref rect, bgBrush);

            NativeMethods.SetBkMode(memDc, 1);
            _renderStack.DrawAll(memDc);

            NativeMethods.BitBlt(hdc, 0, 0, w, h, memDc, 0, 0, NativeMethods.Srccopy);

            NativeMethods.SelectObject(memDc, oldBmp);
            NativeMethods.DeleteObject(hBmp);
            NativeMethods.DeleteDC(memDc);
        }
        finally
        {
            NativeMethods.EndPaint(hwnd, ref ps);
        }
    }


    private static void ThrowLastWin32Error(string msg) => throw new Win32Exception(Marshal.GetLastWin32Error(), msg);

    private static int GET_X_LPARAM(IntPtr lParam) => (short)(lParam.ToInt32() & 0xFFFF);
    private static int GET_Y_LPARAM(IntPtr lParam) => (short)((lParam.ToInt32() >> 16) & 0xFFFF);

    public int GetHeight() => _height;
    public int GetWidth() => _width;
    public string GetTitle() => _title;

    private void OnNotifyRenderable(object? sender, EventArgs e) => Invalidate();

    private void CleanRenderStack()
    {
        foreach (var r in _renderStack)
        {
            switch (r)
            {
                case INotifyRenderableChanged notifyRenderable: notifyRenderable.Changed -= OnNotifyRenderable; break;
                case IDisposable disposable: disposable.Dispose(); break;
            }
        }
        _renderStack.Clear();
    }

    private void ClearEvents()
    {
        Resized = null;
        Closed = null;
        KeyDown = null;
        KeyUp = null;
        KeysDown = null;
        Tick = null;
        CleanRenderStack();
    }

    private void ResetKeyState(bool fireKeyUp)
    {
        if (_keysDown.Count == 0) return;
        if (fireKeyUp)
        {
            foreach (var key in _keysDown)
                KeyUp?.Invoke(key);
        }
        _keysDown.Clear();
        KeysDown?.Invoke(GetPressedKeys());
    }

    public bool IsKeyDown(Key key) => _keysDown.Contains(key);
    public IReadOnlyCollection<Key> GetPressedKeys() => _keysDown;

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        if (_hBackgroundBrush != IntPtr.Zero) { NativeMethods.DeleteObject(_hBackgroundBrush); _hBackgroundBrush = IntPtr.Zero; }
        NativeMethods.KillTimer(_hwnd, 1);
        if (!_isDestroyed && _hwnd != IntPtr.Zero) Destroy();
        ResetKeyState(fireKeyUp: false);
        ClearEvents();
    }
}
