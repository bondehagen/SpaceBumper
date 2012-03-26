using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text;
using System.Windows;
using System.Globalization;

namespace ClientAI
{
    public class Bumpership
    {
        public Vector Position;
        public Vector Velocity;
        public float Score;

        public Bumpership(float x, float y, float vx, float vy, float score)
        {
            this.Position = new Vector(x, y);
            this.Velocity = new Vector(vx, vy);
            this.Score = score;
        }
    }

    public class GameState
    {
        public readonly List<Bumpership> Bumperships = new List<Bumpership>();
        public readonly List<Vector> Stars = new List<Vector>();
        public int MeIndex;
        public int Iteration;
        public Bumpership Me
        {
            get { return Bumperships[MeIndex]; }
        }
    }

    public partial class Client
    {
        private readonly TcpClient client = new TcpClient();
        private readonly StreamReader reader;
        private readonly StreamWriter writer;
        private readonly IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1986);
        
        public GameState GetState()
        {
            try
            {
                writer.WriteLine("GET_STATE");
                writer.Flush();

                GameState state = new GameState();
                bool end = false;
                while (!end)
                {
                    string line = reader.ReadLine();
                    if (line == null)
                        continue;

                    string[] msg = line.Split(' ');
                    switch (msg[0])
                    {
                        case "BEGIN_STATE":
                            state.Iteration = int.Parse(msg[1]);
                            break;
                        case "END_STATE": end = true; break;
                        case "BUMPERSHIP":
                            state.Bumperships.Add(new Bumpership(
                                float.Parse(msg[1], NumberFormatInfo.InvariantInfo),
                                float.Parse(msg[2], NumberFormatInfo.InvariantInfo),
                                float.Parse(msg[3], NumberFormatInfo.InvariantInfo),
                                float.Parse(msg[4], NumberFormatInfo.InvariantInfo),
                                float.Parse(msg[5], NumberFormatInfo.InvariantInfo)));
                            break;
                        case "STAR":
                            state.Stars.Add(new Vector(float.Parse(msg[1], NumberFormatInfo.InvariantInfo),
                                                       float.Parse(msg[2], NumberFormatInfo.InvariantInfo)));
                            break;
                        case "YOU": state.MeIndex = int.Parse(msg[1]); break;
                    }
                }
                return state;
            }
            catch (Exception) { return null; }
        }
        
        public char[,] WaitForMap()
        {
            char[,] map = null;
            bool end = false;
            int r = 0;
            while (!end)
            {
                string line = reader.ReadLine();
                if (line == null)
                    continue;

                string[] msg = line.Split(' ');
                switch (msg[0])
                {
                    case "BEGIN_MAP":
                        map = new char[int.Parse(msg[1]),int.Parse(msg[2])];
                        break;
                    case "END_MAP":
                        end = true;
                        break;
                    default:
                        if(map != null)
                        {
                            for (var c = 0; c < line.Length; c++)
                            {
                                map[c, r] = line[c];
                            }
                            r++;
                        }
                        break;
                }
            }
            return map;
        }

        public void SetName(string name)
        {
            writer.WriteLine("NAME " + name);
            writer.Flush();
        }

        public bool Move(float x, float y)
        {
            writer.WriteLine("ACCELERATION " + x.ToString(NumberFormatInfo.InvariantInfo) + " "
                             + y.ToString(NumberFormatInfo.InvariantInfo));
            writer.Flush();
            string response = reader.ReadLine();
            return response == "OK";
        }

        public bool Connected
        {
            get { return client.GetStream().CanWrite; }
        }

        public Client()
        {
            // Connect to server
            client.Connect(endPoint);
            reader = new StreamReader(client.GetStream(), Encoding.ASCII);
            writer = new StreamWriter(client.GetStream(), Encoding.ASCII);

            // Runs the implementation in MyAI.cs
            RunAI();
        }

        public static void Main(string[] args)
        {
            new Client();
        }
    }
}