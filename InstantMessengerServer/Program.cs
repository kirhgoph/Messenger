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
        void LoadUsers()
        {
            SqlCommand myCommand = new SqlCommand("SELECT *  FROM [Messenger].[dbo].[User];", SQLConnection);
            SqlDataReader myReader = null;
            myReader = myCommand.ExecuteReader();
            myReader.Read();
            Users.Add(new User() { Id = myReader.GetInt32(0), First_Name = myReader.GetString(1), Last_name = myReader.GetString(2), Birth_date = myReader.GetDateTime(3), Pass_hash = myReader.GetString(4), Login = myReader.GetString(5), e_mail = myReader.GetString(6), Status = myReader.GetInt32(7), Date_status = myReader.GetDateTime(8) });
        }
        void SaveUsers()
        {
            //SqlCommand myCommand = new SqlCommand("DELETE from table [Messenger].[dbo].[User];");
            //myCommand.ExecuteNonQuery();
            //myCommand.CommandText = "INSERT INTO table dbo.User (Login, [e-mail], Pass_hash) values ('" + userName + "','" + userName + "','" + password + "')";
            //myCommand.ExecuteNonQuery();
        }
        public User FindLogin(string Login)
        {
            return Users.Find(p => p.Login == Login);
        }
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
        public List<User> Users = new List<User>();
        public Program()
        {
            int state = State_OK;
            Console.Title = "InstantMessenger Server";
            Console.WriteLine("----- InstantMessenger Server -----");
            Console.WriteLine("[{0}] Starting server...", DateTime.Now);

            SQLConnection = new SqlConnection("user id=sa;" +
                                       "password=Informer1$;server=localhost\\SQLExpress;" +
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
            LoadUsers();
            Console.WriteLine("[{0}] Users were loaded successfully!", DateTime.Now);
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
    public class User
    {
        public int Id { get; set; }
        public string First_Name { get; set; }
        public string Last_name { get; set; }
        public DateTime Birth_date { get; set; }
        public string Pass_hash { get; set; }
        public string Login { get; set; }
        public string e_mail { get; set; }
        public int Status { get; set; }
        public DateTime Date_status { get; set; }
    }
}
