using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Collections;

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
        var spawned = false;
        if( IsSpawnable(spawnspot) && CanAfford(u) )
        {
            spawned = players[playerID()].orbitalDrop(spawnspot.x, spawnspot.y, (int)u);
        }
        return spawned;
    }

    private bool TakeOutTurrets()
    {
        bool spawned = false;
        foreach (Point p in Bb.TheirTurrets.ToPoints())
        {
            if (CanAfford(Unit.CLAW) && IsSpawnable(p))
            {
                spawned = true;
                Console.WriteLine(" Turn " + turnNumber() + ": Dropping Claw onto turret at (" + p.x + ", " + p.y + ")!");
                players[playerID()].orbitalDrop(p.x, p.y, (int)Unit.CLAW);
                Bb.ReadBoard();
            }
        }
        return spawned;
    }

    /// <summary>
    /// This function is called each time it is your turn.
    /// </summary>
    /// <returns>True to end your turn. False to ask the server for updated information.</returns>
    public override bool run()
    {
        Bb.ReadBoard();

        var p0hangars = "; ";
        var p1hangars = "; ";

        if (playerID() == 0)
        {
            p0hangars += "OurHangar = " + Bb.OurHangars.ToPoints().Count();
            p1hangars += "TheirHangar = " + Bb.TheirHangars.ToPoints().Count();
        }
        else
        {
            p0hangars += "TheirHangar = " + Bb.OurHangars.ToPoints().Count();
            p1hangars += "OurHangar = " + Bb.TheirHangars.ToPoints().Count();
        }

        Console.WriteLine("Turn: " + turnNumber() + p0hangars + p1hangars);
        

        if (!TakeOutTurrets())
        {
            if (SpawnUnit(Unit.HACKER))
            {
                Console.WriteLine("     Hacker spawned");
            }
        }
        //Spawn Claw by default
        Unit unit = Unit.CLAW;
        while (CanAfford(unit) && Bb.OurClaws.ToPoints().Count() <= 15)
        {
            SpawnUnit(unit);
            Bb.ReadBoard();
        }

        
        // Move hackers first so we can move other units this turn
        //Solver.MoveAndAttack(Bb.OurHackers.ToPoints(), new BitArray(Bb.TheirUnits).And(new BitArray(Bb.TheirHangars).Not()).And(new BitArray(Bb.TheirWalls).Not()));
        if (Bb.OurHackers.ToPoints().Any())
        {
            if (Bb.TheirHackers.ToPoints().Any())
            {
                // Prioritize hacking enemy hackers
                Solver.MoveAndAttack(Bb.OurHackers.ToPoints(), Bb.TheirHackers);
            }
            else
            {
                // Otherwise hack any other hackable enemies
                Solver.MoveAndAttack(Bb.OurHackers.ToPoints(), Bb.TheirUnits);
            }
        }
        Bb.ReadBoard();

        Solver.MoveAndAttack(Bb.OurTurrets.ToPoints(), Bb.TheirUnits);
        Bb.ReadBoard();
        Solver.MoveAndAttack(Bb.OurClaws.ToPoints(), Bb.TheirHangars);
        Bb.ReadBoard();
        Solver.MoveAndAttack(Bb.OurClaws.ToPoints(), Bb.TheirUnits);
        Bb.ReadBoard();

        return true;
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
