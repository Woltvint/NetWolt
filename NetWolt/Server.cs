using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;


namespace NetWolt
{
    public class Server
    {
        private Socket serverSocket;

        private readonly object idCounterLock = new object();
        private int idCounter;

        private readonly object networkClientsLock = new object();
        private Dictionary<int, NetworkClient> networkClients;
        private Dictionary<Socket, int> sockets;

        private readonly object newClientsLock = new object();
        private List<int> newClients;

        private readonly object disconectedClientsLock = new object();
        private List<int> disconectedClients;

        private Thread serverThread;

        public bool debugLog;

        public Server(int port, bool debug = false)
        {
            idCounter = 0;

            debugLog = debug;

            networkClients = new Dictionary<int, NetworkClient>();
            sockets = new Dictionary<Socket, int>();

            newClients = new List<int>();
            disconectedClients = new List<int>();

            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(new IPEndPoint(IPAddress.Any, port));
            serverSocket.Listen(10);
            serverSocket.BeginAccept(new AsyncCallback(acceptCallBack), null);

            serverThread = new Thread(serverLoop);
            serverThread.Start();

            log("Server started!");
        }

        private void acceptCallBack(IAsyncResult AR)
        {
            Socket socket = serverSocket.EndAccept(AR);
            log("Incoming connection");

            int id;

            lock (networkClientsLock)
            {

                lock (idCounterLock)
                {
                    id = idCounter;
                    idCounter++;
                }

                networkClients.Add(id, new NetworkClient(socket));
                sockets.Add(socket, id);

                lock (newClientsLock)
                {
                    newClients.Add(id);
                }


            }

            log($"Client {id} connected");
            serverSocket.BeginAccept(new AsyncCallback(acceptCallBack), null);
            socket.BeginReceive(networkClients[id].buffer, 0, networkClients[id].buffer.Length, SocketFlags.None, receiveCallBack, socket);
        }

        private void receiveCallBack(IAsyncResult AR)
        {
            Socket socket = (Socket)AR.AsyncState;

            int client;

            lock (networkClientsLock)
            {
                client = sockets[socket];

                int received = 0;

                try
                {
                    received = socket.EndReceive(AR);
                }
                catch (Exception)
                {
                }


                if (received <= 0)
                {
                    networkClients.Remove(client);
                    sockets.Remove(socket);

                    lock (disconectedClientsLock)
                    {
                        disconectedClients.Add(client);
                    }
                    
                    log($"Client {client} disconected");
                    return;
                }

                List<byte> bytes = new List<byte>();

                for (int i = 0; i < received; i++)
                {
                    networkClients[client].receivedBytes.Add(networkClients[client].buffer[i]);
                }

                int commands = 0;

                while (true)
                {
                    Command cmd = Command.parseCommand(networkClients[client].receivedBytes);

                    if (!cmd.complete)
                    {
                        break;
                    }

                    networkClients[client].receivedCommands.Add(cmd);
                    networkClients[client].receivedBytes.RemoveRange(0, cmd.cmdLen);
                    commands++;
                }

                log($"received {commands} commands from {client}");

            }

            socket.BeginReceive(networkClients[client].buffer, 0, networkClients[client].buffer.Length, SocketFlags.None, receiveCallBack, socket);
        }

        private void sendCallBack(IAsyncResult AR)
        {
            Socket socket = (Socket)AR.AsyncState;
            socket.EndSend(AR);
        }

        private void serverLoop()
        {
            while (true)
            {
                Thread.Sleep(10);

                lock (networkClientsLock)
                {

                    foreach (KeyValuePair<int, NetworkClient> client in networkClients)
                    {
                        if (client.Value.toSendBytes.Count > 0)
                        {
                            log($"Attempting to send {client.Value.toSendBytes.Count} bytes to {client.Key}");

                            try
                            {
                                client.Value.clientSocket.BeginSend(client.Value.toSendBytes.ToArray(), 0, client.Value.toSendBytes.Count, SocketFlags.None, sendCallBack, client.Value.clientSocket);
                                client.Value.toSendBytes.Clear();
                                log("Command sent");
                            }
                            catch (Exception)
                            {
                                log("Command sending failed");
                            }

                        }
                    }

                }
            }
        }

        public List<int> getNewClients()
        {
            lock (newClientsLock)
            {
                List<int> output = new List<int>();
                output.AddRange(newClients);
                newClients.Clear();
                return output;
            }
        }
        public List<int> getDisconnectedClients()
        {
            lock (disconectedClientsLock)
            {
                List<int> output = new List<int>();
                output.AddRange(disconectedClients);
                disconectedClients.Clear();
                return output;
            }
        }


        public List<int> getAllClients()
        {
            List<int> output = new List<int>();

            lock (networkClientsLock)
            {
                foreach (KeyValuePair<int, NetworkClient> client in networkClients)
                {
                    output.Add(client.Key);
                }
            }

            return output;
        }

        public bool newCommands(int clientId)
        {
            lock (networkClients)
            {
                if (networkClients.ContainsKey(clientId))
                {
                    return networkClients[clientId].receivedCommands.Count > 0;
                }
            }

            return false;
        }

        public Command nextCommand(int clientId)
        {
            Command cmd = new Command();

            lock (networkClientsLock)
            {
                if (networkClients.ContainsKey(clientId))
                {
                    if (networkClients[clientId].receivedCommands.Count > 0)
                    {
                        cmd = networkClients[clientId].receivedCommands[0];
                        networkClients[clientId].receivedCommands.RemoveAt(0);
                    }
                }
            }

            return cmd;
        }

        public void sendCommand(int clientId, Command cmd)
        {
            List<byte> bytes = cmd.sendableFormat();

            lock (networkClientsLock)
            {
                if (networkClients.ContainsKey(clientId))
                {
                    networkClients[clientId].toSendBytes.AddRange(bytes);
                }
            }
        }

        public void sendCommandToAll(Command cmd)
        {
            List<byte> bytes = cmd.sendableFormat();

            lock (networkClientsLock)
            {
                foreach (KeyValuePair<int,NetworkClient> client in networkClients)
                {
                    client.Value.toSendBytes.AddRange(bytes);
                }
            }
        }

        private void log(string text)
        {
            if (debugLog)
            {
                Console.WriteLine("[NetWolt-Server] " + text);
            }
        }
    }
}
