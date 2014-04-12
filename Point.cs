using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

struct Point
{
    public int x;
    public int y;

    public Point(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public override bool Equals(object obj)
    {
        Point o = (Point)obj;
        return o.x == x && o.y == y;
    }

    public override int GetHashCode()
    {
        return x + y * 40; // Bb.Width;
    }

    public override string ToString()
    {
        return String.Format("({0},{1})", x, y);
    }
}
