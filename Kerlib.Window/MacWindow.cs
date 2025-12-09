using Kerlib.Interfaces;
using Kerlib.Native;

namespace Kerlib.Window;

public sealed class MacWindow : INativeWindow
{
    private readonly string _title;
    private int _width;
    private int _height;
    private Color _backgroundColor = Color.White;

    public MacWindow(string title, int width, int height, Color? bgColor = null)
    {
        _title = title;
        _width = width;
        _height = height;
        if (bgColor.HasValue) _backgroundColor = bgColor.Value;
        
        // TODO: Implement Cocoa window creation
        Console.WriteLine($"[Mac] Window created: {title}");
    }

    public int GetWidth() => _width;
    public int GetHeight() => _height;
    public string GetTitle() => _title;
    public void SetBackgroundColor(Color color) => _backgroundColor = color;

#pragma warning disable CS0067
    public event Action? Resized;
    public event Action? Closed;
    public event Action<Key>? KeyDown;
    public event Action<Key>? KeyUp;
    public event Action<IReadOnlyCollection<Key>>? KeysDown;
    public event Action? Tick;
    public event Action<int, int>? MouseMove;
    public event Action<int, int>? MouseDown;
    public event Action<int, int>? MouseUp;
    public event Action<int, int, int>? MouseWheel;
#pragma warning restore CS0067

    public void Show() 
    {
        // TODO: Show window
        Console.WriteLine("[Mac] Window shown");
    }

    public void Destroy() 
    {
        Closed?.Invoke();
    }

    public void Add(IRenderable renderable) { }
    public void Add(RenderStack stack) { }
    public void Remove(IRenderable renderable) { }

    public void Dispose() 
    {
        Destroy();
    }
}
