using Kerlib.Interfaces;
using Kerlib.Window;

namespace Kerlib.Core;

public abstract class Window : IDisposable
{
    public int Width => _window.GetWidth();
    public int Height => _window.GetHeight();
    public string Title => _window.GetTitle();
    
    private readonly Win32Window _window;
    public event Action? OnResize
    {
        add => _window.OnResize += value;
        remove => _window.OnResize -= value;
    }

    public event Action? OnClose
    {
        add => _window.OnClose += value;
        remove => _window.OnClose -= value;
    }

    protected Window(string title, int width, int height)
    {
        _window = new Win32Window(title, width, height);
    }

    internal Win32Window GetWin32Window() => _window;

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