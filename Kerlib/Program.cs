using Kerlib.Drawing;
using Kerlib.Window;
namespace Kerlib;

public static class Program
{
    private static void Main()
    {
        using var window = new Win32Window("Win32 Demo", 800, 600);

        window.OnResize += () => Console.WriteLine("Window resized!");
        window.OnClose += () => Console.WriteLine("Window closed!");

        var stack = new RenderStack { new Line(50, 50, 200, 200, 255, 0, 0) };
        
        window.Add(stack);
        window.Show();
        window.RunMessageLoop();
    }
}