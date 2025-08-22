using Kerlib.Interfaces;
using Kerlib.Native;

namespace Kerlib.Drawing
{ 
    public class Text : IRenderable, INotifyRenderableChanged
    {
        private Point _position;
        private string _content;
        private Color _color;
        private string _fontName;
        private int _fontSize;

        public event EventHandler? Changed;

        public Point Position
        {
            get => _position;
            set
            {
                if (ReferenceEquals(_position, value)) return;
                _position = value ?? throw new ArgumentNullException(nameof(value));
                Changed?.Invoke(this, EventArgs.Empty);
            }
        }

        public string Content
        {
            get => _content;
            set
            {
                if (string.IsNullOrEmpty(value)) throw new ArgumentException("Content cannot be null or empty.", nameof(value));
                if (_content == value) return;
                _content = value;
                Changed?.Invoke(this, EventArgs.Empty);
            }
        }

        public Color Color
        {
            get => _color;
            set
            {
                if (_color.Equals(value)) return;
                _color = value;
                Changed?.Invoke(this, EventArgs.Empty);
            }
        }

        public string FontName
        {
            get => _fontName;
            set
            {
                if (_fontName == value) return;
                _fontName = value;
                Changed?.Invoke(this, EventArgs.Empty);
            }
        }

        public int FontSize
        {
            get => _fontSize;
            set
            {
                if (_fontSize == value) return;
                _fontSize = value;
                Changed?.Invoke(this, EventArgs.Empty);
            }
        }

        public Text(Point position, string content, Color color, string fontName = "Arial", int fontSize = 16)
        {
            _position = position ?? throw new ArgumentNullException(nameof(position));
            _content = content ?? throw new ArgumentNullException(nameof(content));
            _color = color;
            _fontName = fontName;
            _fontSize = fontSize;
        }

        public void Draw(IntPtr hdc)
        {
            NativeMethods.SetTextColor(hdc, NativeMethods.Rgb(_color));
            NativeMethods.SetBkMode(hdc, 1);

            IntPtr hFont = FontCache.GetOrCreateFont(_fontName, _fontSize);
            IntPtr oldFont = NativeMethods.SelectObject(hdc, hFont);

            NativeMethods.TextOutW(hdc, _position.X, _position.Y, _content, _content.Length);

            NativeMethods.SelectObject(hdc, oldFont);
        }
    }
}
