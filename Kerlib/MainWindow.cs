using Kerlib.Drawing;
using Kerlib.Native;
using static Kerlib.Native.Key;

namespace Kerlib;

public class MainWindow : Core.Window
{
    private readonly Image _image;
    public MainWindow() : base("MainWindow", 800, 600)
    {
        /*var text = new Text(new Point(350, 250), "Hello, Kerlib!", Color.Black);
        Add(text);*/
        
        KeyDown += OnKeyDown;
        
        _image = new Image(new Point(375, 275), "Puppy.png", 50, 50);
        Add(_image);
    }
    
    private void OnKeyDown(Key key)
    {
        if (key == Escape)
        {
            Close();
        }

        if (key == A)
        {
            _image.Position = _image.Position with { X = _image.Position.X - 5 };
        }
        if (key == D)
        {
            _image.Position = _image.Position with { X = _image.Position.X + 5 };
        }

        if (key == W)
        {
            _image.Position = _image.Position with { Y = _image.Position.Y - 5 };
        }
        
        if (key == S)
        {
            //small problem. This is just weird. _image.Position.Y += 5; doesn't work but it would be 10x better
            _image.Position = _image.Position with { Y = _image.Position.Y + 5 };
        }
    }
}