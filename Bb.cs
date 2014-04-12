﻿using System;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Bb
{
    public static int Width;
    public static int Height;

    public static Dictionary<Point, Droid> DroidLookup;
    public static Dictionary<Point, Tile> TileLookup;

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

    public static BitArray OurSpawning;
    public static BitArray TheirSpawning;

    private static AI ai;
    private static int size;
    private static int id;

    private static BitArray[] allBoards;
    //= {   OurClaws, TheirClaws,
    //                                          OurRepairers, TheirRepairers,
    //                                          OurTurrets, TheirTurrets,
    //                                          OurTerminators, TheirTerminators,
    //                                          OurArchers, TheirArchers,
    //                                          OurHackers, TheirHackers,
    //                                          OurWalls, TheirWalls,
    //                                          OurHangars, TheirHangars,
    //                                          OurSpawning, TheirSpawning
    //                                      };

    public static void Init(AI new_ai)
    {
        ai = new_ai;
        id = ai.playerID();
        Width = ai.mapWidth();
        Height = ai.mapHeight();
        size = Width * Height;

        DroidLookup = new Dictionary<Point, Droid>();
        TileLookup = AI.tiles.ToDictionary(t => new Point(t.X, t.Y));

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

        OurSpawning = new BitArray(size);
        TheirSpawning = new BitArray(size);

        allBoards = new[]{  OurClaws, TheirClaws,
                            OurRepairers, TheirRepairers,
                            OurTurrets, TheirTurrets,
                            OurTerminators, TheirTerminators,
                            OurArchers, TheirArchers,
                            OurHackers, TheirHackers,
                            OurWalls, TheirWalls,
                            OurHangars, TheirHangars,
                            OurSpawning, TheirSpawning
                          };
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
        foreach (BitArray board in allBoards)
        {
            board.SetAll(false);
        }

        foreach (Tile tile in AI.tiles)
        {
            if (tile.TurnsUntilAssembled > 0)
            {
                int n = GetOffset(tile.ToPoint());
                if (tile.Owner == id)
                {
                    OurSpawning.Set(n, true);
                }
                else
                {
                    TheirSpawning.Set(n, true);
                }
            }
        }

        DroidLookup.Clear();

        foreach (Droid droid in AI.droids)
        {
            Point p = new Point(droid.X, droid.Y);
            int n = GetOffset(p);
            bool isOurs = droid.Owner == id;
            bool isTheirs = !isOurs;

            DroidLookup.Add(p, droid);

            switch ((Unit)droid.Variant)
            {
                case Unit.CLAW:
                    OurClaws.Set(n, isOurs);
                    TheirClaws.Set(n, isTheirs);
                    break;
                case Unit.ARCHER:
                    OurArchers.Set(n, isOurs);
                    TheirArchers.Set(n, isTheirs);
                    break;
                case Unit.REPAIRER:
                    OurRepairers.Set(n, isOurs);
                    TheirRepairers.Set(n, isTheirs);
                    break;
                case Unit.HACKER:
                    OurHackers.Set(n, isOurs);
                    TheirHackers.Set(n, isTheirs);
                    break;
                case Unit.TURRET:
                    OurTurrets.Set(n, isOurs);
                    TheirTurrets.Set(n, isTheirs);
                    break;
                case Unit.WALL:
                    OurWalls.Set(n, isOurs);
                    TheirWalls.Set(n, isTheirs);
                    break;
                case Unit.TERMINATOR:
                    OurTerminators.Set(n, isOurs);
                    TheirTerminators.Set(n, isTheirs);
                    break;
                case Unit.HANGAR:
                    OurHangars.Set(n, isOurs);
                    TheirHangars.Set(n, isTheirs);
                    break;
                default:
                    Console.WriteLine("Could not find variant: " + droid.Variant);
                    break;
            }
        }

        OurUnits.Or(OurClaws).Or(OurRepairers).Or(OurTurrets).Or(OurTerminators).Or(OurArchers).Or(OurHackers).Or(OurWalls).Or(OurHangars);
        TheirUnits.Or(TheirClaws).Or(TheirRepairers).Or(TheirTurrets).Or(TheirTerminators).Or(TheirArchers).Or(TheirHackers).Or(TheirWalls).Or(TheirHangars);
    }
}
