using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SpaceBumper
{
    public class SocketManager : IDisposable
    {
        private readonly IList<SocketHandler> clients;
        private readonly int maxClients;
        private readonly TcpListener tcpListener;
        private int counter;

        public SocketManager(int maxClients, int port)
        {
            this.maxClients = maxClients;
            clients = new List<SocketHandler>();
            tcpListener = new TcpListener(IPAddress.Any, port);
            Thread.Sleep(100);
            counter = 0;
        }

        public int CountClients
        {
            get { return clients.Count; }
        }

        public bool HasIncoming
        {
            get { return tcpListener.Pending(); }
        }

        public void Dispose()
        {
            foreach (SocketHandler socketHandler in clients)
                socketHandler.Dispose();

            tcpListener.Stop();
        }

        public void Start()
        {
            tcpListener.Start(maxClients);
        }

        public SocketHandler ConnectPlayer()
        {
            TcpClient client = tcpListener.AcceptTcpClient();
            SocketHandler player = new SocketHandler(client);
            clients.Add(player);
            return player;
        }

        public void Stop()
        {
            Dispose();
        }

        public IInputHandler GetNetworkPlayer()
        {
            SocketHandler socketHandler = clients[counter];
            counter++;
            return socketHandler;
        }
    }
}