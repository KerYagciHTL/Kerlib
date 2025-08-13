namespace Kerlib.Core;

public static class WindowManager
{
    private static Window? _currentWindow;
    public static void RegisterWindow(Window window)
    {
        if (_currentWindow != null)
        {
            throw new InvalidOperationException("A window is already registered. Please close the current window before registering a new one.");
        }
        
        ArgumentNullException.ThrowIfNull(window);
        _currentWindow = window;
        _currentWindow.Show();
    }
    public static void SwitchWindow(Window window)
    {
        ArgumentNullException.ThrowIfNull(window);
        _currentWindow?.Dispose();
        _currentWindow = window;
        _currentWindow.Show();
    }

    public static Window? GetCurrentWindow() => _currentWindow;
}