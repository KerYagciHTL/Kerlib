using Kerlib.Core;
using Kerlib.Drawing;
using Kerlib.Native;
using Kerlib.Window;

namespace Kerlib;

public class SettingsWindow : Core.Window
{
    public SettingsWindow() 
        : base("Settings Window", 600, 400)
    {
        OnResize += () => Console.WriteLine("SettingsWindow resized");
        OnClose  += () => Console.WriteLine("SettingsWindow closed");

        var btnBack = new Button(100, 100, 200, 50, "Back to Main");
        btnBack.Clicked += (_,_) =>
        {
            Console.WriteLine("Switching to MainWindow...");
            WindowManager.SwitchWindow(new MainWindow());
        };

        var stack = new RenderStack();
        stack.Add(new Text(new Point(50, 50), "Settings Page", Color.Black, "Consolas", 20));
        stack.Add(btnBack);

        Add(stack);
    }
}