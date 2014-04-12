﻿using System;
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

        Func<Point, bool> passable = p => !Bb.DroidLookup.ContainsKey(p);
        Func<Point, bool> patherPassable = p => p.Equals(attacker) || targets.Get(p) || passable(p);

        var path = Pather.AStar(new[] { attacker }, patherPassable, targets.ToFunc(), (p1, p2) => 1, p => 0);

        var targetPoint = path.Last();
        MoveUntilInRange(droid, targetPoint, path);

        if (droid.IsInRange(targetPoint))
        {
            droid.operate(targetPoint.x, targetPoint.y);
        }
    }

    public static void MoveUntilInRange(Droid droid, Point targetPoint, IEnumerable<Point> path)
    {
        foreach (var point in path.Skip(1))
        {
            if (droid.MovementLeft == 0 || droid.IsInRange(targetPoint))
            {
                return;
            }
            droid.move(point.x, point.y);
        }
    }
}
