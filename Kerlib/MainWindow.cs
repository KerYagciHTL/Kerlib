using Kerlib.Drawing;
using Kerlib.Native;

namespace Kerlib;

public class MainWindow : Core.Window
{
    public MainWindow() : base("MainWindow", 800, 600)
    {
        var text = new Text(new Point(350, 250), "Hello, Kerlib!", Color.Black);
        Add(text);
    }
}