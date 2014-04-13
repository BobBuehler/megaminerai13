using System;
using System.Linq;
using System.Runtime.InteropServices;

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
    int spawnX = 0, spawnY = 0;

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

    private void SpawnUnit(Unit u)
    {
        //Find best spawning spot, p
        var spawnspot = Solver.FindFastestSpawn(p => IsSpawnable(p), Bb.TheirUnits.ToPoints(), modelVariants[(int) u].MaxMovement);
        
        if( IsSpawnable(p) && CanAfford(u) )
        {
            players[playerID()].orbitalDrop(spawnspot.x, spawnspot.y, (int)u);
        }
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
        Console.WriteLine("Turn: " + turnNumber());
        
        Bb.ReadBoard();

        if (!TakeOutTurrets())
        {
            SpawnUnit(Unit.HACKER);
        }

        //If spawned on the right
        if (playerID() == 1)
        {
            spawnX = mapWidth() - 1;
        }
        Point spawnHere = new Point(spawnX, spawnY);
        //Spawn Claw by default
        Unit unit = Unit.CLAW;
        while (CanAfford(unit))
        {
            //if afford it, check 
            if (spawnHere.y >= mapHeight())
            {
                spawnHere.y = 0;
                spawnHere.x++;
            }
            if (IsSpawnable(spawnHere))
            {
                //if spawnable spawn the unit
                players[playerID()].orbitalDrop(spawnHere.x, spawnHere.y, (int)unit);
            }
            spawnHere.y++;
            Bb.ReadBoard();
        }

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
                        spawnX = tiles[i].X;
                        spawnY = tiles[i].Y;
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
