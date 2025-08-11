# Kerlib

Kerlib is a lightweight and modern Win32 windowing library for C# applications, inspired by the design principles of Avalonia.  
It provides a minimal but powerful abstraction over the Win32 API to create windows, manage message loops, and draw custom graphics with a clean and extensible API.

---

## Features

- **Direct Win32 API integration** with minimal overhead  
- Custom drawable stack for flexible rendering (lines, rectangles, shapes, and more)  
- Simple event system for window events like resize, close, and user interactions  
- Low garbage collection pressure and high performance  
- Designed with extensibility and clarity in mind  

---

## Installation

Clone this repository and include the `Kerlib` project in your solution.  
No external dependencies required, runs on .NET 8.0+.

---

## Usage Example

```csharp
using Kerlib.Drawing;
using Kerlib.Native;
using Kerlib.Window;
namespace Kerlib;

public static class Program
{
    private static void Main()
    {
        using var window = new Win32Window("Win32 Demo", 800, 600);

        window.OnResize += () => Console.WriteLine("Window resized!");
        window.OnClose += () => Console.WriteLine("Window closed!");

        var btn = new Button(100, 100, 200, 50, "Click me!");

        btn.Clicked += _ =>
        {
            Console.WriteLine("Button clicked!");
            btn.Foreground = new Color(255, 255, 255);
            btn.BackgroundNormal = new Color(0, 128, 255);
        };

        btn.MouseEnter += _ =>
        {
            Console.WriteLine("Mouse entered!");
            btn.BackgroundHover = new Color(255, 200, 0);
        };

        btn.MouseLeave += _ =>
        {
            Console.WriteLine("Mouse left!");
            btn.BackgroundHover = new Color(180, 180, 180);
        };

        btn.MouseDown += _ =>
        {
            Console.WriteLine("Pressed down!");
            btn.BackgroundPressed = new Color(255, 0, 0);
        };        
        
        var stack = new RenderStack();
        
        stack.Add(new Line(new Point(50, 50), new Point(200, 200), Color.Red));
        stack.Add(new Rectangle(new Point(100, 100), 300, 200, Color.Green));
        stack.Add(new Text(new Point(50, 50), "Hello Kerlib!", Color.Black, "Consolas", 20));
        stack.Add(btn);

        window.Add(stack);
        window.Show();
        window.RunMessageLoop();
    }
}
