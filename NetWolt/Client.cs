using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace NetWolt
{
    public class Client
    {
        private Socket clientSocket;

        private readonly object receivedCommandsLock = new object();
        private List<Command> receivedCommands;

        private List<byte> receivedBytes;

        private readonly object toSendBytesLock = new object();
        private List<byte> toSendBytes;

        private byte[] buffer = new byte[1024];

        private string _address;
        private int _port;

        private Thread clientThread;

        public bool debugLog;
        public bool reconectTry;

        public Client(string address, int port, bool debug = false, bool reconect = false)
        {
            debugLog = debug;
            reconectTry = reconect;

            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            receivedCommands = new List<Command>();

            receivedBytes = new List<byte>();
            toSendBytes = new List<byte>();

            _address = address;
            _port = port;

            clientThread = new Thread(clientLoop);
            clientThread.Start();
        }

        private void receiveCallBack(IAsyncResult AR)
        {
            Socket socket = (Socket)AR.AsyncState;

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
                log($"Server disconected");
                return;
            }

            List<byte> bytes = new List<byte>();

            for (int i = 0; i < received; i++)
            {
                receivedBytes.Add(buffer[i]);
            }

            int commands = 0;

            while (true)
            {
                Command cmd = Command.parseCommand(receivedBytes);

                if (!cmd.complete)
                {
                    break;
                }

                receivedCommands.Add(cmd);
                receivedBytes.RemoveRange(0, cmd.cmdLen);
                commands++;
            }

            log($"received {commands} commands from server");

            clientSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, receiveCallBack, clientSocket);
        }

        private void sendCallBack(IAsyncResult AR)
        {
            Socket socket = (Socket)AR.AsyncState;
            socket.EndSend(AR);
        }

        private void clientLoop()
        {
            connect();

            while (true)
            {
                Thread.Sleep(16);

                if (!clientSocket.Connected)
                {
                    if (reconectTry)
                    {
                        log("Attempting to reconect to the server");
                        clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        connect();
                    }
                    continue;
                }

                if (toSendBytes.Count > 0)
                {
                    log($"Attempting to send {toSendBytes.Count} bytes");

                    try
                    {
                        clientSocket.BeginSend(toSendBytes.ToArray(), 0, toSendBytes.Count, SocketFlags.None, sendCallBack, clientSocket);
                        toSendBytes.Clear();
                        log("Command sent");
                    }
                    catch (Exception)
                    {
                        log("Command sending failed");
                    }

                }

            }
        }

        private void connect()
        {
            int attempts = 0;

            while (!clientSocket.Connected)
            {
                try
                {
                    attempts++;
                    clientSocket.Connect(_address, _port);
                }
                catch (SocketException)
                {
                    log("Failed: " + attempts.ToString());
                }

                if (attempts > 10)
                {
                    log("Connecting failed");
                    return;
                }
            }

            clientSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, receiveCallBack, clientSocket);

            log("Client connected");
        }

        public bool newCommands()
        {
            lock (receivedCommandsLock)
            {
                return receivedCommands.Count > 0;
            }
        }

        public Command nextCommand()
        {
            Command cmd = new Command();

            lock (receivedCommandsLock)
            {
                if (receivedCommands.Count > 0)
                {
                    cmd = receivedCommands[0];
                    receivedCommands.RemoveAt(0);
                }
            }

            return cmd;
        }

        public void sendCommand(Command cmd)
        {
            List<byte> bytes = cmd.sendableFormat();

            lock (toSendBytesLock)
            {
                toSendBytes.AddRange(bytes);
            }
        }

        private void log(string text)
        {
            if (debugLog)
            {
                Console.WriteLine("[NetWolt-Client] " + text);
            }
        }
    }
}
