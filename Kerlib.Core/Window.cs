using Kerlib.Interfaces;
using Kerlib.Native;
using Kerlib.Window;
using System.Runtime.InteropServices;

namespace Kerlib.Core;

public abstract class Window : IDisposable
{
    public int Width => _window.GetWidth();
    public int Height => _window.GetHeight();
    public string Title => _window.GetTitle();
    
    public Color BackgroundColor
    {
        get => _backgroundColor;
        set
        {
            _backgroundColor = value;
            _window.SetBackgroundColor(value);
        }
    }
    
    private Color _backgroundColor = Color.White;
    
    private readonly INativeWindow _window;
    public event Action? Resized
    {
        add => _window.Resized += value;
        remove => _window.Resized -= value;
    }

    public event Action? Closed
    {
        add => _window.Closed += value;
        remove => _window.Closed -= value;
    }
    
    public event Action<Key>? KeyDown
    {
        add => _window.KeyDown += value;
        remove => _window.KeyDown -= value;
    }
    
    public event Action<Key>? KeyUp
    {
        add => _window.KeyUp += value;
        remove => _window.KeyUp -= value;
    }
    
    public event Action? Tick
    {
        add => _window.Tick += value;
        remove => _window.Tick -= value;
    }

    public event Action<IReadOnlyCollection<Key>>? KeysDown
    {
        add => _window.KeysDown += value;
        remove => _window.KeysDown -= value;
    }
    public event Action<int, int>? MouseMove
    {
        add => _window.MouseMove += value;
        remove => _window.MouseMove -= value;
    }
    public event Action<int, int>? MouseDown
    {
        add => _window.MouseDown += value;
        remove => _window.MouseDown -= value;
    }
    public event Action<int, int>? MouseUp
    {
        add => _window.MouseUp += value;
        remove => _window.MouseUp -= value;
    }
    public event Action<int, int, int>? MouseWheel
    {
        add => _window.MouseWheel += value;
        remove => _window.MouseWheel -= value;
    }

    protected Window(string title, int width, int height)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            _window = new Win32Window(title, width, height, _backgroundColor);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            _window = new LinuxWindow(title, width, height, _backgroundColor);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            _window = new MacWindow(title, width, height, _backgroundColor);
        }
        else
        {
            throw new PlatformNotSupportedException("Operating system not supported.");
        }
    }

    internal INativeWindow GetNativeWindow() => _window;

    protected internal void Show()
    {
        _window.Show();
    }

    public void Add(IRenderable renderable)
    {
        _window.Add(renderable);
    }

    public void Add(RenderStack stack)
    {
        _window.Add(stack);
    }

    public void Remove(IRenderable renderable)
    {
        _window.Remove(renderable);
    }

    public void Switch(Window other, bool closePrevious = true)
    {
        WindowManager.SwitchWindow(other, closePrevious);
    }
    public void Close()
    {
        _window.Destroy();
    }
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _window.Dispose();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}