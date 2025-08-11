using Kerlib.Interfaces;
using Kerlib.Native;

namespace Kerlib.Drawing
{
    public class Button : IButton
    {
        private readonly int _x, _y, _width, _height;
        private bool _hovered;
        private bool _pressed;
        public uint BackgroundNormal { get; set; }
        public uint BackgroundHover { get; set; }
        public uint BackgroundPressed { get; set; }
        public uint Foreground { get; set; }

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

            // Default
            BackgroundNormal = NativeMethods.Rgb(200, 200, 200);
            BackgroundHover = NativeMethods.Rgb(180, 180, 180);
            BackgroundPressed = NativeMethods.Rgb(160, 160, 160);
            Foreground = NativeMethods.Rgb(0, 0, 0);
        }

        public void Draw(IntPtr hdc)
        {
            uint bgColor = _pressed ? BackgroundPressed : _hovered ? BackgroundHover : BackgroundNormal;

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

            NativeMethods.SetTextColor(hdc, Foreground);
            NativeMethods.SetBkMode(hdc, 1); // TRANSPARENT
            NativeMethods.DrawText(hdc, Text, Text.Length, ref rect,
                NativeMethods.DtCenter | NativeMethods.DtVcenter | NativeMethods.DT_SINGLELINE);
        }

        public bool HandleMouseMove(int x, int y)
        {
            var inside = Contains(x, y);
            switch (inside)
            {
                case true when !_hovered:
                    _hovered = true;
                    MouseEnter?.Invoke(this);
                    return true;
                case false when _hovered:
                    _hovered = false;
                    MouseLeave?.Invoke(this);
                    return true;
                default:
                    return false;
            }
        }


        public void HandleMouseDown(int x, int y)
        {
            if (!Contains(x, y)) return;
            _pressed = true;
            MouseDown?.Invoke(this);
        }

        public void HandleMouseUp(int x, int y)
        {
            if (!_pressed) return;
            _pressed = false;
            MouseUp?.Invoke(this);
            if (Contains(x, y))
                Clicked?.Invoke(this);
        }

        private bool Contains(int x, int y) =>
            x >= _x && x <= _x + _width && y >= _y && y <= _y + _height;
    }
}
