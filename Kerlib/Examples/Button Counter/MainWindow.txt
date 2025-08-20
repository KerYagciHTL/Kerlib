using Kerlib.Drawing;
using Kerlib.Native;
using Kerlib.Window;

namespace Kerlib;

public class MainWindow : Core.Window
{
    private readonly Button _button;
    private readonly Text _label;
    private int _clickCount;
    public MainWindow() : base("Hello World", 800, 600)
    {
        OnClose += OnWindowClose;
        
        var renderStack = new RenderStack();
        
        _button = new Button(new Point(350, 250), 100, 100, "Click Me");
        _label = new Text(new Point(350, 200), "Click Count: 0", Color.Black);
        _button.Clicked += OnButtonClicked;
        
        renderStack.Add(_button);
        renderStack.Add(_label);
        
        Add(renderStack);
    }
    private void OnButtonClicked(object? sender, EventArgs e)
    {
        _clickCount++;
        _label.Content = $"Click Count: {_clickCount}";
    }
    
    private void OnWindowClose()
    {
        _button.Clicked -= OnButtonClicked;
        OnClose -= OnWindowClose;
    }
}