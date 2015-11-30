using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Security.Cryptography;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace InstantMessenger
{
    public class IMClient
    {
        public MainWindow ParentWindow;
        Thread tcpThread;      // Receiver
        bool _conn = false;    // Is connected/connecting?
        bool _logged = false;  // Is logged in?
        string _user;          // Username
        string _pass;          // Password
        bool reg;              // Register mode

        public string Server { get { return "localhost"; } }  // Address of server. In this case - local IP address.
        public int Port { get { return 2000; } }

        public List<Contact> ContactList = new List<Contact>();
        public List<Privacy_record> SeeingList = new List<Privacy_record>();
        public List<Privacy_record> UnseeingList = new List<Privacy_record>();
        public List<Privacy_record> IgnoringList = new List<Privacy_record>();
        public bool IsLoggedIn { get { return _logged; } }
        public string UserName { get { return _user; } }
        public string Password { get { return _pass; } }

        // Start connection thread and login or register.
        void connect(string user, string password, bool register)
        {
            if (!_conn)
            {
                _conn = true;
                _user = user;
                _pass = password;
                reg = register;
                tcpThread = new Thread(new ThreadStart(SetupConn));
                tcpThread.Start();
            }
        }
        public void Login(string user, string password)
        {
            connect(user, password, false);
        }
        public void Register(string user, string password)
        {
            connect(user, password, true);
        }
        public void Disconnect()
        {
            if (_conn)
                CloseConn();
        }
        public void GetProfile()
        {
             bw.Write(IM_GetProfile);
        }
        public void DeleteContact(string str)
        {
            bw.Write(IM_DeleteContact);
            bw.Write(str);
        }
        public void ChangeStatus(int stat)
        {
            bw.Write(IM_ChangeStatus);
            bw.Write(stat);
        }
        public void ChangePrivacy(int priv)
        {
            bw.Write(IM_ChangePrivacy);
            bw.Write(priv);
        }
        public void AddPrivacy(string PrivacyType,int Id_user)
        {
            bw.Write(IM_AddPrivacy);
            bw.Write(PrivacyType);
            bw.Write(Id_user);
        }
        public void DeletePrivacy(string PrivacyType, int Id_user)
        {
            bw.Write(IM_DeletePrivacy);
            bw.Write(PrivacyType);
            bw.Write(Id_user);
        }
        public void GetSeeingList()
        {
            bw.Write(IM_GetSeeingList);
        }
        public void GetUnseeingList()
        {
            bw.Write(IM_GetUnseeingList);
        }
        public void GetIgnoringList()
        {
            bw.Write(IM_GetIgnoringList);
        }

        public void SaveProfile(ProfileReceivedEventArgs e)
        {
            bw.Write(IM_SaveProfile);
            bw.Write(e.FirstName);
            bw.Write(e.LastName);
            bw.Write(e.BirthDate);
        }
        public void Addcontact_Search(AddContactSearchEventArgs e)
        {
            bw.Write(IM_AddcontactSearch);
            bw.Write(e.SearchString);
        }
        public void Addcontact_Add(AddContactAddEventArgs e)
        {
            bw.Write(IM_AddcontactAdd);
            bw.Write(e.Login);
        }
   


        public event EventHandler LoginOK;
        public event EventHandler ConnectionFailed;
        public event EventHandler RegisterOK;
        public event EventHandler Disconnected;
        public event IMErrorEventHandler LoginFailed;
        public event IMErrorEventHandler RegisterFailed;
        public event IMReceivedEventHandler MessageReceived;
        public event ProfileReceivedEventHandler ProfileReceived;
        public event AddContactResultEventHandler AddcontactResult;
        public event PrivacyListReceivedEventHandler PrivacyListReceived;

        virtual protected void OnRegisterOK()
        {
            if (RegisterOK != null)
                RegisterOK(this, EventArgs.Empty);
        }
        virtual protected void OnConnectionFailed()
        {
            if (ConnectionFailed != null)
                ConnectionFailed(this, EventArgs.Empty);
        }
        virtual protected void OnLoginOK()
        {
            if (LoginOK != null)
                LoginOK(this, EventArgs.Empty);
        }
        virtual protected void OnRegisterFailed(IMErrorEventArgs e)
        {
            if (RegisterFailed != null)
                RegisterFailed(this, e);
        }
        virtual protected void OnDisconnected()
        {
            if (Disconnected != null)
                Disconnected(this, EventArgs.Empty);
        }
        virtual protected void OnLoginFailed(IMErrorEventArgs e)
        {
            if (LoginFailed != null)
                LoginFailed(this, e);
        }
        virtual protected void OnMessageReceived(IMReceivedEventArgs e)
        {
            if (MessageReceived != null)
                MessageReceived(this, e);
        }
        virtual protected void OnProfileReceived(ProfileReceivedEventArgs e)
        {
            if (ProfileReceived != null)
                ProfileReceived(this, e);
        }
        public virtual void OnAddcontactResult(AddContactResultEventArgs e)
        {
            if (AddcontactResult != null)
                AddcontactResult(this, e);
        }
        public virtual void OnPrivacyListReceived(PrivacyListReceivedEventArgs e)
        {
            if (PrivacyListReceived != null)
                PrivacyListReceived(this, e);
        }
        

        TcpClient client;
        NetworkStream netStream;
        SslStream ssl;
        BinaryReader br;
        BinaryWriter bw;

        void RefreshContactList()
        {
            ContactList = new List<Contact>();
            int SizeOfContactList = br.ReadInt32();
            for (int i = 0; i < SizeOfContactList; i++)
            {
                Contact cnt = new Contact { Id = br.ReadInt32(), Id_user = br.ReadInt32(), Id_contact = br.ReadInt32(), Id_grupp = br.ReadInt32(), Name_for_user = br.ReadString(), status = br.ReadInt32() };
                ContactList.Add(cnt);
            }
            ParentWindow.RefreshTreeView(ContactList);
        }
        void SetupConn()  // Setup connection and login
        
        {
            try 
            {
                client = new TcpClient(Server, Port);  // Connect to the server.
            }
            catch
            {
                OnConnectionFailed();
                CloseConn();
                return;
            }
            try{
            netStream = client.GetStream();
            ssl = new SslStream(netStream, false, new RemoteCertificateValidationCallback(ValidateCert));
            ssl.AuthenticateAsClient("InstantMessengerServer");
            // Now we have encrypted connection.

            br = new BinaryReader(ssl, Encoding.UTF8);
            bw = new BinaryWriter(ssl, Encoding.UTF8);

            // Receive "hello"
            int hello = br.ReadInt32();
            if (hello == IM_Hello)
            {
                // Hello OK, so answer.
                bw.Write(IM_Hello);

                bw.Write(reg ? IM_Register : IM_Login);  // Login or register
                bw.Write(UserName);
                bw.Write(Password);
                bw.Flush();

                byte ans = br.ReadByte();  // Read answer.
                if (ans == IM_OK)  // Login/register OK
                {
                    if (reg)
                        OnRegisterOK();  // Register is OK.
                    OnLoginOK();  // Login is OK (when registered, automatically logged in)
                    Receiver(); // Time for listening for incoming messages.
                }
                else
                {
                    IMErrorEventArgs err = new IMErrorEventArgs((IMError)ans);
                    if (reg)
                        OnRegisterFailed(err);
                    else
                        OnLoginFailed(err);
                }
            }
            if (_conn)
                CloseConn();
            }
            catch { CloseConn(); }
        }
        void CloseConn() // Close connection.
        {
            try
            {
                br.Close();
            }
            catch { }
            try
            {
            ssl.Close();
            }
            catch { }
            try
            {
                netStream.Close();
            }
            catch { }
            try
            {
            client.Close();
            }
            catch { }
            try
            {
            OnDisconnected();
            }
            catch { }
            try
            {
            _conn = false;
                            }
            catch { }
        }
        void Receiver()  // Receive all incoming packets.
        {
            _logged = true;

            try
            {
                    RefreshContactList();
                    while (client.Connected)  // While we are connected.
                    {
                        byte type = br.ReadByte();  // Get incoming packet type.

                        //if (type == IM_IsAvailable)
                        //{
                        //    string user = br.ReadString();
                        //    bool isAvail = br.ReadBoolean();
                        //    OnUserAvail(new IMAvailEventArgs(user, isAvail));
                        //}
                        if (type == IM_Received)
                        {
                            string from = br.ReadString();
                            string msg = br.ReadString();
                            OnMessageReceived(new IMReceivedEventArgs(from, msg));
                        }
                        if (type == IM_SetProfile)
                        {
                            String FirstName = br.ReadString();
                            String LastName = br.ReadString();
                            String BirthDate = br.ReadString();
                            OnProfileReceived(new ProfileReceivedEventArgs(FirstName, LastName, BirthDate));
                        }
                        if (type == IM_AddcontactResult)
                        {
                            if (br.ReadInt32() != 0)
                            {
                                OnAddcontactResult(new AddContactResultEventArgs(br.ReadString()));
                            }
                            //OnAddcontactResult(new AddContactResultEventArgs(null));
                        }
                        if (type == IM_RefreshContactList)
                        {
                            RefreshContactList();
                        }
                        if (type == IM_SetPrivacyList)
                        {
                            string PrivacyListType = br.ReadString();
                            int PrivacyListCount = br.ReadInt32();
                            switch(PrivacyListType)
                            { 
                                case "Seeing":
                                    SeeingList.Clear();
                                    for (int i = 0; i < PrivacyListCount; i++)
                                      {
                                      SeeingList.Add(new Privacy_record { Id = br.ReadInt32(), Id_user = br.ReadInt32(), Id_contact = br.ReadInt32() });
                                      }
                                    OnPrivacyListReceived(new PrivacyListReceivedEventArgs(PrivacyListType, SeeingList));
                                    break;
                                case "Unseeing":
                                    UnseeingList.Clear();
                                    for (int i = 0; i < PrivacyListCount; i++)
                                    {
                                        UnseeingList.Add(new Privacy_record { Id = br.ReadInt32(), Id_user = br.ReadInt32(), Id_contact = br.ReadInt32() });
                                    }
                                    OnPrivacyListReceived(new PrivacyListReceivedEventArgs(PrivacyListType, UnseeingList));
                                    break;
                                case "Ignoring":
                                    IgnoringList.Clear();
                                    for (int i = 0; i < PrivacyListCount; i++)
                                    {
                                        IgnoringList.Add(new Privacy_record { Id = br.ReadInt32(), Id_user = br.ReadInt32(), Id_contact = br.ReadInt32() });
                                    }
                                    OnPrivacyListReceived(new PrivacyListReceivedEventArgs(PrivacyListType, IgnoringList));
                                    break;

                            }

                        }
                    }
            }
            
            catch (IOException) { }

            _logged = false;
        }

        // Packet types
        public const int IM_Hello = 2012;      // Hello
        public const byte IM_OK = 0;           // OK
        public const byte IM_Login = 1;        // Login
        public const byte IM_Register = 2;     // Register
        public const byte IM_TooUsername = 3;  // Too long username
        public const byte IM_TooPassword = 4;  // Too long password
        public const byte IM_Exists = 5;       // Already exists
        public const byte IM_NoExists = 6;     // Doesn't exist
        public const byte IM_WrongPass = 7;    // Wrong password
        public const byte IM_IsAvailable = 8;  // Is user available?
        public const byte IM_Send = 9;         // Send message
        public const byte IM_Received = 10;    // Message received
        public const byte IM_GetProfile = 11;  // Get profile details
        public const byte IM_SetProfile = 12;  // Set profile details
        public const byte IM_SaveProfile = 13; // Save profile details
        public const byte IM_ChangeStatus = 14;// Change status
        public const byte IM_ChangePrivacy = 15; //Change privacy
        public const byte IM_AddcontactSearch = 16; //Search request to add new contact
        public const byte IM_AddcontactResult = 17; //Search result to add new contact
        public const byte IM_AddcontactAdd = 18; //Order to add new contact
        public const byte IM_DeleteContact = 19; //Order to delete contact
        public const byte IM_RefreshContactList = 20; //Order to refresh contact list
        public const byte IM_GetSeeingList = 21; //Get list of Seeing
        public const byte IM_SetPrivacyList = 22;//Set list of Seeing
        public const byte IM_AddPrivacy = 23;//Add entry to one of privacy lists
        public const byte IM_DeletePrivacy = 24;//Delete entry from one of privacy lists
        public const byte IM_GetUnseeingList = 25;//Get list of Unseeing
        public const byte IM_GetIgnoringList = 26;//Get list of Ignoring

        public static bool ValidateCert(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            //if (sslPolicyErrors == SslPolicyErrors.None)
            //    return true;
            //else
            //    return false;

            return true; // Allow untrusted certificates.
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
    }
    public class Contact
    {
        public int Id { get; set; }
        public int Id_user { get; set; }
        public int Id_contact { get; set; }
        public int Id_grupp { get; set; }
        public string Name_for_user { get; set; }
        public int status { get; set; }
    }
    public class Group
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
