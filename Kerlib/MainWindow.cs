using Kerlib.Drawing;
using Kerlib.Native;

namespace Kerlib;

public class MainWindow : Core.Window
{
    public MainWindow() : base("MainWindow", 800, 600)
    {
        /*var text = new Text(new Point(350, 250), "Hello, Kerlib!", Color.Black);
        Add(text);*/
        
        KeyDown += OnKeyDown;
        
        var image = new Image(new Point(375, 275), "Puppy.png", 50, 50);
        Add(image);
    }
    
    private void OnKeyDown(Key key)
    {
        Console.WriteLine(key);
    }
}