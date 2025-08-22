using System.Runtime.InteropServices;

namespace Kerlib.Native;

[StructLayout(LayoutKind.Sequential)]
public readonly struct Color : IEquatable<Color>
{
    public byte R { get; }
    public byte G { get; }
    public byte B { get; }

    public Color(byte r, byte g, byte b)
    {
        R = r;
        G = g;
        B = b;
    }

    public static Color Black => new(0, 0, 0);
    public static Color White => new(255, 255, 255);
    public static Color Red => new(255, 0, 0);
    public static Color Green => new(0, 255, 0);
    public static Color Blue => new(0, 0, 255);
    public static Color Yellow => new(255, 255, 0);

    public static Color FromSystemColor(System.Drawing.Color color) =>
        new(color.R, color.G, color.B);

    public System.Drawing.Color ToSystemColor() =>
        System.Drawing.Color.FromArgb(R, G, B);

    public override bool Equals(object? obj) => 
        obj is Color other && Equals(other);

    public bool Equals(Color other) => 
        R == other.R && G == other.G && B == other.B;

    public override int GetHashCode() => 
        HashCode.Combine(R, G, B);

    public static bool operator ==(Color left, Color right) => 
        left.Equals(right);

    public static bool operator !=(Color left, Color right) => 
        !left.Equals(right);

    public static Color FromRgb(uint color)
    {
        return new Color(
            (byte)(color & 0xFF),         
            (byte)((color >> 8) & 0xFF),  
            (byte)((color >> 16) & 0xFF) 
        );
    }

    public override string ToString() => 
        $"#{(R << 16) | (G << 8) | B:X6}";

    public string ToRgbString() => 
        $"RGB({R}, {G}, {B})";
}