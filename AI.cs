using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;

enum Unit
{
    CLAW = 0,
    ARCHER = 1,
    REPAIRER = 2,
    HACKER = 3,
    TURRET = 4,
    WALL = 5,
    TERMINATOR = 6,
    HANGAR = 7,
};

/// <summary>
/// The class implementing gameplay logic.
/// </summary>
class AI : BaseAI
{
    public override string username()
    {
        return "Needs Review";
    }

    public override string password()
    {
        return "password";
    }


    private bool IsSpawnable(Point p)
    {
        return !(Bb.OurUnits.Get(p) || Bb.TheirHangars.Get(p) || Bb.OurSpawning.Get(p) || Bb.TheirSpawning.Get(p));
    }

    private bool CanAfford(Unit u)
    {
        return modelVariants[(int)u].Cost <= players[playerID()].ScrapAmount;
    }

    private bool SpawnUnit(Unit u)
    {
        //Find best spawning spot, p
        var spawnspot = Solver.FindFastestSpawn(p => IsSpawnable(p), Bb.TheirUnits.ToPoints(), modelVariants[(int) u].MaxMovement);
        if (!spawnspot.IsOnBoard())
        {
            return false;
        }

        var spawned = false;
        if(IsSpawnable(spawnspot) && CanAfford(u) )
        {
            spawned = players[playerID()].orbitalDrop(spawnspot.x, spawnspot.y, (int)u);
        }
        return spawned;
    }

    private void PreventTurrets()
    {
        //var spawningTurrets = Bb.TheirSpawning.ToPoints().Where(p => (Unit)Bb.TileLookup[p].VariantToAssemble == Unit.TURRET);
        //foreach (var s in spawningTurrets)
        //{
        //    var tile = Bb.TileLookup.G
        //}
    }

    private void TakeOutTurrets()
    {
        foreach (Point p in Bb.TheirTurrets.ToPoints())
        {
            if (CanAfford(Unit.CLAW) && IsSpawnable(p))
            {
                players[playerID()].orbitalDrop(p.x, p.y, (int)Unit.CLAW);
            }
        }
        Bb.ReadBoard();
    }

    public void Spawn()
    {
        PreventTurrets();
        TakeOutTurrets();

        var units = new HashSet<Unit> { Unit.CLAW, Unit.ARCHER, Unit.HACKER, Unit.REPAIRER, Unit.TERMINATOR, Unit.TURRET };
        var ourUnitCounts = units.ToDictionary(u => u, u => 0);
        var theirUnitCounts = units.ToDictionary(u => u, u => 0);
        droids.Where(d => units.Contains((Unit)d.Variant)).ForEach(d => { if (d.Owner == Bb.id) ourUnitCounts[(Unit)d.Variant]++; else theirUnitCounts[(Unit)d.Variant]++; });
        var theirTotal = theirUnitCounts.Sum(kvp => kvp.Value);
        var isSpawningTurretsSoon = Bb.TheirSpawning.ToPoints().Any(p => (Unit)Bb.TileLookup[p].VariantToAssemble == Unit.TURRET && Bb.TileLookup[p].TurnsUntilAssembled < 10);

        float targetClawRatio = theirTotal == 0 ? 1.0f : 0.0f;
        float targetHackerRatio = theirUnitCounts[Unit.HACKER] + theirUnitCounts[Unit.CLAW];
        float targetArcherRatio = theirUnitCounts[Unit.ARCHER] + theirUnitCounts[Unit.TURRET] + (isSpawningTurretsSoon ? 5 : 0);
        float targetTerminatorRatio = theirUnitCounts[Unit.ARCHER] + theirUnitCounts[Unit.TERMINATOR] + theirUnitCounts[Unit.CLAW] + theirUnitCounts[Unit.REPAIRER];
        float targetRepairerRatio = theirUnitCounts[Unit.ARCHER] / 4 + theirUnitCounts[Unit.CLAW] / 4 + ourUnitCounts[Unit.TERMINATOR];

        var total = targetClawRatio + targetHackerRatio + targetArcherRatio + targetTerminatorRatio + targetRepairerRatio;
        targetHackerRatio /= total;
        targetArcherRatio /= total;
        targetTerminatorRatio /= total;
        targetRepairerRatio /= total;

        float unitCount = Bb.OurUnits.Count() - Bb.OurHangars.Count() - Bb.OurTurrets.Count() - Bb.OurWalls.Count() + .0001f;
        int clawCount = Bb.OurClaws.Count();
        int archerCount = Bb.OurArchers.Count();
        int hackerCount = Bb.OurHackers.Count();
        int terminatorCount = Bb.OurTerminators.Count();
        int repairerCount = Bb.OurRepairers.Count();

        if (CanAfford(Unit.CLAW) && clawCount / unitCount < targetClawRatio)
        {
            SpawnUnit(Unit.CLAW);
            Bb.ReadBoard();
        }
        if (CanAfford(Unit.TERMINATOR) && terminatorCount / unitCount < targetTerminatorRatio)
        {
            SpawnUnit(Unit.TERMINATOR);
            Bb.ReadBoard();
        }
        if (CanAfford(Unit.HACKER) && hackerCount / unitCount < targetHackerRatio)
        {
            SpawnUnit(Unit.HACKER);
            Bb.ReadBoard();
        }
        if (CanAfford(Unit.ARCHER) && archerCount / unitCount < targetArcherRatio)
        {
            SpawnUnit(Unit.ARCHER);
            Bb.ReadBoard();
        }
        if (CanAfford(Unit.REPAIRER) && repairerCount / unitCount < targetRepairerRatio)
        {
            SpawnUnit(Unit.REPAIRER);
            Bb.ReadBoard();
        }
    }

