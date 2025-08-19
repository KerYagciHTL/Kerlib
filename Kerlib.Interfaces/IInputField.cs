using System;
namespace Kerlib.Interfaces
{
    public interface IInputField : IRenderable
    {
        string Text { get; set; }
        bool IsFocused { get; }

        event EventHandler? TextChanged;
        event EventHandler? FocusGained;
        event EventHandler? FocusLost;

        bool HandleMouseMove(int x, int y);
        void HandleMouseDown(int x, int y);
        void HandleKeyPress(char key);
    }
}