using Kerlib.Drawing;
using Kerlib.Native;
using Kerlib.Window;
namespace Kerlib;

public static class Program
{
    private static void Main()
    {
        using var window = new Win32Window("Win32 Demo", 800, 600);

        window.OnResize += () => Console.WriteLine("Window resized!");
        window.OnClose += () => Console.WriteLine("Window closed!");

        var btn = new Button(100, 100, 200, 50, "Click me!");

        btn.Clicked += _ => Console.WriteLine("Button clicked!");
        btn.MouseEnter += _ => Console.WriteLine("Mouse entered button.");
        btn.MouseLeave += _ => Console.WriteLine("Mouse left button.");
        btn.MouseDown += _ => Console.WriteLine("Mouse down on button.");
        btn.MouseUp += _ => Console.WriteLine("Mouse up on button.");           
        
        var stack = new RenderStack();
        
        stack.Add(new Line(new Point(50, 50), new Point(200, 200), Color.Red));
        stack.Add(new Rectangle(new Point(100, 100), 300, 200, Color.Green));
        stack.Add(new Text(new Point(50, 50), "Hello Kerlib!", Color.Black, "Consolas", 20));
        stack.Add(btn);

        window.Add(stack);
        window.Show();
        window.RunMessageLoop();
    }
}