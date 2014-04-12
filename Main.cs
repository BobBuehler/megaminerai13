using System;
using System.Runtime.InteropServices;
using System.Collections;

public class Program
{
    public static void Main(string[] args)
    {
        if (args.Length < 1)
        {
            System.Console.WriteLine("Please enter a hostname");

            Bb.Width = 5;
            Bb.Height = 5;

            var passable = new BitArray(new bool[] {
                true, false, true, true, true,
                true, false, true, true, true,
                true, false, false, false, true,
                true, false, true, true, true,
                true, true, true, true, true
            });

            var start = new Point(0, 0);
            var goal = new Point(2, 0);

            var path = Pather.AStar(
                new[] { start },
                passable.ToFunc(),
                p => p.Equals(goal),
                (p1, p2) => p2.y == 0 ? 10 : 1,
                p => 0);

            path.ForEach(p => Console.WriteLine(p));
            Console.WriteLine("Done");

            return;
        }

        IntPtr connection = Client.createConnection();

        AI ai = new AI(connection);
        if (Client.serverConnect(connection, args[0], "19000") == 0)
        {
            System.Console.WriteLine("Unable to connect to server");
            return;
        }

        if (Client.serverLogin(connection, ai.username(), ai.password()) == 0)
            return;

        if (args.Length < 2)
            Client.createGame(connection);
        else
            Client.joinGame(connection, Int32.Parse(args[1]), "player");

        while (Client.networkLoop(connection) != 0)
        {
            if (ai.startTurn())
                Client.endTurn(connection);
            else
                Client.getStatus(connection);
        }

        Client.networkLoop(connection); //Grab end game state
        Client.networkLoop(connection); //Grab log
        ai.end();
    }
}
