using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class Pather
{
    public static IEnumerable<Point> AStar(IEnumerable<Point> starts, Func<Point, bool> isPassable, Func<Point, bool> isGoal, Func<Point, Point, int> getCost, Func<Point, int> getH)
    {
        var closedSet = new HashSet<Point>();
        var openSet = new HashSet<Point>(starts);
        var cameFrom = new Dictionary<Point, Point>();

        var gScore = starts.ToDictionary(s => s, s => 0);
        var fScore = starts.ToDictionary(s => s, s => gScore[s] + getH(s));

        while (openSet.Any())
        {
            var current = openSet.MinBy(p => fScore[p]);
            if (isGoal(current))
            {
                return ConstructPath(cameFrom, current);
            }

            openSet.Remove(current);
            closedSet.Add(current);
            foreach (var n in GetNeighbors(current, isPassable))
            {
                if (closedSet.Contains(n))
                    continue;

                var tentativeG = gScore[current] + getCost(current, n);

                if (!openSet.Contains(n) || tentativeG < gScore[n])
                {
                    cameFrom[n] = current;
                    gScore[n] = tentativeG;
                    fScore[n] = tentativeG + getH(n);
                    openSet.Add(n);
                }
            }
        }

        return new Point[] { };
    }

    public static IEnumerable<Point> ConstructPath(Dictionary<Point, Point> cameFrom, Point end)
    {
        var path = new LinkedList<Point>();
        path.AddFirst(end);

        var current = end;
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.AddFirst(current);
        }

        return path;
    }

    public static IEnumerable<Point> GetNeighbors(Point point, Func<Point, bool> isPassable)
    {
        var neighbors = new Point[] {
            new Point(point.x - 1, point.y),
            new Point(point.x + 1, point.y),
            new Point(point.x, point.y - 1),
            new Point(point.x, point.y + 1)
        };

        return neighbors.Where(n => n.x >= 0 && n.y >= 0 && n.x < 40 && n.y < 20 && isPassable(n));
        // WIDTH and HEIGHT
    }
}
