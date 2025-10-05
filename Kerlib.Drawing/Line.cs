using Kerlib.Interfaces;
using Kerlib.Native;

namespace Kerlib.Drawing
{
    public sealed class Line : IRenderable, INotifyRenderableChanged, IDisposable
    {
        private Point _start;
        private Point _end;
        private uint _color;
        private bool _disposed;

        public event EventHandler? Changed;

        public Point Start
        {
            get => _start;
            set
            {
                if (ReferenceEquals(_start, value)) return;
                _start.Changed -= OnPointChanged;
                _start = value ?? throw new ArgumentNullException(nameof(value));
                _start.Changed += OnPointChanged;
                Changed?.Invoke(this, EventArgs.Empty);
            }
        }

        public Point End
        {
            get => _end;
            set
            {
                if (ReferenceEquals(_end, value)) return;
                _end.Changed -= OnPointChanged;
                _end = value ?? throw new ArgumentNullException(nameof(value));
                _end.Changed += OnPointChanged;
                Changed?.Invoke(this, EventArgs.Empty);
            }
        }

        public Color Color
        {
            get => Color.FromRgb(_color);
            set
            {
                var rgb = NativeMethods.Rgb(value);
                if (_color == rgb) return;
                _color = rgb;
                Changed?.Invoke(this, EventArgs.Empty);
            }
        }

        public Line(Point start, Point end, Color color)
        {
            _start = start ?? throw new ArgumentNullException(nameof(start));
            _end = end ?? throw new ArgumentNullException(nameof(end));
            _color = NativeMethods.Rgb(color);

            _start.Changed += OnPointChanged;
            _end.Changed += OnPointChanged;
        }

        private void OnPointChanged(object? sender, EventArgs e)
        {
            if (_disposed) return;
            Changed?.Invoke(this, EventArgs.Empty);
        }

        public void Draw(IntPtr hdc)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(Line));
            var pen = GdiCache.GetOrCreatePen(1, _color);
            var oldPen = NativeMethods.SelectObject(hdc, pen);
            NativeMethods.MoveToEx(hdc, _start.X, _start.Y, IntPtr.Zero);
            NativeMethods.LineTo(hdc, _end.X, _end.Y);
            NativeMethods.SelectObject(hdc, oldPen);
        }

        public void Dispose()
        {
            if (_disposed) return;

            _start.Changed -= OnPointChanged;
            _end.Changed -= OnPointChanged;

            Changed = null;

            _disposed = true;
        }
    }
}
