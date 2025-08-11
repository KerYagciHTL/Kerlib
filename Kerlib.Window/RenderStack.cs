using System.Collections;
using Kerlib.Interfaces;

namespace Kerlib.Window;

public sealed class RenderStack : IEnumerable
{
    private readonly List<IRenderable> _items = [];

    public void Add(IRenderable drawable)
    {
        ArgumentNullException.ThrowIfNull(drawable);
        _items.Add(drawable);
    }

    public void Remove(IRenderable drawable)
    {
        ArgumentNullException.ThrowIfNull(drawable);
        _items.Remove(drawable);
    }

    public void DrawAll(IntPtr hdc)
    {
        foreach (var item in _items)
            item.Draw(hdc);
    }

    public IEnumerator GetEnumerator()
    {
        return _items.GetEnumerator();
    }
}