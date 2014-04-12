using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class Pather
{
    public static IEnumerable<Point> AStar(IEnumerable<Point> starts, Func<Point, bool> passable, Func<Point, bool> isGoal)
    {
        var pt = starts.MinBy(p => p.x);
        return new [] {pt};
    }
}
