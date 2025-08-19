using Kerlib.Drawing;
using Kerlib.Native;

namespace Kerlib;

public class MainWindow : Core.Window
{
    public MainWindow() : base("MainWindow", 800, 600)
    {
        var input = new InputField(new Point(50, 50), 200, 30);

        input.BackgroundNormal = new Color(255, 255, 255);
        input.BackgroundHover = new Color(230, 230, 230);
        input.BackgroundFocused = new Color(200, 200, 255);
        input.Foreground = new Color(0, 0, 0);

        input.TextChanged += (s, e) =>
        {
            Console.WriteLine("Text geändert: " + input.Text);
        };

        
        Add(input);
    }
}