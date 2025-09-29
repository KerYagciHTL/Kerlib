using System.Collections;
using Kerlib.Interfaces;

namespace Kerlib.Window;

public sealed class RenderStack : IEnumerable<IRenderable>
{
    private readonly List<IRenderable> _items = [];

    public int Count => _items.Count;

    public void Add(IRenderable drawable)
    {
        ArgumentNullException.ThrowIfNull(drawable);
        _items.Add(drawable);
    }

    public void AddRange(IEnumerable<IRenderable> drawables)
    {
        foreach (var d in drawables) Add(d);
    }

    public bool Remove(IRenderable drawable)
    {
        ArgumentNullException.ThrowIfNull(drawable);
        return _items.Remove(drawable);
    }

    public void DrawAll(IntPtr rt)
    {
        // Iteration per Index (leichter schneller als foreach auf List-Enumerator)
        for (int i = 0; i < _items.Count; i++)
            _items[i].Draw(rt);
    }

    public void Clear() => _items.Clear();

    public IEnumerator<IRenderable> GetEnumerator() => _items.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}