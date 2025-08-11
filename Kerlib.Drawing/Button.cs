using Kerlib.Interfaces;
using Kerlib.Native;

namespace Kerlib.Drawing
{
    public class Button : IButton
    {
        private readonly int _x, _y, _width, _height;
        private bool _hovered;
        private bool _pressed;

        private uint _bgNormal;
        private uint _bgHover;
        private uint _bgPressed;
        private uint _textColor;

        public string Text { get; set; }
        public bool IsHovered => _hovered;
        public bool IsPressed => _pressed;

        public event Action<IButton>? Clicked;
        public event Action<IButton>? MouseEnter;
        public event Action<IButton>? MouseLeave;
        public event Action<IButton>? MouseDown;
        public event Action<IButton>? MouseUp;

        public Button(int x, int y, int width, int height, string text)
        {
            _x = x;
            _y = y;
            _width = width;
            _height = height;
            Text = text;

            // Default colors
            _bgNormal = NativeMethods.Rgb(200, 200, 200);
            _bgHover = NativeMethods.Rgb(180, 180, 180);
            _bgPressed = NativeMethods.Rgb(160, 160, 160);
            _textColor = NativeMethods.Rgb(0, 0, 0);
        }

        public void Draw(IntPtr hdc)
        {
            // Hintergrundfarbe auswählen
            uint bgColor = _pressed ? _bgPressed : _hovered ? _bgHover : _bgNormal;

            IntPtr brush = NativeMethods.CreateSolidBrush(bgColor);
            IntPtr oldBrush = NativeMethods.SelectObject(hdc, brush);

            IntPtr pen = NativeMethods.CreatePen(0, 1, NativeMethods.Rgb(0, 0, 0));
            IntPtr oldPen = NativeMethods.SelectObject(hdc, pen);

            NativeMethods.Rectangle(hdc, _x, _y, _x + _width, _y + _height);

            NativeMethods.SelectObject(hdc, oldBrush);
            NativeMethods.DeleteObject(brush);

            NativeMethods.SelectObject(hdc, oldPen);
            NativeMethods.DeleteObject(pen);

            var rect = new NativeMethods.Rect
            {
                left = _x,
                top = _y,
                right = _x + _width,
                bottom = _y + _height
            };

            NativeMethods.SetTextColor(hdc, _textColor);
            NativeMethods.SetBkMode(hdc, 1); // TRANSPARENT
            NativeMethods.DrawText(hdc, Text, Text.Length, ref rect, NativeMethods.DtCenter | NativeMethods.DtVcenter | NativeMethods.DT_SINGLELINE);
        }

        public void HandleMouseMove(int x, int y)
        {
            bool inside = Contains(x, y);
            if (inside && !_hovered)
            {
                _hovered = true;
                MouseEnter?.Invoke(this);
            }
            else if (!inside && _hovered)
            {
                _hovered = false;
                MouseLeave?.Invoke(this);
            }
        }

        public void HandleMouseDown(int x, int y)
        {
            if (Contains(x, y))
            {
                _pressed = true;
                MouseDown?.Invoke(this);
            }
        }

        public void HandleMouseUp(int x, int y)
        {
            if (_pressed)
            {
                _pressed = false;
                MouseUp?.Invoke(this);
                if (Contains(x, y))
                {
                    Clicked?.Invoke(this);
                }
            }
        }

        private bool Contains(int x, int y) =>
            x >= _x && x <= _x + _width && y >= _y && y <= _y + _height;
    }
}
