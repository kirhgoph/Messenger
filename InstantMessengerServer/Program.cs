using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Security.Cryptography.X509Certificates;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Data.SqlClient;

namespace InstantMessengerServer
{
    public class Program
    {
        static void Main(string[] args)
        {
            Program p = new Program();
            Console.WriteLine();
            Console.WriteLine("Press enter to close program.");
            Console.ReadLine();
        }

        // Self-signed certificate for SSL encryption.
        // You can generate one using my generate_cert script in tools directory (OpenSSL is required).
        public X509Certificate2 cert = new X509Certificate2("server.pfx", "instant");
        
        // IP of this computer. If you are running all clients at the same computer you can use 127.0.0.1 (localhost). 
        public IPAddress ip = IPAddress.Parse("127.0.0.1");
        public int port = 2000;
        public bool running = true;
        public TcpListener server;
        public SqlConnection SQLConnection;
        public const int State_OK = 0;
        public const int State_TCPErr = 1;
        public const int State_SQLErr = 2;

        public Program()
        {
            int state = State_OK;
            Console.Title = "InstantMessenger Server";
            Console.WriteLine("----- InstantMessenger Server -----");
            Console.WriteLine("[{0}] Starting server...", DateTime.Now);

            SQLConnection = new SqlConnection("user id=sa;" +
                                       "password=Informer1$;server=localhost;" +
                                       "Trusted_Connection=yes;" +
                                       "database=Messenger; " +
                                       "connection timeout=30");
            try
            {
                SQLConnection.Open();
            }
            catch (Exception e)
            {
                Console.WriteLine("[{0}] Error connecting to SQL server!", DateTime.Now);
                state = State_SQLErr;
            }

            server = new TcpListener(ip, port);
            try
            {
                server.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine("[{0}] Error starting TCP socket!", DateTime.Now);
                state = State_TCPErr;
            }
            if (state==State_OK)
                Console.WriteLine("[{0}] Server is running properly!", DateTime.Now);


            
            Listen();
        }

        void Listen()  // Listen to incoming connections.
        {
            while (running)
            {
                TcpClient tcpClient = server.AcceptTcpClient();  // Accept incoming connection.
                Client client = new Client(this, tcpClient);     // Handle in another thread.
            }
        }

    }
}
