using Kerlib.Core;
using Kerlib.Drawing;
using Kerlib.Native;
using Kerlib.Window;

namespace Kerlib;
public class MainWindow : Core.Window
{
    public MainWindow() 
        : base("Main Window", 800, 600) 
    {
        OnResize += () => Console.WriteLine("MainWindow resized");
        OnClose  += () => Console.WriteLine("MainWindow closed");

        var btnSwitch = new Button(100, 100, 200, 50, "Go to Settings");
        btnSwitch.Clicked += (_,_)=>
        {
            Console.WriteLine("Switching to SettingsWindow...");
            WindowManager.SwitchWindow(new SettingsWindow());
        };

        var stack = new RenderStack();
        stack.Add(new Text(new Point(50, 50), "Welcome to Main Window", Color.Black, "Consolas", 20));
        stack.Add(btnSwitch);

        Add(stack);
    }
}