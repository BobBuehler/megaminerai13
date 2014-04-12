using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

public static class Solver
{
    public static void MoveAndAttack(IEnumerable<Point> attackers, BitArray targets)
    {
        attackers.ForEach(a => MoveAndAttack(a, targets));
    }

    public static void MoveAndAttack(Point attacker, BitArray targets)
    {
        Bb.ReadBoard();

        var droid = Bb.DroidLookup[attacker];
        if (droid.AttacksLeft <= 0)
        {
            return;
        }

        var liveTargets = targets.ToPoints().Where(p => Bb.DroidLookup[p].HealthLeft > 0).ToBitArray();

        Func<Point, bool> passable = p => !Bb.DroidLookup.ContainsKey(p);
        Func<Point, bool> patherPassable = p => p.Equals(attacker) || liveTargets.Get(p) || passable(p);

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
                return;
            }
            droid.move(point.x, point.y);
        }

        if (droid.IsInRange(targetPoint))
        {
            droid.operate(targetPoint.x, targetPoint.y);
        }
    }

    public static void MoveAndAttackSmart(IEnumerable<Point> attackers, BitArray targets)
    {
        var droids = attackers.Select(a => Bb.DroidLookup[a]).Where(d => d.AttacksLeft > 0);
        var droidPoints = droids.Select(d => d.ToPoint());
        var attackerBits = droidPoints.ToBitArray();

        var targetBits = targets.ToPoints().Where(p => Bb.DroidLookup[p].HealthLeft > 0).ToBitArray();

        Func<Point, bool> passable = p => !Bb.DroidLookup.ContainsKey(p);
        Func<Point, bool> patherPassable = p => attackerBits.Get(p) || targetBits.Get(p) || passable(p);

        var path = Pather.AStar(droidPoints, patherPassable, targetBits.ToFunc());

    }
}
