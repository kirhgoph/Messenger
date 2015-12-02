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
                {
                    User usr = new User() { Id = SafeGetInt(myReader, 0), First_Name = SafeGetString(myReader, 1), Last_name = SafeGetString(myReader, 2), Birth_date = SafeGetDate(myReader, 3), Pass_hash = SafeGetString(myReader, 4), Login = SafeGetString(myReader, 5), e_mail = SafeGetString(myReader, 6), Status = SafeGetInt(myReader, 7), Date_status = SafeGetDate(myReader, 8) };
                    usr.Status = 0;
                    Users.Add(usr);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            myReader.Close();
        }
        public void SaveUsers(User user)
        {
            string com = "INSERT into [Messenger].[dbo].[User] (Id, Pass_hash,Login,[e-mail],Status,Birth_date) values(" + user.Id + "," + user.Pass_hash + "," + user.Login + "," + user.e_mail + "," + user.Status + ",convert(datetime,'" + user.Birth_date.ToString("yyyy-MM-dd HH:mm:ss") + "',20));";
            SqlCommand myCommand = new SqlCommand("INSERT into [Messenger].[dbo].[User] (Id, Pass_hash,Login,[e-mail],Status,Birth_date) values(" + user.Id + "," + user.Pass_hash + "," + user.Login + "," + user.e_mail + "," + user.Status + ",convert(datetime,'" + user.Birth_date.ToString("yyyy-MM-dd HH:mm:ss") + "',20));", SQLConnection);
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
        public void EditUsers(User user)
        {
            string Birthdate_str = user.Birth_date.ToString("yyyy-MM-dd HH:mm:ss");
            string com = "UPDATE [Messenger].[dbo].[User] SET [Status]=" + user.Status +",[First_Name] = '" + user.First_Name + "',[Last_name] = '" + user.Last_name + "',[Birth_date] = convert(datetime,'" + Birthdate_str + "',20) WHERE [Login]=" + user.Login;
            SqlCommand myCommand = new SqlCommand("UPDATE [Messenger].[dbo].[User] SET [Status]=" + user.Status + ",[First_Name] = '" + user.First_Name + "',[Last_name] = '" + user.Last_name + "',[Birth_date] = convert(datetime,'" + Birthdate_str + "',20) WHERE [Login]=" + user.Login, SQLConnection);
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
            try
            {
                while (myReader.Read())
                {
                    Contact cont = new Contact() { Id = SafeGetInt(myReader, 0), Id_user = SafeGetInt(myReader, 1), Id_contact = SafeGetInt(myReader, 2), Id_grupp = SafeGetInt(myReader, 3), Name_for_user = SafeGetString(myReader, 4) };
                    Contacts.Add(cont);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            myReader.Close();
        }
        public void SaveContacts(Contact cont)
        {
            String com = "INSERT into [Messenger].[dbo].[Contacts] (Id,Id_user,Id_contact,ID_grupp,Name_for_user) values(" + cont.Id + "," + cont.Id_user + "," + cont.Id_contact + "," + cont.Id_grupp + "," + cont.Name_for_user + ");";
            SqlCommand myCommand = new SqlCommand("INSERT into [Messenger].[dbo].[Contacts] (Id,Id_user,Id_contact,ID_grupp,Name_for_user) values(" + cont.Id + "," + cont.Id_user + "," + cont.Id_contact + "," + cont.Id_grupp + "," + cont.Name_for_user + ");",SQLConnection);
            try
            {
            myCommand.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        public void DeleteContact(int id)
        {
            String com = "DELETE FROM [Messenger].[dbo].[Contacts] Where Id=" + id + ";";
            SqlCommand myCommand = new SqlCommand(com, SQLConnection);
            try
            {
                myCommand.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
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
        public void LoadPrivacy(string PrivacyType)
        {
            string com=null;
            switch(PrivacyType)
            { 
                case "Seeing":
                    com = "SELECT *  FROM [Messenger].[dbo].[List_vis];";
                    break;
                case "Unseeing":
                    com = "SELECT *  FROM [Messenger].[dbo].[List_invis];";
                    break;
                case "Ignoring":
                    com = "SELECT *  FROM [Messenger].[dbo].[List_ignor];";
                    break;
            }
            SqlCommand myCommand = new SqlCommand(com, SQLConnection);
            SqlDataReader myReader = null;
            myReader = myCommand.ExecuteReader();
            try
            {
                while (myReader.Read())
                {
                    Privacy_record prv = new Privacy_record() { Id = SafeGetInt(myReader, 0), Id_user= SafeGetInt(myReader, 1), Id_contact = SafeGetInt(myReader, 2)};
                    switch(PrivacyType)
                    {
                        case "Seeing":
                            Seeing.Add(prv);
                            break;
                        case "Unseeing":
                            Unseeing.Add(prv);
                            break;
                        case "Ignoring":
                            Ignoring.Add(prv);
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            myReader.Close();
        }
        public void AddPrivacy(string PrivacyType, Privacy_record prv)
        {
            string com = null;
            switch (PrivacyType)
            {
                case "Seeing":
                    com = "INSERT into [Messenger].[dbo].[List_vis] (Id, Id_user, Id_contact) values(" + prv.Id + "," + prv.Id_user + "," + prv.Id_contact + ");";
                    break;
                case "Unseeing":
                    com = "INSERT into [Messenger].[dbo].[List_invis] (Id, Id_user, Id_contact) values(" + prv.Id + "," + prv.Id_user + "," + prv.Id_contact + ");";
                    break;
                case "Ignoring":
                    com = "INSERT into [Messenger].[dbo].[List_ignor] (Id, Id_user, Id_contact) values(" + prv.Id + "," + prv.Id_user + "," + prv.Id_contact + ");";
                    break;
            }
            SqlCommand myCommand = new SqlCommand(com, SQLConnection);
            try
            {
                myCommand.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        public void DeletePrivacy(string PrivacyType, Privacy_record prv)
        {
            string com = null;
            switch (PrivacyType)
            {
                case "Seeing":
                    com = "DELETE from [Messenger].[dbo].[List_vis] where Id_user="+prv.Id_user+"and Id_contact="+prv.Id_contact+";";
                    break;
                case "Unseeing":
                    com = "DELETE from [Messenger].[dbo].[List_invis] where Id_user=" + prv.Id_user + "and Id_contact=" + prv.Id_contact + ";";
                    break;
                case "Ignoring":
                    com = "DELETE from [Messenger].[dbo].[List_ignor] where Id_user=" + prv.Id_user + "and Id_contact=" + prv.Id_contact + ";";
                    break;
            }
            SqlCommand myCommand = new SqlCommand(com, SQLConnection);
            try
            {
                myCommand.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        public User FindLogin(string Login)
        {
            return Users.Find(p => p.Login == Login);
        }
        public int GetImaginaryStatus(int Id_contact, int Id_user)
        {
            int realstatus = Users.Find(p => p.Id == Id_contact).Status;
            if (realstatus == 0) return 0;
            if (Ignoring.Find(p=> (p.Id_contact==Id_user) &&(p.Id_user==Id_contact))!=null) return 0;
            if (realstatus % 10 == 1) return realstatus;
            if (realstatus % 10 == 2) if (Seeing.Find(p => (p.Id_contact == Id_user) && (p.Id_user == Id_contact)) != null) return realstatus; else return 0;
            return 0;
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
        public List<Privacy_record> Seeing = new List<Privacy_record>();
        public List<Privacy_record> Unseeing = new List<Privacy_record>();
        public List<Privacy_record> Ignoring = new List<Privacy_record>();
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
                LoadContacts();
                LoadPrivacy("Seeing");
                LoadPrivacy("Unseeing");
                LoadPrivacy("Ignoring");
            }
            catch
            {
                Console.WriteLine("[{0}] Error data from SQL!", DateTime.Now);
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
    public class Privacy_record
    {
        public int Id { get; set; }
        public int Id_user { get; set; }
        public int Id_contact { get; set; }
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
