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
        else if ((Unit)attacker.Variant == Unit.REPAIRER)
        {
            validTargets = targets.ToPoints().Where(t => t.IsRepairable()).ToBitArray();
        }
        else
        {
            validTargets = targets.ToPoints().Where(t => t.IsAttackable()).ToBitArray();
            if (Bb.KillHangerCountDown > 0)
            {
                validTargets = validTargets.ToPoints().Where(t =>
                    {
                        var droid = Bb.DroidLookup[t];
                        return !((Unit)droid.Variant == Unit.HANGAR && droid.HealthLeft <= attacker.Attack);
                    }).ToBitArray();
            }
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
            var target = Bb.DroidLookup[targetPoint];
            if ((Unit)target.Variant == Unit.HANGAR && target.HealthLeft <= 0)
            {
                Bb.KillHangerCountDown = 0; // Todo, release the kittens
            }
        }
    }

    public static void MoveCloseTo(Point mover, Point target)
    {
        var droid = Bb.DroidLookup[mover];
        var search = new Pather.Search(
            new Point[] { mover },
            isPassable,
            p => false,
            (p1, p2) => 1,
            droid.MovementLeft);

        var walkable = search.GScore.Where(kvp => kvp.Value < droid.MovementLeft).Select(kvp => kvp.Key);
        if (!walkable.Any())
        {
            return;
        }

        var destination = walkable.MinBy(p => p.ManhattanDistance(mover));
        var steps = Pather.ConstructPath(search.CameFrom, destination).Skip(1);

        foreach (var step in steps)
        {
            TryOperate(droid);
            droid.move(step.x, step.y);
        }
        TryOperate(droid);
    }

    public static void TryOperate(Droid droid)
    {
        if (droid.AttacksLeft > 0)
        {
            foreach (var p in droid.ToPoint().GetPointsInRange(droid.Range))
            {
                Droid target;
                if (Bb.DroidLookup.TryGetValue(p, out target))
                {
                    if (droid.IsOperatable(target))
                    {
                        while(droid.AttacksLeft > 0)
                        {
                            droid.operate(target.X, target.Y);
                        }
                    }
                }
            }
        }
    }

    public static Point FindFastestSpawn(Func<Point, bool> isSpawnable, IEnumerable<Point> targets, int moveSpeed)
    {
        var search = new Pather.Search(targets, isPassable, p => false);
        var reachable = search.GScore.Keys.Where(s => isSpawnable(s));
        if (!reachable.Any())
        {
            return new Point(-1, -1);
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

    public static void BeSmarter(IEnumerable<Point> attackers, IEnumerable<Point> targets, Func<Droid, IEnumerable<DroidTurn>, DroidTurn> choose)
    {
        Bb.ReadBoard();

        var validAttackers = attackers.Where(a => Bb.DroidLookup.ContainsKey(a) && Bb.DroidLookup[a].AttacksLeft > 0);
        if (!validAttackers.Any())
        {
            return;
        }

        var validTargets = targets.Where(t => t.IsAttackable());
        if (!validTargets.Any())
        {
            return;
        }

        var attackerBits = validAttackers.ToBitArray();
        var targetBits = validTargets.ToBitArray();

        var targetRangeSearch = new Pather.Search(
            targets,
            p => isPassable(p) || attackerBits.Get(p) || targetBits.Get(p),
            p => false);

        var reachingAttackers = targetRangeSearch.GScore.Keys.Where(p => attackerBits.Get(p));
        if (!reachingAttackers.Any())
        {
            return;
        }

        var attacker = reachingAttackers.MinBy((a1, a2) => CompareAttackers(a1, a2, targetRangeSearch.GScore));
        var droid = Bb.DroidLookup[attacker];

        var walkSearch = new Pather.Search(
            new[] { attacker },
            p => isPassable(p),
            p => false,
            (p1, p2) => 1,
            droid.MovementLeft);

        List<DroidTurn> turnChoices = new List<DroidTurn>();

        var walkable = walkSearch.GScore.Where(kvp => kvp.Value <= droid.MovementLeft).Select(kvp => kvp.Key);
        foreach (var destination in walkable)
        {
            var path = Pather.ConstructPath(walkSearch.CameFrom, destination).Skip(1);
            var targetsInRange = destination.GetPointsInRange(droid.Range).Where(p => targetBits.Get(p));
            var movementLeft = droid.MovementLeft - (path.Count());
            if (movementLeft > 0)
            {
                var nextWalkSearch = new Pather.Search(
                    new[] { destination },
                    p => isPassable(p) || p.Equals(attacker),
                    p => false,
                    (p1, p2) => 1,
                    movementLeft);
                var nextWalkable = nextWalkSearch.GScore.Where(kvp => kvp.Value <= movementLeft).Select(kvp => kvp.Key);
                foreach (var nextDestination in nextWalkable)
                {
                    var fullPath = path.Concat(Pather.ConstructPath(walkSearch.CameFrom, nextDestination).Skip(1));
                    var turn = new DroidTurn();
                    turn.Steps = fullPath;
                    turn.Targets = targetsInRange;
                    turnChoices.Add(turn);
                }
            }
            else
            {
                var turn = new DroidTurn();
                turn.Steps = path;
                turn.Targets = targetsInRange;
                turnChoices.Add(turn);
            }
        }

        Console.WriteLine("{0} {1}", (Unit)droid.Variant, droid.ToPoint());
        foreach (var turn in turnChoices)
        {
            foreach (var step in turn.Steps)
            {
                Console.Write(step + "->");
            }
            Console.WriteLine(" X" + turn.Targets.Count());
        }

        var choice = choose(droid, turnChoices);
        choice.Apply(droid);
        
        attackerBits.Set(droid.ToPoint(), false);

        BeSmarter(attackerBits.ToPoints(), targets, choose);
    }

    public class DroidTurn
    {
        public IEnumerable<Point> Steps;
        public IEnumerable<Point> Targets;

        public void Apply(Droid attacker)
        {
            foreach (var step in Steps)
            {
                TryAttack(attacker);
                attacker.move(step.x, step.y);
            }
            TryAttack(attacker);
        }

        public void TryAttack(Droid attacker)
        {
            if (attacker.AttacksLeft > 0)
            {
                var targetsInRange = attacker.ToPoint().GetPointsInRange(attacker.Range).Where(p => Targets.Contains(p));
                if (targetsInRange.Any())
                {
                    var target = targetsInRange.First();
                    while (attacker.AttacksLeft > 0)
                    {
                        attacker.operate(target.x, target.y);
                    }
                }
            }
        }
    }

    public static int CompareAttackers(Point a1, Point a2, Dictionary<Point, int> gScore)
    {
        var d1 = Bb.DroidLookup[a1];
        var d2 = Bb.DroidLookup[a2];
        var g1 = gScore[a1];
        var g2 = gScore[a2];

        var closeness1 = g1 - (d1.Range + d1.MovementLeft);
        var closeness2 = g2 - (d2.Range + d2.MovementLeft);

        return closeness1 <= closeness2 ? -1 : 1;
    }
}
