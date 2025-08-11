namespace Kerlib.Interfaces
{
    public interface IButton : IRenderable
    {
        string Text { get; set; }
        bool IsHovered { get; }
        bool IsPressed { get; }

        event Action<IButton>? Clicked;
        event Action<IButton>? MouseEnter;
        event Action<IButton>? MouseLeave;
        event Action<IButton>? MouseDown;
        event Action<IButton>? MouseUp;

        void HandleMouseMove(int x, int y);
        void HandleMouseDown(int x, int y);
        void HandleMouseUp(int x, int y);
    }
}