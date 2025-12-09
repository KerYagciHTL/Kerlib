using Kerlib.Native;
using System.Runtime.InteropServices;

namespace Kerlib.Core;

public static class WindowManager
{

    private static Window? _currentWindow;
    private static bool _shouldQuit = false;

    public static void RegisterWindow(Window window)
    {
        if (_currentWindow != null)
        {
            throw new InvalidOperationException("A window is already registered.");
        }
        
        _currentWindow = window;
        _currentWindow.Closed += OnWindowClose;
        _currentWindow.Show();
    }
    
    public static void SwitchWindow(Window window, bool closePrevious = true)
    {
        if (_currentWindow != null)
        {
            _currentWindow.Closed -= OnWindowClose;

            if (closePrevious)
            {
                _currentWindow.GetNativeWindow().Destroy();
                _currentWindow.Dispose();
            }
        }

        _currentWindow = window;
        _currentWindow.Closed += OnWindowClose;
        _currentWindow.Show();
    }

    private static void OnWindowClose()
    {
        _shouldQuit = true;
    }

    public static void Run()
    {
        while (!_shouldQuit)
        {
            if (!NativeMethods.GetMessage(out var msg, IntPtr.Zero, 0, 0)) continue;
            NativeMethods.TranslateMessage(ref msg);
            NativeMethods.DispatchMessage(ref msg);
        }

        if (_currentWindow == null) return;
        _currentWindow.Dispose();
        _currentWindow = null;
    }
}