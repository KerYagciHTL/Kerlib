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
        Resized += OnWindowResize;
        Closed += OnWindowClose;

        BackgroundColor = Color.Green;
        
        _button = new Button(new Point(100, 100), 200, 50, "Go to Settings");
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
        Switch(new SettingsWindow());
    }
    private void OnWindowResize()
    {
        Console.WriteLine($"MainWindow resized to {Width}x{Height}");
    }
    private void OnWindowClose()
    {
        Console.WriteLine("MainWindow closed");

        _button.Clicked -= OnButtonClick;

        Resized -= OnWindowResize;
        Closed -= OnWindowClose;
    }
}