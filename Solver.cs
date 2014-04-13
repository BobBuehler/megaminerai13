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
        Bb.ReadBoard();

        foreach (var attacker in attackers)
        {
            var droid = Bb.DroidLookup[attacker];
            MoveAndAttack(droid, targets);
        }
    }

    public static void MoveAndAttack(Droid attacker, BitArray targets)
    {
        Bb.ReadBoard();

        if (attacker.AttacksLeft <= 0)
        {
            return;
        }

        BitArray validTargets;
        if ((Unit)attacker.Variant == Unit.HACKER)
        {
            validTargets = targets.ToPoints().Where(t => t.IsHackable()).ToBitArray();
        }
        else
        {
            validTargets = targets.ToPoints().Where(t => t.IsAttackable()).ToBitArray();
        }

        Func<Point, bool> patherPassable = p => IsPassable(p) || p.Equals(attacker.ToPoint()) || validTargets.Get(p);

        var path = Pather.AStar(new[] { attacker.ToPoint() }, patherPassable, validTargets.ToFunc());
        if (path == null)
        {
            return;
        }
        if (path.Count() < 2)
        {
            Console.WriteLine("Bad path from attacker " + attacker.Id);
        }

        MoveAndAttack(attacker, path.Skip(1));
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
        }
    }

    public static Point FindFastestSpawn(Func<Point, bool> isSpawnable, IEnumerable<Point> targets, int moveSpeed)
    {
        var search = new Pather.Search(targets, isPassable, p => false);
        var reachable = search.GScore.Keys.Where(s => isSpawnable(s));
        if (!reachable.Any())
        {
            var spawnables = Enumerable.Range(0, 40).SelectMany(x => Enumerable.Range(0, 20).Select(y => new Point(x, y))).Where(p => isSpawnable(p));
            spawnables.MinBy(s => Bb.GetSpawnDelay(s));
        }
        return reachable.MinBy(s => search.GScore[s] + Bb.GetSpawnDelay(s) * moveSpeed);
    }

    public static void MoveFarthestAndAttack(Droid attacker, BitArray targets)
    {
        if (attacker.AttacksLeft == 0)
        {
            return;
        }
        
        var liveTargets = targets.ToPoints().Where(p => Bb.DroidLookup[p].HealthLeft > 0);
        Func<Point, bool> patherPassable = p => IsPassable(p) || p.Equals(attacker.ToPoint());

        var movementSearch = new Pather.Search(new Point[] { attacker.ToPoint() }, patherPassable, p => false, (p1, p2) => 1, attacker.MovementLeft);
        var walkables = movementSearch.GScore.Keys.Where(p => movementSearch.GScore[p] <= attacker.MovementLeft);
        var walkablesWithTargets = walkables.Where(p => liveTargets.Any(t => p.IsInRange(attacker.Range, t)));

        Console.WriteLine("MoveFarthestAndAttack " + attacker.ToPoint());
        Console.WriteLine("Walkables: " + walkables.Count());
        Console.WriteLine("WalkablesWithTargets: " + walkablesWithTargets.Count());

        if (walkablesWithTargets.Any())
        {
            var walkTo = walkablesWithTargets.MaxBy(p => movementSearch.GScore[p]);
            var path = Pather.ConstructPath(movementSearch.CameFrom, walkTo);
            foreach (var step in path.Skip(1))
            {
                Console.WriteLine("Move {0} -> {1}", attacker.ToPoint(), step);
                attacker.move(step.x, step.y);
            }
            var target = liveTargets.First(t => attacker.IsInRange(t));
            attacker.operate(target.x, target.y);
        }
    }
}
