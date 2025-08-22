using System.Runtime.InteropServices;

namespace Kerlib.Native;

[StructLayout(LayoutKind.Sequential)]
public class Point : IEquatable<Point>
{
    public int X { get; set; }
    public int Y { get; set; }

    public Point(int x, int y)
    {
        X = x;
        Y = y;
    }

    public static Point Empty => new(0, 0);

    public static Point FromSystemPoint(System.Drawing.Point point) =>
        new(point.X, point.Y);

    public System.Drawing.Point ToSystemPoint() =>
        new(X, Y);

    public Point Offset(int dx, int dy) =>
        new(X + dx, Y + dy);

    public Point Offset(Point point) =>
        new(X + point.X, Y + point.Y);

    public Point Scale(float factor) =>
        new((int)(X * factor), (int)(Y * factor));

    public bool Equals(Point? other) =>
        other is not null && X == other.X && Y == other.Y;

    public override bool Equals(object? obj) =>
        obj is Point other && Equals(other);

    public override int GetHashCode() =>
        HashCode.Combine(X, Y);

    public static bool operator ==(Point? left, Point? right) =>
        Equals(left, right);

    public static bool operator !=(Point? left, Point? right) =>
        !Equals(left, right);

    public static Point operator +(Point left, Point right) =>
        new(left.X + right.X, left.Y + right.Y);

    public static Point operator -(Point left, Point right) =>
        new(left.X - right.X, left.Y - right.Y);

    public override string ToString() =>
        $"({X}, {Y})";

    public void Deconstruct(out int x, out int y)
    {
        x = X;
        y = Y;
    }

    public double DistanceTo(Point other)
    {
        var dx = other.X - X;
        var dy = other.Y - Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }

    public double Magnitude =>
        Math.Sqrt(X * X + Y * Y);
}