namespace Kerlib.Interfaces
{
    public interface IButton : IRenderable, INotifyRenderableChanged
    {
        string Text { get; set; }
        bool IsHovered { get; }
        bool IsPressed { get; }

        event EventHandler? Clicked;
        event EventHandler? MouseEnter;
        event EventHandler? MouseLeave;
        event EventHandler? MouseDown;
        event EventHandler? MouseUp;

        bool HandleMouseMove(int x, int y);
        void HandleMouseDown(int x, int y);
        void HandleMouseUp(int x, int y);
    }
}