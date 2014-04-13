using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

public static class Extensions
{
    public static T MinBy<T>(this IEnumerable<T> source, Func<T, T, int> compare)
    {
        var min = source.First();

        foreach (var s in source)
        {
            if (compare(s, min) < 0)
            {
                min = s;
            }
        }

        return min;
    }
    public static T MinBy<T, K>(this IEnumerable<T> source, Func<T, K> selector)
    {
        var comparer = Comparer<K>.Default;
        return source.MinBy((s1, s2) => comparer.Compare(selector(s1), selector(s2)));
    }

    public static T MaxBy<T, K>(this IEnumerable<T> source, Func<T, K> selector)
    {
        var comparer = Comparer<K>.Default;

        var max = source.First();
        var maxV = selector(max);

        foreach (var s in source)
        {
            var v = selector(s);
            if (comparer.Compare(v, maxV) > 0)
            {
                maxV = v;
                max = s;
            }
        }

        return max;
    }

    public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
        foreach (var s in source)
        {
            action(s);
        }
    }

    public static bool Get(this BitArray bits, int x, int y)
    {
        return bits.Get(Bb.GetOffset(x, y));
    }

    public static bool Get(this BitArray bits, Point p)
    {
        return bits.Get(p.x, p.y);
    }

    public static void Set(this BitArray bits, int x, int y, bool value)
    {
        bits.Set(Bb.GetOffset(x, y), value);
    }

    public static void Set(this BitArray bits, Point p, bool value)
    {
        bits.Set(p.x, p.y, value);
    }

    public static Point ToPoint(this Mappable mappable)
    {
        return new Point(mappable.X, mappable.Y);
    }

    public static Func<Point, bool> ToFunc(this BitArray bits)
    {
        return p => bits.Get(p);
    }

    public static IEnumerable<Point> ToPoints(this BitArray bits)
    {
        for (int x = 0; x < Bb.Width; ++x)
        {
            for (int y = 0; y < Bb.Height; ++y)
            {
                if (bits.Get(x, y))
                {
                    yield return new Point(x, y);
                }
            }
        }
    }

    public static BitArray ToBitArray(this IEnumerable<Point> points)
    {
        var bits = new BitArray(Bb.Width * Bb.Height);
        points.ForEach(p => bits.Set(p, true));
        return bits;
    }

    public static int ManhattanDistance(this Point source, Point target)
    {
        return Math.Abs(source.x - target.x) + Math.Abs(source.y - target.y);
    }

    public static bool IsInRange(this Point source, int range, Point target)
    {
        return range >= source.ManhattanDistance(target);
    }

    public static bool IsInRange(this Droid droid, Point target)
    {
        return droid.ToPoint().IsInRange(droid.Range, target);
    }

    public static IEnumerable<Point> GetPointsInRange(this Point center, int range)
    {
        for (int y = center.y - range; y < center.y + range; ++y)
        {
            for (int x = center.x - range; x < center.x + range; ++x)
            {
                var p = new Point(x, y);
                if (p.IsOnBoard() && center.IsInRange(range, p))
                {
                    yield return p;
                }
            }
        }
    }

    public static bool IsOnBoard(this Point point)
    {
        return point.x >= 0 && point.x < Bb.Width && point.y >= 0 && point.y < Bb.Height;
    }

    public static bool IsHackable(this Point p)
    {
        Droid d;
        if (Bb.DroidLookup.TryGetValue(p, out d))
        {
            return IsHackable(d);
        }
        return false;
    }

    public static bool IsHackable(this Droid d)
    {
        return d.Owner != Bb.id && d.Hackets < d.HacketsMax && d.HackedTurnsLeft == 0;
    }

    public static bool IsAttackable(this Point p)
    {
        Droid d;
        if (Bb.DroidLookup.TryGetValue(p, out d))
        {
            return IsAttackable(d);
        }
        return false;
    }

    public static bool IsAttackable(this Droid d)
    {
        return d.HealthLeft > 0 && !((d.Owner == Bb.id) == (d.HackedTurnsLeft == 0));
    }

    public static bool IsRepairable(this Droid d)
    {
        return d.Armor < d.MaxArmor && d.Owner == Bb.id && d.HackedTurnsLeft == 0;
    }

    public static bool IsRepairable(this Point p)
    {
        Droid d;
        if (Bb.DroidLookup.TryGetValue(p, out d))
        {
            return IsRepairable(d);
        }
        return false;
    }

    public static bool IsOperatable(this Droid d, Droid target)
    {
        if ((Unit)d.Variant == Unit.HACKER)
        {
            return IsHackable(target);
        }
        if ((Unit)d.Variant == Unit.REPAIRER)
        {
            return IsRepairable(target);
        }
        return IsAttackable(target);
    }

    public static bool IsSpawnable(this Point p)
    {
        Tile t = Bb.TileLookup[p];
        return t.TurnsUntilAssembled > 0 && !Bb.OurHangars.Get(p) && !Bb.OurTurrets.Get(p) && !Bb.TheirHangars.Get(p);
    }

    public static int Count(this BitArray bits)
    {
        int count = 0;
        for (int i = 0; i < bits.Count; ++i)
        {
            if (bits[i])
            {
                ++count;
            }
        }
        return count;
    }
}
