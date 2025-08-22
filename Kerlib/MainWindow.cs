using Kerlib.Drawing;
using Kerlib.Native;
using static Kerlib.Native.Key;

namespace Kerlib;

public class MainWindow : Core.Window
{
    private readonly Image _image;
    private readonly Image _coin;
    public MainWindow() : base("MainWindow", 800, 600)
    {
        /*var text = new Text(new Point(350, 250), "Hello, Kerlib!", Color.Black);
        Add(text);*/

         KeysDown += OnKeysDown;
         Tick += OnTick;

        _image = new Image(new Point(375, 275), "Assets/puppy.png", 50, 50);
        _coin = new Image(new Point(100, 100), "Assets/coin.png", 32, 32);
        
        Add(_image);
    }

    private void OnKeysDown(IReadOnlyCollection<Key> keys)
    {
        if (keys.Contains(A))
            _image.Position.X -= 5;
        if (keys.Contains(D))
            _image.Position.X += 5;
        if (keys.Contains(W))
            _image.Position.Y -= 5;
        if (keys.Contains(S))
            _image.Position.Y += 5;
    }
    
    private void OnTick()
    {
        Console.WriteLine(_image.Position.ToString());
    }
}