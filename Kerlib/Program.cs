using Kerlib.Core;

namespace Kerlib;

public static class Program
{
    private static void Main()
    {
        var window = new MainWindow();
        WindowManager.RegisterWindow(window);
    }
}