using System.Runtime.InteropServices;

namespace Kerlib.Native;

[StructLayout(LayoutKind.Sequential)]
public class Point : IEquatable<Point>
{
        private int _x;
        private int _y;

        public event EventHandler? Changed;

        public int X
        {
            get => _x;
            set
            {
                if (_x == value) return;
                _x = value;
                Changed?.Invoke(this, EventArgs.Empty);
            }
        }

        public int Y
        {
            get => _y;
            set
            {
                if (_y == value) return;
                _y = value;
                Changed?.Invoke(this, EventArgs.Empty);
            }
        }

        public Point(int x, int y)
        {
            _x = x;
            _y = y;
        }
    public static Point Empty => new(0, 0);

    public static Point FromSystemPoint(System.Drawing.Point point) =>
        new(point.X, point.Y);

    public System.Drawing.Point ToSystemPoint() =>
        new(_x, _y);

    public Point Offset(int dx, int dy) =>
        new(_x + dx, _y + dy);

    public Point Offset(Point point) =>
        new(_x + point.X, _y + point.Y);

    public Point Scale(float factor) =>
        new((int)(_x * factor), (int)(_y * factor));

    public bool Equals(Point? other) =>
        other is not null && _x == other.X && _y == other.Y;

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
        x = _x;
        y = _y;
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