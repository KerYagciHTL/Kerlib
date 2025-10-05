using Kerlib.Core;
using Kerlib.Native;

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
        finally
        {
            GdiCache.Dispose();
            FontCache.Dispose();
        }
    }
}