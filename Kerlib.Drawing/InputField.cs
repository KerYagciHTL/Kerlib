using Kerlib.Interfaces;
using Kerlib.Native;

namespace Kerlib.Drawing
{
    public class InputField : IInputField
    {
        private readonly int _x, _y, _width, _height;
        private bool _hovered;
        private bool _focused;
        private string _text = "";
        private int _cursorPos = 0;
        private DateTime _lastBlink = DateTime.Now;
        private bool _cursorVisible = true;

        private uint BgNormal => NativeMethods.Rgb(BackgroundNormal);
        private uint BgHover => NativeMethods.Rgb(BackgroundHover);
        private uint BgFocused => NativeMethods.Rgb(BackgroundFocused);
        private uint Fg => NativeMethods.Rgb(Foreground);

        public Point Position => new(_x, _y);
        public Color BackgroundNormal { get; set; }
        public Color BackgroundHover { get; set; }
        public Color BackgroundFocused { get; set; }
        public Color Foreground { get; set; }
        public string Text
        {
            get => _text;
            set
            {
                _text = value;
                _cursorPos = Math.Min(_cursorPos, _text.Length);
            }
        }

        public bool IsFocused => _focused;

        public event EventHandler? TextChanged;
        public event EventHandler? FocusGained;
        public event EventHandler? FocusLost;

        public InputField(Point pos, int width, int height)
        {
            _x = pos.X;
            _y = pos.Y;
            _width = width;
            _height = height;

            BackgroundNormal = new Color(255, 255, 255);
            BackgroundHover = new Color(230, 230, 230);
            BackgroundFocused = new Color(200, 200, 255);
            Foreground = new Color(0, 0, 0);
        }

        public void Draw(IntPtr hdc)
        {
            var bgColor = _focused ? BgFocused : _hovered ? BgHover : BgNormal;
            var brush = NativeMethods.CreateSolidBrush(bgColor);
            var oldBrush = NativeMethods.SelectObject(hdc, brush);

            var pen = NativeMethods.CreatePen(0, 1, NativeMethods.Rgb(0, 0, 0));
            var oldPen = NativeMethods.SelectObject(hdc, pen);

            NativeMethods.Rectangle(hdc, _x, _y, _x + _width, _y + _height);

            NativeMethods.SelectObject(hdc, oldBrush);
            NativeMethods.DeleteObject(brush);
            NativeMethods.SelectObject(hdc, oldPen);
            NativeMethods.DeleteObject(pen);

            var rect = new NativeMethods.Rect
            {
                left = _x + 4,
                top = _y,
                right = _x + _width - 4,
                bottom = _y + _height
            };

            NativeMethods.SetTextColor(hdc, Fg);
            NativeMethods.SetBkMode(hdc, 1); // TRANSPARENT
            NativeMethods.DrawText(hdc, _text, _text.Length, ref rect,
                NativeMethods.DtLeft | NativeMethods.DtVcenter | NativeMethods.DtSingleline);

            if (_focused && (DateTime.Now - _lastBlink).TotalMilliseconds > 500)
            {
                _cursorVisible = !_cursorVisible;
                _lastBlink = DateTime.Now;
            }

            if (!_focused || !_cursorVisible) return;
            var cursorX = _x + 4 + NativeMethods.GetTextWidth(hdc, _text.Substring(0, _cursorPos));
            NativeMethods.MoveToEx(hdc, cursorX, _y + 2, IntPtr.Zero);
            NativeMethods.LineTo(hdc, cursorX, _y + _height - 2);
        }

        public bool HandleMouseMove(int x, int y)
        {
            var inside = Contains(x, y);
            switch (inside)
            {
                case true when !_hovered:
                    _hovered = true;
                    return true;
                case false when _hovered:
                    _hovered = false;
                    return true;
                default:
                    return false;
            }
        }

        public void HandleMouseDown(int x, int y)
        {
            var inside = Contains(x, y);
            if (inside)
            {
                if (!_focused)
                {
                    _focused = true;
                    FocusGained?.Invoke(this, EventArgs.Empty);
                }
            }
            else
            {
                if (!_focused) return;
                _focused = false;
                FocusLost?.Invoke(this, EventArgs.Empty);
            }
        }

        public void HandleKeyPress(char key)
        {
            if (!_focused) return;

            if (key == '\b') // Backspace
            {
                if (_cursorPos > 0)
                {
                    _text = _text.Remove(_cursorPos - 1, 1);
                    _cursorPos--;
                    TextChanged?.Invoke(this, EventArgs.Empty);
                }
            }
            else
            {
                _text = _text.Insert(_cursorPos, key.ToString());
                _cursorPos++;
                TextChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private bool Contains(int x, int y) =>
            x >= _x && x <= _x + _width && y >= _y && y <= _y + _height;
    }
}
