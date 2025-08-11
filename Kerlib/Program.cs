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

        btn.Clicked += b =>
        {
            Console.WriteLine("Button clicked!");
            btn.Foreground = NativeMethods.Rgb(255, 255, 255);
            btn.BackgroundNormal = NativeMethods.Rgb(0, 128, 255);
        };

        btn.MouseEnter += b =>
        {
            Console.WriteLine("Mouse entered!");
            btn.BackgroundHover = NativeMethods.Rgb(255, 200, 0);
        };

        btn.MouseLeave += b =>
        {
            Console.WriteLine("Mouse left!");
            btn.BackgroundHover = NativeMethods.Rgb(180, 180, 180);
        };

        btn.MouseDown += b =>
        {
            Console.WriteLine("Pressed down!");
            btn.BackgroundPressed = NativeMethods.Rgb(255, 0, 0);
        };        
        
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