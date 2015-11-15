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
        public string SafeGetString(SqlDataReader reader, int colIndex)
        {
            if (!reader.IsDBNull(colIndex))
                return reader.GetString(colIndex);
            else
                return string.Empty;
        }
        public DateTime SafeGetDate(SqlDataReader reader, int colIndex)
        {
            if (!reader.IsDBNull(colIndex))
                return reader.GetDateTime(colIndex);
            else
                return DateTime.MinValue;
        }
        public int SafeGetInt(SqlDataReader reader, int colIndex)
        {
            if (!reader.IsDBNull(colIndex))
                return reader.GetInt32(colIndex);
            else
                return -1;
        }
        public void LoadUsers()
        {
            SqlCommand myCommand = new SqlCommand("SELECT *  FROM [Messenger].[dbo].[User];", SQLConnection);
            SqlDataReader myReader = null;
            myReader = myCommand.ExecuteReader();
            try
            { 
            while (myReader.Read())
                Users.Add(new User() { Id = SafeGetInt(myReader, 0), First_Name = SafeGetString(myReader, 1), Last_name = SafeGetString(myReader, 2), Birth_date = SafeGetDate(myReader, 3), Pass_hash = SafeGetString(myReader, 4), Login = SafeGetString(myReader, 5), e_mail = SafeGetString(myReader, 6), Status = SafeGetInt(myReader, 7), Date_status = SafeGetDate(myReader, 8) });
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            myReader.Close();
        }
        public void SaveUsers(User user)
        {
            string com = "INSERT into [Messenger].[dbo].[User] (Id, Pass_hash,Login,[e-mail],Status) values(" + user.Id + "," + user.Pass_hash + "," + user.Login + "," + user.e_mail + ","+user.Status+");";
            SqlCommand myCommand = new SqlCommand("INSERT into [Messenger].[dbo].[User] (Id, Pass_hash,Login,[e-mail],Status) values(" + user.Id + "," + user.Pass_hash + "," + user.Login + "," + user.e_mail + "," + user.Status + ");", SQLConnection);
            try
            {
                myCommand.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            //SqlCommand myCommand = new SqlCommand("DELETE from table [Messenger].[dbo].[User];");
            //myCommand.ExecuteNonQuery();
            //myCommand.CommandText = "INSERT INTO table dbo.User (Login, [e-mail], Pass_hash) values ('" + userName + "','" + userName + "','" + password + "')";
            //myCommand.ExecuteNonQuery();
        }
        public void LoadContacts()
        {
            SqlCommand myCommand = new SqlCommand("SELECT *  FROM [Messenger].[dbo].[Contacts];", SQLConnection);
            SqlDataReader myReader = null;
            myReader = myCommand.ExecuteReader();
            while (myReader.Read())
            Contacts.Add(new Contact() { Id = myReader.GetInt32(0), Id_user= myReader.GetInt32(1), Id_contact = myReader.GetInt32(2), Id_grupp = myReader.GetInt32(3), Name_for_user = myReader.GetString(4)});
            myReader.Close();
        }
        void SaveContacts()
        {
            //SqlCommand myCommand = new SqlCommand("DELETE from table [Messenger].[dbo].[User];");
            //myCommand.ExecuteNonQuery();
            //myCommand.CommandText = "INSERT INTO table dbo.User (Login, [e-mail], Pass_hash) values ('" + userName + "','" + userName + "','" + password + "')";
            //myCommand.ExecuteNonQuery();
        }
        public void LoadGroups()
        {
            SqlCommand myCommand = new SqlCommand("SELECT *  FROM [Messenger].[dbo].[Grupps];", SQLConnection);
            SqlDataReader myReader = null;
            myReader = myCommand.ExecuteReader();
            while (myReader.Read())
                Groups.Add(new Group() { Id = myReader.GetInt32(0), Name = myReader.GetString(1)});
            myReader.Close();
        }
        void SaveGroups()
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
        public List<Contact> Contacts = new List<Contact>();
        public List<Group> Groups= new List<Group>();
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
            try
            {
                LoadUsers();
            }
            catch
            {
                Console.WriteLine("[{0}] Error loading users!", DateTime.Now);
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
        public Client Connection;
    }
    public class Contact
    {
        public int Id { get; set; }
        public int Id_user{ get; set; }
        public int Id_contact{ get; set; }
        public int Id_grupp{ get; set; }
        public string Name_for_user{ get; set; }
    }
    public class Group
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

}
