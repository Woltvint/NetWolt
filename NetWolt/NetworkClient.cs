using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace NetWolt
{
    internal class NetworkClient
    {
        public Socket clientSocket;

        public readonly object networkClientLock = new object();

        public byte[] buffer;

        public List<Command> receivedCommands;
        public List<byte> toSendBytes;
        public List<byte> receivedBytes = new List<byte>();

        public NetworkClient(Socket socket)
        {
            clientSocket = socket;

            buffer = new byte[1024*16];

            receivedCommands = new List<Command>();
            toSendBytes = new List<byte>();
            receivedBytes = new List<byte>();
        }
    }
}
