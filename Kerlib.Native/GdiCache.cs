using System;
using System.Collections.Generic;
using System.Threading;

namespace Kerlib.Native
{
    public static class GdiCache
    {
        private static readonly Dictionary<uint, IntPtr> Brushes = new();
        private static readonly Dictionary<(int width, uint color), IntPtr> Pens = new();
        private static bool _disposed;
        private static readonly object Sync = new();

        public static IntPtr GetOrCreateBrush(uint color)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(GdiCache));
            lock (Sync)
            {
                if (Brushes.TryGetValue(color, out var brush)) return brush;
                brush = NativeMethods.CreateSolidBrush(color);
                Brushes[color] = brush;
                return brush;
            }
        }

        public static IntPtr GetOrCreatePen(int width, uint color)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(GdiCache));
            lock (Sync)
            {
                var key = (width, color);
                if (Pens.TryGetValue(key, out var pen)) return pen;
                pen = NativeMethods.CreatePen(0, width, color);
                Pens[key] = pen;
                return pen;
            }
        }

        public static void Dispose()
        {
            if (_disposed) return;
            lock (Sync)
            {
                if (_disposed) return;
                foreach (var pen in Pens.Values)
                    NativeMethods.DeleteObject(pen);
                Pens.Clear();

                foreach (var brush in Brushes.Values)
                    NativeMethods.DeleteObject(brush);
                Brushes.Clear();

                _disposed = true;
            }
        }
    }
}