    /// <summary>
    /// This function is called each time it is your turn.
    /// </summary>
    /// <returns>True to end your turn. False to ask the server for updated information.</returns>
    public override bool run()
    {
        Bb.ReadBoard();
        Console.WriteLine("Turn: {0}  {1} v {2}  {3} v {4}", turnNumber(), Bb.OurHangars.Count(), Bb.TheirHangars.Count(),
            Bb.OurHangars.ToPoints().Sum(p => Bb.DroidLookup[p].HealthLeft),
            Bb.TheirHangars.ToPoints().Sum(p => Bb.DroidLookup[p].HealthLeft));

        Spawn();

        int mid = Bb.Width / 2;
        var ourValue = Bb.OurUnits.ToPoints().Where(u => Bb.id == 0 ? u.x > mid : u.x < mid).Sum(p => modelVariants[Bb.DroidLookup[p].Variant].Cost);
        var theirValue = Bb.TheirUnits.ToPoints().Sum(p => modelVariants[Bb.DroidLookup[p].Variant].Cost) + players[Bb.id == 0 ? 1 : 0].ScrapAmount;
        if (ourValue < theirValue * 2)
        {
            Bb.KillHangerCountDown = 1;
        }
        else
        {
            Bb.KillHangerCountDown = 0;
        }
        
        Solver.MoveAndAttack(Bb.OurHackers.ToPoints(), Bb.TheirHackers);
        Solver.MoveAndAttack(Bb.OurHackers.ToPoints(), Bb.TheirUnits);

        Solver.MoveAndAttack(Bb.OurTurrets.ToPoints(), Bb.TheirUnits);

        Solver.MoveAndAttack(Bb.OurClaws.ToPoints(), Bb.TheirHangars);

        Solver.MoveAndAttack(Bb.OurRepairers.ToPoints(), Bb.OurHangars);
        Solver.MoveAndAttack(Bb.OurRepairers.ToPoints(), Bb.OurUnits);
        Bb.ReadBoard();
        if (Bb.OurTerminators.ToPoints().Any())
        {
            var saveMe = Bb.OurTerminators.ToPoints().MaxBy(t => Bb.GetSpawnDelay(t));
            Bb.OurRepairers.ToPoints().ForEach(p => Solver.MoveCloseTo(p, saveMe));
        }

        Solver.MoveAndAttack(Bb.OurUnits.ToPoints(), new BitArray(Bb.TheirUnits).And(new BitArray(Bb.TheirWalls).Not()));
        Solver.MoveAndAttack(Bb.OurUnits.ToPoints(), Bb.TheirUnits);
        Solver.MoveAndAttack(Bb.OurUnits.ToPoints(), Bb.TheirUnits);

        Bb.KillHangerCountDown--;

        return true;
    }

    // Choose and remove all but your favorite target from your choice
    private Solver.DroidTurn ChooseTurn(Droid droid, IEnumerable<Solver.DroidTurn> turns)
    {
        return turns.MaxBy(t => t.Steps.Count());
    }



    /// <summary>
    /// This function is called once, before your first turn.
    /// </summary>
    public override void init()
    {
        Bb.Init(this);
        int offset = 0;
        bool found = false;
        while (!found)
        {
            //find a location without a hangar
            for (int i = 0; i < tiles.Length; i++)
            {
                //make sure that the tile is near the edge
                if (tiles[i].X == (mapWidth() - 1) * playerID() + offset)
                {
                    bool hangarPresent = false;
                    //check for hangar
                    for (int z = 0; z < droids.Length; z++)
                    {
                        if (droids[z].X == tiles[i].X && droids[z].Y == tiles[i].Y)
                        {
                            hangarPresent = true;
                            break;
                        }
                    }
                    if (!hangarPresent)
                    {
                        found = true;
                        break;
                    }
                }
            }
            //if nothing was found then move away from the edge
            if (!found)
            {
                //if on the left
                if (playerID() == 0)
                {
                    offset++;
                }
                else
                {
                    //on the right
                    offset--;
                }
            }
        }
    }

    /// <summary>
    /// This function is called once, after your last turn.
    /// </summary>
    public override void end() { }

    public AI(IntPtr c)
        : base(c) { }
}
