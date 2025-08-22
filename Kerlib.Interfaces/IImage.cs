using Kerlib.Native;

namespace Kerlib.Interfaces;

public interface IImage : INotifyRenderableChanged, IRenderable, IDisposable
{
    Point Position { get; set; }
    int Width { get; set; }
    int Height { get; set; }
    string Path { get; }
}