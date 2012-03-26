using System;
using System.Threading;

namespace ClientAI
{
    public partial class Client
    {
        private readonly Random rnd = new Random();

        // Implement your AI here
        // Use these functions to communicate with server:
        //    SetName(name) - Sets your name to appear in the simulator
        //    GetState() - returns the current GameState object
        //    Move(x, y) - applies a wind in the given direction (returns true if OK, false if IGNORED)
        public void RunAI()
        {
            SetName("StupidAI");

            char[,] map = WaitForMap();

            for (int r = 0; r < map.GetLength(1); r++)
            {
                for (int c = 0; c < map.GetLength(0); c++)
                    Console.Write(map[c, r]);
                
                Console.WriteLine();
            }

            while (Connected)
            {
                // Poll the game state
                GameState state = GetState();
                if (state == null) break;

                // Ignore game state and do something random!
                Move((float) rnd.NextDouble() * 20 - 10, (float) rnd.NextDouble() * 20 - 10);

                Thread.Sleep(500);
            }
        }
    }
}