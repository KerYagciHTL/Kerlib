namespace Kerlib.Native;

public static class FontCache
{
    private static readonly Dictionary<(string name, int size), IntPtr> Fonts = new();
    private static bool _disposed;

    public static IntPtr GetOrCreateFont(string name, int size)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(FontCache));
        var key = (name, size);
        if (Fonts.TryGetValue(key, out var font)) return font;

        font = NativeMethods.CreateFont(
            -size, 0, 0, 0, 400,
            0, 0, 0, 1, 0, 0, 0, 0,
            name);

        Fonts[key] = font;
        return font;

    }

    public static void Dispose()
    {
        if (_disposed) return;
        foreach (var f in Fonts.Values)
            NativeMethods.DeleteObject(f);
        Fonts.Clear();
        _disposed = true;
    }
}