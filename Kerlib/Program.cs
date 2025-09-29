using Kerlib.Core;

namespace Kerlib;

public static class Program
{
    private static void Main()
    {
        try
        {
            var window = new MainWindow();
            WindowManager.RegisterWindow(window);
            WindowManager.Run();
        }
        catch (DllNotFoundException)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Critical error: Unable to load shared libraries!");
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("This library is only supported on Windows versions greater than 6.1.");
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }

    }
}