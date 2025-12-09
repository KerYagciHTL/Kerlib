using Kerlib.Interfaces;
using Kerlib.Native;

namespace Kerlib.Window;

public interface INativeWindow : IDisposable
{
    int GetWidth();
    int GetHeight();
    string GetTitle();
    void SetBackgroundColor(Color color);
    
    event Action? Resized;
    event Action? Closed;
    event Action<Key>? KeyDown;
    event Action<Key>? KeyUp;
    event Action<IReadOnlyCollection<Key>>? KeysDown;
    event Action? Tick;
    event Action<int, int>? MouseMove;
    event Action<int, int>? MouseDown;
    event Action<int, int>? MouseUp;
    event Action<int, int, int>? MouseWheel;

    void Show();
    void Destroy();
    void Add(IRenderable renderable);
    void Add(RenderStack stack);
    void Remove(IRenderable renderable);
}
