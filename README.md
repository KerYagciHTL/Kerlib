# Please note that I am still not finished with the Windows and Linux support! This will not work, so far!

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
﻿using Kerlib.Core;

namespace Kerlib;

public static class Program
{
    private static void Main()
    {
        var window = new MainWindow();
        WindowManager.RegisterWindow(window);
        WindowManager.Run();
    }
}
```

```csharp
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
