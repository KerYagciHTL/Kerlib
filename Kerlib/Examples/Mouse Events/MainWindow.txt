using Kerlib.Drawing; 
using Kerlib.Native;

namespace Kerlib;

public class MainWindow : Core.Window
{
    private readonly Text _mouseText = new(new Point(10, 10), "Mouse: -, -", Color.Black);
    private readonly Text _wheelText = new(new Point(10, 30), "Wheel: -", Color.Black);
    private readonly Text _clickText = new(new Point(10, 50), "Click: -", Color.Black);

    public MainWindow() : base("MainWindow", 800, 600)
    {
        var text = new Text(new Point(350, 250), "Hello, Kerlib!", Color.Black); 
        Add(text);

        Add(_mouseText);
        Add(_wheelText);
        Add(_clickText);

        MouseMove += (x, y) => _mouseText.Content = $"Mouse: {x}, {y}";
        MouseWheel += (x, y, delta) => _wheelText.Content = $"Wheel: {delta} @ {x}, {y}";
        MouseDown += (x, y) => _clickText.Content = $"Click: Down @ {x}, {y}";
        MouseUp += (x, y) => _clickText.Content = $"Click: Up @ {x}, {y}";
    }
}