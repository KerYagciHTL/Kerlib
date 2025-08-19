using Kerlib.Drawing;
using Kerlib.Native;

namespace Kerlib;

public class MainWindow : Core.Window
{
    public MainWindow() : base("MainWindow", 800, 600)
    {
        var input = new InputField(new Point(50, 50), 200, 30);
        
        input.TextChanged += (_, _) =>
        {
            Console.WriteLine("Text geändert: " + input.Text);
        };

        
        Add(input);
    }
}