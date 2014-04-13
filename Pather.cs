using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class Pather
{
    public static IEnumerable<Point> AStar(IEnumerable<Point> starts, Func<Point, bool> isPassable, Func<Point, bool> isGoal)
    {
        return AStar(starts, isPassable, isGoal, (p1, p2) => 1, p => 0);
    }

    public static IEnumerable<Point> AStar(IEnumerable<Point> starts, Func<Point, bool> isPassable, Func<Point, bool> isGoal, Func<Point, Point, int> getCost, Func<Point, int> getH)
    {
        return new Search(starts, isPassable, isGoal, getCost, getH).Path;
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

        return neighbors.Where(n => n.IsOnBoard() && isPassable(n));
    }

    public class Search
    {
        public IEnumerable<Point> Path;
        public HashSet<Point> ClosedSet;
        public HashSet<Point> OpenSet;
        public Dictionary<Point, Point> CameFrom;
        public Dictionary<Point, int> GScore;
        public Dictionary<Point, int> FScore;

        public Search(IEnumerable<Point> starts, Func<Point, bool> isPassable, Func<Point, bool> isGoal)
        {
            DoSearch(starts, isPassable, isGoal, (p1, p2) => 1, p => 0);
        }

        public Search(IEnumerable<Point> starts, Func<Point, bool> isPassable, Func<Point, bool> isGoal, Func<Point, Point, int> getCost, Func<Point, int> getH)
        {
            DoSearch(starts, isPassable, isGoal, getCost, getH);
        }

        private void DoSearch(IEnumerable<Point> starts, Func<Point, bool> isPassable, Func<Point, bool> isGoal, Func<Point, Point, int> getCost, Func<Point, int> getH)
        {
            ClosedSet = new HashSet<Point>();
            OpenSet = new HashSet<Point>(starts);
            CameFrom = new Dictionary<Point, Point>();

            GScore = starts.ToDictionary(s => s, s => 0);
            FScore = starts.ToDictionary(s => s, s => GScore[s] + getH(s));

            while (OpenSet.Any())
            {
                var current = OpenSet.MinBy(p => FScore[p]);
                if (isGoal(current))
                {
                    Path = ConstructPath(CameFrom, current);
                }

                OpenSet.Remove(current);
                ClosedSet.Add(current);
                foreach (var n in GetNeighbors(current, isPassable))
                {
                    if (ClosedSet.Contains(n))
                        continue;

                    var tentativeG = GScore[current] + getCost(current, n);

                    if (!OpenSet.Contains(n) || tentativeG < GScore[n])
                    {
                        CameFrom[n] = current;
                        GScore[n] = tentativeG;
                        FScore[n] = tentativeG + getH(n);
                        OpenSet.Add(n);
                    }
                }
            }

            Path = new Point[] { };
        }
    }
}
