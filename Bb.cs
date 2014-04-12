using System;
using System.Linq;
using System.Text;
using System.Collections;

class Bb
{
    public static int Width;
    public static int Height;

    public static BitArray OurUnits;
    public static BitArray TheirUnits;

    public static BitArray OurClaws;
    public static BitArray TheirClaws;
    public static BitArray OurRepairers;
    public static BitArray TheirRepairers;
    public static BitArray OurTurrets;
    public static BitArray TheirTurrets;
    public static BitArray OurTerminators;
    public static BitArray TheirTerminators;
    public static BitArray OurArchers;
    public static BitArray TheirArchers;
    public static BitArray OurHackers;
    public static BitArray TheirHackers;
    public static BitArray OurWalls;
    public static BitArray TheirWalls;
    public static BitArray OurHangars;
    public static BitArray TheirHangars;

    private static AI ai;
    private static int size;
    private static Tile[] tiles = AI.tiles;
    private static int id;

    public static void Init(AI new_ai)
    {
        ai = new_ai;
        id = ai.playerID();
        Width = ai.mapWidth();
        Height = ai.mapHeight();
        size = Width * Height;

        OurUnits = new BitArray(size);
        TheirUnits = new BitArray(size);

        OurClaws = new BitArray(size);
        TheirClaws = new BitArray(size);
        OurRepairers = new BitArray(size);
        TheirRepairers = new BitArray(size);
        OurTurrets = new BitArray(size);
        TheirTurrets = new BitArray(size);
        OurTerminators = new BitArray(size);
        TheirTerminators = new BitArray(size);
        OurArchers = new BitArray(size);
        TheirArchers = new BitArray(size);
        OurHackers = new BitArray(size);
        TheirHackers = new BitArray(size);
        OurWalls = new BitArray(size);
        TheirWalls = new BitArray(size);
        OurHangars = new BitArray(size);
        TheirHangars = new BitArray(size);
    }

    public static int GetOffset(int x, int y)
    {
        return y * Width + x;
    }

    public static int GetOffset(Point p)
    {
        return GetOffset(p.x, p.y);
    }

    public static void ReadBoard()
    {
        OurClaws.SetAll(false);
        TheirClaws.SetAll(false);
        OurRepairers.SetAll(false);
        TheirRepairers.SetAll(false);
        OurTurrets.SetAll(false);
        TheirTurrets.SetAll(false);
        OurTerminators.SetAll(false);
        TheirTerminators.SetAll(false);
        OurArchers.SetAll(false);
        TheirArchers.SetAll(false);
        OurHackers.SetAll(false);
        TheirHackers.SetAll(false);
        OurWalls.SetAll(false);
        TheirWalls.SetAll(false);
        OurHangars.SetAll(false);
        TheirHangars.SetAll(false);

        foreach (Droid droid in AI.droids)
        {
            int x = droid.X;
            int y = droid.Y;
            if (droid.Owner == id)
            {
                // Ours
                switch ((Unit)droid.Variant)
                {
                    case Unit.CLAW:
                        if (droid.Owner == id)
                        {
                            
                        }
                        break;
                    case Unit.ARCHER:
                        break;
                    case Unit.REPAIRER:
                        break;
                    case Unit.HACKER:
                        break;
                    case Unit.TURRET:
                        break;
                    case Unit.WALL:
                        break;
                    case Unit.TERMINATOR:
                        break;
                    case Unit.HANGAR:
                        break;
                    default:
                        Console.WriteLine("Could not find variant: " + droid.Variant);
                        break;
                }
            }
            else
            {
                // Theirs
            }
        }

        foreach (Tile tile in tiles)
        {

        }

        OurUnits = OurClaws.Or(OurRepairers).Or(OurTurrets).Or(OurTerminators).Or(OurArchers).Or(OurHackers).Or(OurWalls).Or(OurHangars);
        TheirUnits = TheirClaws.Or(TheirRepairers).Or(TheirTurrets).Or(TheirTerminators).Or(TheirArchers).Or(TheirHackers).Or(TheirWalls).Or(TheirHangars);
    }
}
