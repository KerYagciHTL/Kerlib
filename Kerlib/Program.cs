using Kerlib.Window;
namespace Kerlib;

public static class Program
{
    private static void Main()
    {
        using var window = new Win32Window("Win32 Demo", 800, 600);

        window.OnResize += () => Console.WriteLine("Window resized!");
        window.OnClose += () => Console.WriteLine("Window closed!");

        window.Show();
        window.RunMessageLoop();
    }
}