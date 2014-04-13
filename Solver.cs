using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

public static class Solver
{
    public static bool IsPassable(Point p)
    {
        Droid d;
        if (Bb.DroidLookup.TryGetValue(p, out d))
        {
            return d.HealthLeft == 0;
        }
        return true;
    }

    public static Func<Point, bool> isPassable = p => IsPassable(p);

    public static void MoveAndAttack(IEnumerable<Point> attackers, BitArray targets)
    {
        attackers.ForEach(a => MoveAndAttack(a, targets));
    }

    public static void MoveAndAttack(Point attacker, BitArray targets)
    {
        Bb.ReadBoard();

        if (!Bb.DroidLookup.ContainsKey(attacker))
        {
            Console.WriteLine("Attacker not found: " + attacker);
            return;
        }
        var droid = Bb.DroidLookup[attacker];
        if (droid.AttacksLeft <= 0)
        {
            return;
        }

        var liveTargets = targets.ToPoints().Where(p => Bb.DroidLookup[p].HealthLeft > 0).ToBitArray();

        Func<Point, bool> patherPassable = p => p.Equals(attacker) || liveTargets.Get(p) || IsPassable(p);

        var path = Pather.AStar(new[] { attacker }, patherPassable, liveTargets.ToFunc());
        if (!path.Any())
        {
            return;
        }

        MoveAndAttack(droid, path.Skip(1));
    }

    public static void MoveAndAttack(Droid droid, IEnumerable<Point> path)
    {
        var targetPoint = path.Last();

        foreach (var point in path)
        {
            if (droid.MovementLeft == 0 || droid.IsInRange(targetPoint))
            {
                break;
            }
            droid.move(point.x, point.y);
        }

        if (droid.IsInRange(targetPoint))
        {
            droid.operate(targetPoint.x, targetPoint.y);

            if (droid.MovementLeft > 0)
            {
                Bb.ReadBoard();
                var neighbors = Pather.GetNeighbors(droid.ToPoint(), isPassable);
                foreach (var n in neighbors)
                {
                    droid.move(n.x, n.y);
                    break;
                }
            }
        }
    }

    public static Point FindFastestSpawn(IEnumerable<Point> spawnable, IEnumerable<Point> targets, int moveSpeed)
    {
        var search = new Pather.Search(targets, isPassable, p => false);
        return spawnable.Where(s => search.GScore.ContainsKey(s)).MinBy(s => search.GScore[s] + Bb.GetSpawnDelay(s) * moveSpeed);
    }

    public static Point FindFastestSpawn(Func<Point, bool> isSpawnable, IEnumerable<Point> targets, int moveSpeed)
    {
        var search = new Pather.Search(targets, isPassable, p => false);
        return search.GScore.Keys.Where(s => isSpawnable(s)).MinBy(s => search.GScore[s] + Bb.GetSpawnDelay(s) * moveSpeed);
    }
}
