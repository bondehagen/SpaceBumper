using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;

namespace SpaceBumper
{
    public class SocketHandler : IInputHandler
    {
        private const int bufferSize = 256;
        private readonly TcpClient client;
        private readonly Queue<string> commandQueue;
        private readonly byte[] data;
        private readonly NetworkStream netStream;
        private bool disposed;
        private string playerName;
        private dynamic state;
        private readonly char[] separators;

        public SocketHandler(TcpClient client)
        {
            commandQueue = new Queue<string>();
            this.client = client;
            data = new byte[bufferSize];
            netStream = this.client.GetStream();
            netStream.BeginRead(data, 0, bufferSize, ReceiveMessage, null);
            separators = new[] { '\n', '\r', '\f', '\0', (char)3 };
        }

        public void Update(int iteration, World world, Bumpership bumpership)
        {
            if (!IsConnected())
            {
                Dispose();
                return;
            }

            bumpership.Name = playerName;

            state = new
            {
                player = bumpership,
                world,
                iteration
            };

            while (commandQueue.Any())
            {
                ParseMessage(commandQueue.Dequeue(), bumpership);
            }
        }

        public void Dispose()
        {
            if (disposed) return;
            netStream.Close();
            client.Close();
            disposed = true;
        }

        public void Start(World world)
        {
            StringBuilder message = new StringBuilder();
            SendMap(message, world);
            SendMessage("START\n");
        }

        private void SendMap(StringBuilder message, World world) {
            message.AppendFormat("BEGIN_MAP {0} {1}\n", world.Map.Width, world.Map.Height);
            for (int r = 0; r < world.Map.Height; r++)
            {
                for (int c = 0; c < world.Map.Width; c++)
                    message.Append(GetCellAsChar(world.Map.Grid[c, r].CellType));

                message.AppendLine();
            }

            message.AppendLine("END_MAP");
            SendMessage(message.ToString());
        }

        private char GetCellAsChar(CellType cellType)
        {
            char c;
            if (new Dictionary<CellType, char>
                {
                    { CellType.None, ' ' },
                    { CellType.Blocked, '#' },
                    { CellType.Boost, 'b' },
                    { CellType.SlowDown, 's' },
                    { CellType.Normal, '.' },
                    { CellType.Attractor, '.' }
                }.TryGetValue(cellType, out c))
                return c;
            return ' ';
        
        }

        private bool IsConnected()
        {
            try
            {
                return (client.Connected && client.Available != -1 && netStream.CanRead && netStream.CanWrite);
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public string WaitForName()
        {
            while (string.IsNullOrEmpty(playerName))
                Thread.Sleep(100);

            return playerName;
        }

        public void ReceiveMessage(IAsyncResult ar)
        {
            try
            {
                if (!client.Connected && !netStream.CanRead)
                    return;

                int bufferLength = netStream.EndRead(ar);
                string messageReceived = Encoding.ASCII.GetString(data, 0, bufferLength);
                if (!messageReceived.IsNullOrWhiteSpace())
                {
                    commandQueue.Enqueue(messageReceived);
                    CheckForNameMessage(messageReceived);
                    netStream.Flush();
                }
                netStream.BeginRead(data, 0, bufferSize, ReceiveMessage, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void CheckForNameMessage(string messageReceived)
        {
            string[] message = messageReceived.Split(separators, StringSplitOptions.RemoveEmptyEntries);
            KeyValuePair<string, string> pair = GetLines(message).FirstOrDefault(l => l.Key.Equals("NAME"));
            if (!pair.Equals(default(KeyValuePair<string, string>)))
                playerName = pair.Value;
        }

        private void ParseMessage(string messageReceived, Bumpership player)
        {
            string[] message = messageReceived.Split(separators, StringSplitOptions.RemoveEmptyEntries);

            foreach (KeyValuePair<string, string> line in GetLines(message))
            {
                if (state == null)
                    continue;

                if (line.Key.Equals("ACCELERATION"))
                {
                    player.Move(ParseVector(line.Value));
                    SendMessage("OK\n");
                }

                if (line.Key.Equals("GET_STATE"))
                    SendMessage(StateProtocol.Create(state.player, state.world, state.iteration));
            }
        }

        private static Vector ParseVector(string value)
        {
            string[] vector = value.Split(' ');
            double x = double.Parse(vector.First(), NumberFormatInfo.InvariantInfo);
            double y = double.Parse(vector.Last(), NumberFormatInfo.InvariantInfo);
            return new Vector(x, y);
        }

        private static IEnumerable<KeyValuePair<string, string>> GetLines(IEnumerable<string> strings)
        {
            return strings.Select(s =>
                                  {
                                      string[] split = s.Split(new[] { ' ' }, 2);
                                      return split.Any()
                                                 ? new KeyValuePair<string, string>(split.First().ToUpper(),
                                                                                    split.Last())
                                                 : new KeyValuePair<string, string>(s, "");
                                  });
        }

        public void SendMessage(string message)
        {
            try
            {
                byte[] bytesToSend = Encoding.ASCII.GetBytes(message);
                client.Client.Send(bytesToSend);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}