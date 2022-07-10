using System;
using System.Collections.Generic;

using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace HttpServer1
{
    partial class ThreadedHTTPServer
    {
        // configuration
        IPEndPoint ipEndPoint;
        Socket listeningSocket;

        List<ClientHelper> activeClients;

        // constructor
        public ThreadedHTTPServer(IPAddress ipAddress, int ipPort)
        {
            ipEndPoint = new IPEndPoint(ipAddress, ipPort);
            listeningSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            Console.WriteLine("Server created.");
            
            activeClients = new List<ClientHelper>();
        }

        // starting the server
        public void Start()
        {
            listeningSocket.Bind(ipEndPoint);
            listeningSocket.Listen(1);

            Console.WriteLine("Server @ {0} started.", ipEndPoint);
            do
            {
                Console.WriteLine("Server @ {0} waits for a client...", ipEndPoint);
                Socket clientSocket = listeningSocket.Accept(); 

                Console.WriteLine("Server @ {0} client connected @ {1}.", ipEndPoint, clientSocket.RemoteEndPoint);
                Console.WriteLine("Server @ {0} starting client thread.", ipEndPoint, clientSocket.RemoteEndPoint);

                ClientHelper ch = new ClientHelper(clientSocket, this);

                activeClients.Add(ch);

                Thread t = new Thread(ch.ProcessCommunication);

                t.Start();

            }
            while (true); 
        }

        void RemoveClient(ClientHelper ch)
        {
            activeClients.Remove(ch);
        }

        public void Stop()
        {
            listeningSocket.Close();
        }

    } // server

} // namespace
