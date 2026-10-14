using System.Diagnostics;
using Kerlib.Drawing;
using Kerlib.Native;

namespace Kerlib;

public class MainWindow : Core.Window
{
    private static readonly Color Background = new(14, 16, 22);
    private static readonly Color Sidebar = new(21, 24, 33);
    private static readonly Color Card = new(26, 30, 41);
    private static readonly Color CardBorder = new(38, 43, 57);
    private static readonly Color Foreground = new(232, 234, 240);
    private static readonly Color Muted = new(138, 144, 162);
    private static readonly Color Accent = new(110, 123, 255);
    private static readonly Color Teal = new(45, 212, 191);
    private static readonly Color Pink = new(244, 114, 182);

    private const string Font = "Segoe UI";
    private const string FontLight = "Segoe UI Light";
    private const string FontSemibold = "Segoe UI Semibold";
    private const string FontMono = "Consolas";

    private readonly Stopwatch _uptime = Stopwatch.StartNew();
    private readonly FrameCounter _frameCounter = new();
    private readonly Wave _wave;

    private readonly Text _clock;
    private readonly Text _uptimeValue;
    private readonly Text _framesValue;
    private readonly Text _mouseValue;
    private readonly Text _frequencyText;
    private readonly Text _greeting;
    private readonly Text _lastKey;
    private readonly Text _lastClick;
    private readonly Text _lastWheel;
    private readonly InputField _nameInput;

    public MainWindow() : base("Kerlib Showcase", 1000, 628)
    {
        BackgroundColor = Background;

        // Sidebar
        Add(new Panel(0, 0, 220, 2000, Sidebar));
        Add(new Panel(28, 33, 14, 14, Accent, radius: 14));
        Add(new Text(new Point(52, 22), "Kerlib", Foreground, FontSemibold, 26));
        Add(new Text(new Point(28, 62), "v1.0.0  ·  Win32 / GDI", Muted, Font, 13));

        Add(new Text(new Point(28, 112), "FEATURES", Muted, FontSemibold, 11));
        string[] features = ["Double-buffered GDI", "Event-driven input", "Custom IRenderables", "Zero dependencies"];
        for (var i = 0; i < features.Length; i++)
        {
            var y = 140 + i * 30;
            Add(new Panel(28, y + 6, 8, 8, Teal, radius: 8));
            Add(new Text(new Point(46, y), features[i], Foreground, Font, 15));
        }

        Add(new Text(new Point(28, 584), "github.com/KerYagciHTL/Kerlib", Muted, Font, 12));

        // Header
        Add(new Text(new Point(250, 22), "Hello, Kerlib.", Foreground, FontLight, 40));
        Add(new Text(new Point(254, 80), "A lightweight Win32 windowing library for C#. Every pixel here is plain GDI.", Muted, Font, 15));
        _clock = new Text(new Point(880, 36), Now(), Foreground, FontMono, 20);
        Add(_clock);

        // Stat cards
        _uptimeValue = AddStatCard(252, "UPTIME", Accent);
        _framesValue = AddStatCard(496, "FRAMES RENDERED", Teal);
        _mouseValue = AddStatCard(740, "MOUSE", Pink);

        // Live rendering card
        AddCard(252, 228, 716, 230);
        Add(new Text(new Point(272, 244), "Live rendering", Foreground, FontSemibold, 16));
        Add(new Text(new Point(272, 268), "Move the mouse to bend the waves  ·  scroll to change the frequency", Muted, Font, 13));
        _frequencyText = new Text(new Point(852, 248), "-", Muted, FontMono, 13);
        _wave = new Wave(272, 292, 676, 150, [Accent, Teal, Pink], CardBorder);
        UpdateFrequencyText();
        Add(_frequencyText);
        Add(_wave);

        // Interactive card
        AddCard(252, 474, 350, 130);
        Add(new Text(new Point(272, 490), "Say hi", Foreground, FontSemibold, 16));
        Add(new DcStyle(Font, 15, Foreground));
        _nameInput = new InputField(new Point(272, 522), 200, 34)
        {
            BackgroundNormal = new Color(34, 39, 52),
            BackgroundHover = new Color(40, 46, 61),
            BackgroundFocused = new Color(46, 52, 70),
            Foreground = Foreground
        };
        var greetButton = new Button(new Point(484, 522), 98, 34, "Greet")
        {
            BackgroundNormal = Accent,
            BackgroundHover = new Color(133, 144, 255),
            BackgroundPressed = new Color(88, 101, 232),
            Foreground = Color.White
        };
        greetButton.Clicked += (_, _) => Greet();
        _greeting = new Text(new Point(272, 570), "Type your name and hit Greet (or Enter).", Muted, Font, 13);
        Add(_nameInput);
        Add(greetButton);
        Add(_greeting);

        // Event log card
        AddCard(618, 474, 350, 130);
        Add(new Text(new Point(638, 490), "Event log", Foreground, FontSemibold, 16));
        _lastKey = AddLogLine(522, "Key");
        _lastClick = AddLogLine(546, "Click");
        _lastWheel = AddLogLine(570, "Wheel");

        Add(_frameCounter);

        Tick += OnTick;
        MouseMove += OnMouseMove;
        MouseDown += (x, y) => _lastClick.Content = $"{x}, {y}";
        MouseWheel += OnMouseWheel;
        KeyDown += OnKeyDown;
    }

    private void OnTick()
    {
        _clock.Content = Now();
        _uptimeValue.Content = _uptime.Elapsed.ToString(@"hh\:mm\:ss");
        _framesValue.Content = _frameCounter.Frames.ToString("N0");
    }

    private void OnMouseMove(int x, int y)
    {
        _mouseValue.Content = $"{x}, {y}";
        if (_wave.Contains(x, y)) _wave.Focus(x, y);
        else _wave.Unfocus();
    }

    private void OnMouseWheel(int x, int y, int delta)
    {
        _lastWheel.Content = $"{delta:+0;-0} @ {x}, {y}";
        _wave.Frequency = Math.Clamp(_wave.Frequency + delta / 120.0 * 0.25, 0.5, 6.0);
        UpdateFrequencyText();
    }

    private void OnKeyDown(Key key)
    {
        _lastKey.Content = key.Name;
        if (key == Key.Enter && _nameInput.IsFocused) Greet();
    }

    private void Greet()
    {
        var name = _nameInput.Text.Trim();
        _greeting.Content = name.Length == 0 ? "Type a name first :)" : $"Nice to meet you, {name}!";
        _greeting.Color = name.Length == 0 ? Pink : Teal;
    }

    private void UpdateFrequencyText() => _frequencyText.Content = $"freq {_wave.Frequency:0.00}x";

    private void AddCard(int x, int y, int width, int height) =>
        Add(new Panel(x, y, width, height, Card, CardBorder, 14));

    private Text AddStatCard(int x, string label, Color accent)
    {
        AddCard(x, 120, 228, 92);
        Add(new Panel(x + 20, 136, 28, 4, accent, radius: 4));
        Add(new Text(new Point(x + 20, 148), label, Muted, FontSemibold, 11));
        var value = new Text(new Point(x + 20, 166), "-", Foreground, FontSemibold, 26);
        Add(value);
        return value;
    }

    private Text AddLogLine(int y, string label)
    {
        Add(new Text(new Point(638, y), label, Muted, FontMono, 14));
        var value = new Text(new Point(700, y), "-", Foreground, FontMono, 14);
        Add(value);
        return value;
    }

    private static string Now() => DateTime.Now.ToString("HH:mm:ss");
}
