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
using Kerlib;
using Kerlib.Drawing;

class Program
{
    static void Main()
    {
        using var window = new Win32Window("Kerlib Demo", 800, 600);

        window.OnResize += () => Console.WriteLine("Window resized!");
        window.OnClose += () => Console.WriteLine("Window closed!");

        window.Add(new Line(50, 50, 200, 200, 255, 0, 0));          // Red line
        window.Add(new RectangleShape(100, 100, 300, 200, 0, 255, 0)); // Green rectangle

        window.Show();
        window.RunMessageLoop();
    }
}
