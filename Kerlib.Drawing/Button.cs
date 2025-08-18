using Kerlib.Interfaces;
using Kerlib.Native;

namespace Kerlib.Drawing
{
    public class Button : IButton
    {
        private readonly int _x, _y, _width, _height;
        private bool _hovered;
        private bool _pressed;
        private uint BgNormal => NativeMethods.Rgb(BackgroundNormal.R, BackgroundNormal.G, BackgroundNormal.B);
        private uint BgHover => NativeMethods.Rgb(BackgroundHover.R, BackgroundHover.G, BackgroundHover.B);
        private uint BgPressed => NativeMethods.Rgb(BackgroundPressed.R, BackgroundPressed.G, BackgroundPressed.B);
        private uint Fg => NativeMethods.Rgb(Foreground.R, Foreground.G, Foreground.B);
        public Color BackgroundNormal { get; set; }
        public Color BackgroundHover { get; set; }
        public Color BackgroundPressed { get; set; }
        public Color Foreground { get; set; }
        public string Text { get; set; }
        public bool IsHovered => _hovered;
        public bool IsPressed => _pressed;

        public event EventHandler? Clicked;
        public event EventHandler? MouseEnter;
        public event EventHandler? MouseLeave;
        public event EventHandler? MouseDown;
        public event EventHandler? MouseUp;

        public Button(int x, int y, int width, int height, string text)
        {
            _x = x;
            _y = y;
            _width = width;
            _height = height;
            Text = text;

            // Default
            BackgroundNormal = new Color(200, 200, 200);
            BackgroundHover = new Color(180, 180, 180);
            BackgroundPressed = new Color(160, 160, 160);
            Foreground = new Color(0, 0, 0);
        }

        public void Draw(IntPtr hdc)
        {
            var bgColor = _pressed ? BgPressed : _hovered ? BgHover : BgNormal;

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
                left = _x,
                top = _y,
                right = _x + _width,
                bottom = _y + _height
            };

            NativeMethods.SetTextColor(hdc, Fg);
            NativeMethods.SetBkMode(hdc, 1); // TRANSPARENT
            NativeMethods.DrawText(hdc, Text, Text.Length, ref rect,
                NativeMethods.DtCenter | NativeMethods.DtVcenter | NativeMethods.DtSingleline);
        }

        public bool HandleMouseMove(int x, int y)
        {
            var inside = Contains(x, y);
            switch (inside)
            {
                case true when !_hovered:
                    _hovered = true;
                    MouseEnter?.Invoke(this, EventArgs.Empty);
                    return true;
                case false when _hovered:
                    _hovered = false;
                    MouseLeave?.Invoke(this, EventArgs.Empty);
                    return true;
                default:
                    return false;
            }
        }


        public void HandleMouseDown(int x, int y)
        {
            if (!Contains(x, y)) return;
            _pressed = true;
            MouseDown?.Invoke(this, EventArgs.Empty);
        }

        public void HandleMouseUp(int x, int y)
        {
            if (!_pressed) return;
            _pressed = false;
            MouseUp?.Invoke(this, EventArgs.Empty);
            if (Contains(x, y))
                Clicked?.Invoke(this, EventArgs.Empty);
        }

        private bool Contains(int x, int y) =>
            x >= _x && x <= _x + _width && y >= _y && y <= _y + _height;
    }
}
