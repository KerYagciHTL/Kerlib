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
        while (!_shouldQuit && _currentWindow != null)
        {
            _currentWindow.GetNativeWindow().ProcessMessages();
        }

        if (_currentWindow == null) return;
        _currentWindow.Dispose();
        _currentWindow = null;
    }
}