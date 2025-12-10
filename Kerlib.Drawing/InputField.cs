﻿using System.Text;
using Kerlib.Interfaces;
using Kerlib.Native;

namespace Kerlib.Drawing;

public sealed class InputField : IInputField, IDisposable
{
    private Point _position;
    private int _width, _height;
    private bool _hovered;
    private bool _focused;

    private readonly StringBuilder _textBuilder = new(24);
    private string? _cachedText;
    private int _cursorPos;

    private int _cachedCursorXOffset;
    private bool _cursorXDirty = true;

    private DateTime _lastBlink = DateTime.Now;
    private bool _cursorVisible = true;
    private bool _disposed;

    private Color _backgroundNormal;
    private Color _backgroundHover;
    private Color _backgroundFocused;
    private Color _foreground;

    private uint BgNormal => NativeMethods.Rgb(_backgroundNormal);
    private uint BgHover => NativeMethods.Rgb(_backgroundHover);
    private uint BgFocused => NativeMethods.Rgb(_backgroundFocused);
    private uint Fg => NativeMethods.Rgb(_foreground);

    public event EventHandler? Changed;
    public event EventHandler? TextChanged;
    public event EventHandler? FocusGained;
    public event EventHandler? FocusLost;

    public InputField(Point pos, int width, int height)
    {
        _position = pos ?? throw new ArgumentNullException(nameof(pos));
        _position.Changed += OnPositionChanged;
        _width = width;
        _height = height;

        _backgroundNormal = new Color(255, 255, 255);
        _backgroundHover = new Color(230, 230, 230);
        _backgroundFocused = new Color(200, 200, 255);
        _foreground = new Color(0, 0, 0);
    }

    public Point Position
    {
        get => _position;
        set
        {
            if (ReferenceEquals(_position, value)) return;
            _position.Changed -= OnPositionChanged;
            _position = value ?? throw new ArgumentNullException(nameof(value));
            _position.Changed += OnPositionChanged;
            Changed?.Invoke(this, EventArgs.Empty);
        }
    }

    public int Width
    {
        get => _width;
        set
        {
            if (_width == value) return;
            _width = value;
            Changed?.Invoke(this, EventArgs.Empty);
        }
    }

    public int Height
    {
        get => _height;
        set
        {
            if (_height == value) return;
            _height = value;
            Changed?.Invoke(this, EventArgs.Empty);
        }
    }

    public Color BackgroundNormal
    {
        get => _backgroundNormal;
        set
        {
            if (_backgroundNormal == value) return;
            _backgroundNormal = value;
            Changed?.Invoke(this, EventArgs.Empty);
        }
    }

    public Color BackgroundHover
    {
        get => _backgroundHover;
        set
        {
            if (_backgroundHover == value) return;
            _backgroundHover = value;
            Changed?.Invoke(this, EventArgs.Empty);
        }
    }

    public Color BackgroundFocused
    {
        get => _backgroundFocused;
        set
        {
            if (_backgroundFocused == value) return;
            _backgroundFocused = value;
            Changed?.Invoke(this, EventArgs.Empty);
        }
    }

    public Color Foreground
    {
        get => _foreground;
        set
        {
            if (_foreground == value) return;
            _foreground = value;
            Changed?.Invoke(this, EventArgs.Empty);
        }
    }

    public string Text
    {
        get => _cachedText ??= _textBuilder.ToString();
        set
        {
            if (_cachedText == value)
            {
                _cursorPos = Math.Min(_cursorPos, _cachedText!.Length);
                return;
            }

            _textBuilder.Clear();
            _textBuilder.Append(value);
            _cachedText = value;
            _cursorPos = Math.Min(_cursorPos, _textBuilder.Length);

            _cursorXDirty = true;

            TextChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public bool IsFocused => _focused;
    public void Draw(IntPtr hdc)
    {
        if (_disposed) return;

        var bgColor = _focused ? BgFocused : _hovered ? BgHover : BgNormal;

        // Fill background
        GraphicsContext.FillRectangle(hdc, _position.X, _position.Y, 
            _position.X + _width, _position.Y + _height, bgColor);

        // Draw border
        var penColor = NativeMethods.Rgb(0, 0, 0);
        GraphicsContext.DrawRectangle(hdc, _position.X, _position.Y, 
            _position.X + _width, _position.Y + _height, penColor);

        var drawText = _cachedText ??= _textBuilder.ToString();

        // Draw text
        GraphicsContext.DrawTextInRect(hdc, drawText, _position.X + 4, _position.Y, 
            _position.X + _width - 4, _position.Y + _height, Fg,
            NativeMethods.DtLeft | NativeMethods.DtVcenter | NativeMethods.DtSingleline);

        if (_focused && (DateTime.Now - _lastBlink).TotalMilliseconds > 500)
        {
            _cursorVisible = !_cursorVisible;
            _lastBlink = DateTime.Now;
            Changed?.Invoke(this, EventArgs.Empty);
        }

        if (!_focused || !_cursorVisible) return;

        if (_cursorXDirty)
        {
            _cachedCursorXOffset = GetTextWidthPrefix(hdc, drawText, _cursorPos);
            _cursorXDirty = false;
        }

        var cursorX = _position.X + 4 + _cachedCursorXOffset;
        GraphicsContext.DrawLine(hdc, cursorX, _position.Y + 2, cursorX, _position.Y + _height - 2, penColor);
    }

    public bool HandleMouseMove(int x, int y)
    {
        var inside = Contains(x, y);
        switch (inside)
        {
            case true when !_hovered:
                _hovered = true;
                Changed?.Invoke(this, EventArgs.Empty);
                return true;
            case false when _hovered:
                _hovered = false;
                Changed?.Invoke(this, EventArgs.Empty);
                return true;
            default:
                return false;
        }
    }

    public void HandleMouseDown(int x, int y)
    {
        var inside = Contains(x, y);
        if (inside)
        {
            if (_focused) return;
            _focused = true;
            FocusGained?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            if (!_focused) return;
            _focused = false;
            FocusLost?.Invoke(this, EventArgs.Empty);
        }
    }

    public void HandleKeyPress(char key)
    {
        if (!_focused) return;
        if (key == '\b') { Backspace(); return; }
        if (char.IsControl(key)) return;
        InsertChar(key);
    }
    private void InsertChar(char c)
    {
        _textBuilder.Insert(_cursorPos, c);
        _cursorPos++;
        _cachedText = null;
        _cursorXDirty = true;
        TextChanged?.Invoke(this, EventArgs.Empty);
    }

    private void Backspace()
    {
        if (_cursorPos <= 0) return;
        _textBuilder.Remove(_cursorPos - 1, 1);
        _cursorPos--;
        _cachedText = null;
        _cursorXDirty = true;
        TextChanged?.Invoke(this, EventArgs.Empty);
    }
    private static int GetTextWidthPrefix(IntPtr hdc, string text, int length)
    {
        if (length <= 0) return 0;
        if (length > text.Length) length = text.Length;
        var prefix = text.Substring(0, length);
        var (width, _) = GraphicsContext.MeasureText(hdc, prefix);
        return width;
    }

    private bool Contains(int x, int y) =>
        x >= _position.X && x <= _position.X + _width && y >= _position.Y && y <= _position.Y + _height;

    private void OnPositionChanged(object? sender, EventArgs e)
    {
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed) return;
        if (disposing)
        {
            _position.Changed -= OnPositionChanged;

            Changed = null;
            TextChanged = null;
            FocusGained = null;
            FocusLost = null;
        }

        _disposed = true;
    }
}