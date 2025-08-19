using Kerlib.Core;
using Kerlib.Drawing;
using Kerlib.Native;
using Kerlib.Window;

namespace Kerlib;
public class MainWindow : Core.Window
{
    private readonly Button _button;
    public MainWindow() 
        : base("Main Window", 800, 600)
    {
        OnResize += OnWindowResize;
        OnClose += OnWindowClose;
        
        _button = new Button(100, 100, 200, 50, "Go to Settings");
        _button.Clicked += OnButtonClick;
        
        var stack = new RenderStack();
        stack.Add(new Text(new Point(50, 50), "Welcome to Main Window", Color.Black, "Consolas", 20));
        stack.Add(_button);

        Add(stack);
        
        Console.WriteLine(Title);
        Console.WriteLine(Width.ToString());
        Console.WriteLine(Height.ToString());
    }
    private void OnButtonClick(object? sender, EventArgs e)
    {
        Console.WriteLine("Switching to SettingsWindow...");
        WindowManager.SwitchWindow(new SettingsWindow());
    }
    private void OnWindowResize()
    {
        Console.WriteLine($"MainWindow resized to {Width}x{Height}");
    }
    private void OnWindowClose()
    {
        Console.WriteLine("MainWindow closed");

        _button.Clicked -= OnButtonClick;

        OnResize -= OnWindowResize;
        OnClose -= OnWindowClose;
    }
}