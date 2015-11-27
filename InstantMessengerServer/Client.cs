﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Authentication;
using System.IO;
using System.Threading;
using System.Data.SqlClient;

namespace InstantMessengerServer
{
    public class Client
    {
        public Client(Program p, TcpClient c)
        {
            prog = p;
            client = c;

            // Handle client in another thread.
            (new Thread(new ThreadStart(SetupConn))).Start();
        }

        Program prog;
        public TcpClient client;
        public NetworkStream netStream;  // Raw-data stream of connection.
        public SslStream ssl;            // Encrypts connection using SSL.
        public BinaryReader br;
        public BinaryWriter bw;

        User Logging = null;  // Information about current user.
        
        void SetupConn()  // Setup connection and login or register.
        {
            try
            {
                Console.WriteLine("[{0}] New connection!", DateTime.Now);
                netStream = client.GetStream();
                ssl = new SslStream(netStream, false);
                ssl.AuthenticateAsServer(prog.cert, false, SslProtocols.Tls, true);
                Console.WriteLine("[{0}] Connection authenticated!", DateTime.Now);
                // Now we have encrypted connection.

                br = new BinaryReader(ssl, Encoding.UTF8);
                bw = new BinaryWriter(ssl, Encoding.UTF8);

                // Say "hello".
                bw.Write(IM_Hello);
                bw.Flush();
                int hello = br.ReadInt32();
                if (hello == IM_Hello)
                {
                    // Hello packet is OK. Time to wait for login or register.
                    byte logMode = br.ReadByte();
                    string Login = br.ReadString();
                    //string e_mail = br.ReadString();
                    string e_mail = Login;
                    string password = br.ReadString();
                    Logging = prog.FindLogin(Login);
                    if (Login.Length < 50) // Isn't username too long?
                    {
                        if (password.Length < 50)  // Isn't password too long?
                        {
                            if (logMode == IM_Register)  // Register mode
                            {
                                if (Logging==null)  // User already exists?
                                {
                                    Logging = new User() { Id = prog.Users.Count, Pass_hash = password, Login = Login, e_mail = e_mail, Status = 11 };
                                    Logging.Birth_date = new DateTime(1900, 1, 1);
                                    Logging.First_Name = "";
                                    Logging.Last_name = "";
                                    prog.SaveUsers(Logging);
                                    prog.Users.Add(Logging);
                                    bw.Write(IM_OK);
                                    bw.Flush();
                                    Console.WriteLine("[{0}] ({1}) Registered new user", DateTime.Now, Login);
                                    Receiver();  // Listen to client in loop.
                                }
                                else
                                    bw.Write(IM_Exists);
                            }
                            else if (logMode == IM_Login)  // Login mode
                            {
                                if (Logging != null)  // User exists?
                                {
                                    if (password == Logging.Pass_hash)  // Is password OK?
                                    {
                                        // If user is logged in yet, disconnect him.
                                        if (Logging.Status!=0)
                                            Logging.Connection.CloseConn();

                                        Logging.Connection = this;
                                        bw.Write(IM_OK);
                                        bw.Flush();
                                        Receiver();  // Listen to client in loop.
                                    }
                                    else
                                        bw.Write(IM_WrongPass);
                                }
                                else
                                    bw.Write(IM_NoExists);
                            }
                        }
                        else
                            bw.Write(IM_TooPassword);
                    }
                    else
                        bw.Write(IM_TooUsername);
                }
                CloseConn();
            }
            catch { CloseConn(); }
        }
        void CloseConn() // Close connection.
        {
            try
            {
                Logging.Status = 0;
                br.Close();
                bw.Close();
                ssl.Close();
                netStream.Close();
                client.Close();
                Console.WriteLine("[{0}] End of connection!", DateTime.Now);
            }
            catch { }
        }
        void Receiver()  // Receive all incoming packets.
        {
            Console.WriteLine("[{0}] ({1}) User logged in", DateTime.Now, Logging.Login);
            Logging.Status = 11;

            try
            {
                RefreshContactList();
                while (client.Client.Connected)  // While we are connected.
                {
                    byte type = br.ReadByte();  // Get incoming packet type.

                    if (type == IM_IsAvailable)
                    {
                        string who = br.ReadString();

                        bw.Write(IM_IsAvailable);
                        bw.Write(who);

                        User info = prog.Users.Find(p => p.Login == who);
                        if (info != null)
                        {
                            if (info.Status == 1)
                                bw.Write(true);   // Available
                            else
                                bw.Write(false);  // Unavailable
                        }
                        else
                            bw.Write(false);      // Unavailable
                        bw.Flush();
                    }
                    else if (type == IM_Send)
                    {
                        string to = br.ReadString();
                        string msg = br.ReadString();

                        User recipient = prog.Users.Find(p => p.Login == to); ;
                        if (recipient != null)
                        {
                            // Is recipient logged in?
                            if (recipient.Status != 0)
                            {
                                // Write received packet to recipient
                                recipient.Connection.bw.Write(IM_Received);
                                recipient.Connection.bw.Write(Logging.Login);  // From
                                recipient.Connection.bw.Write(msg);
                                recipient.Connection.bw.Flush();
                                Console.WriteLine("[{0}] ({1} -> {2}) Message sent!", DateTime.Now, Logging.Login, recipient.Login);
                            }
                        }
                    }
                    else if (type == IM_GetProfile)
                    {
                        bw.Write(IM_SetProfile);
                        bw.Write(Logging.First_Name);
                        bw.Write(Logging.Last_name);
                        bw.Write(Logging.Birth_date.ToString());
                    }
                    else if (type == IM_SaveProfile)
                    {
                        Logging.First_Name = br.ReadString();
                        Logging.Last_name = br.ReadString();
                        Logging.Birth_date = Convert.ToDateTime(br.ReadString());
                        prog.EditUsers(Logging);
                    }
                    else if (type == IM_ChangeStatus)
                    {
                        Logging.Status=Logging.Status%10+(br.ReadInt32()+1)*10;
                        prog.EditUsers(Logging);
                    }
                    else if (type == IM_ChangePrivacy)
                    {
                        Logging.Status = Logging.Status - Logging.Status % 10 + (br.ReadInt32()+1);
                        prog.EditUsers(Logging);
                    }
                    else if (type == IM_AddcontactSearch)
                    {
                        bw.Write(IM_AddcontactResult);
                        User result = prog.FindLogin(br.ReadString());
                        if (result != null)
                        {
                            bw.Write(1);
                            bw.Write(result.Login);
                        }
                        else bw.Write(0);
                    }
                    else if (type == IM_AddcontactAdd)
                    {
                        String Login = br.ReadString();
                        Contact cont = new Contact { Id = prog.Contacts.Count, Id_user = Logging.Id, Id_contact = prog.FindLogin(Login).Id, Id_grupp = 0, Name_for_user = Login };
                        prog.SaveContacts(cont);
                        prog.Contacts.Add(cont);
                        //RefreshContactList();
                    }
                    else if (type == IM_DeleteContact)
                    {
                        prog.Contacts.Remove(prog.Contacts.Find(p => p.Name_for_user == br.ReadString()));
                        //RefreshContactList();
                    }
                }
            }
            catch (IOException) { }

            Logging.Status = 0;
            Console.WriteLine("[{0}] ({1}) User logged out", DateTime.Now, Logging.Login);
        }

        public void RefreshContactList()
        {
            List<Contact> ContactList = prog.Contacts.FindAll(p => (p.Id_user == Logging.Id));
            bw.Write(ContactList.Count);
            for (int i = 0; i < ContactList.Count; i++)
            {
                ContactList.ForEach(delegate(Contact cnt)
                {
                    bw.Write(cnt.Id);
                    bw.Write(cnt.Id_user);
                    bw.Write(cnt.Id_contact);
                    bw.Write(cnt.Id_grupp);
                    bw.Write(cnt.Name_for_user);
                    bw.Write(prog.Users.Find(p => p.Id == cnt.Id_contact).Status);
                });
            }
        }
        public const int IM_Hello = 2012;      // Hello
        public const byte IM_OK = 0;           // OK
        public const byte IM_Login = 1;        // Login
        public const byte IM_Register = 2;     // Register
        public const byte IM_TooUsername = 3;  // Too long username
        public const byte IM_TooPassword = 4;  // Too long password
        public const byte IM_Exists = 5;       // Already exists
        public const byte IM_NoExists = 6;     // Doesn't exists
        public const byte IM_WrongPass = 7;    // Wrong password
        public const byte IM_IsAvailable = 8;  // Is user available?
        public const byte IM_Send = 9;         // Send message
        public const byte IM_Received = 10;    // Message received
        public const byte IM_GetProfile = 11;  // Get profile details
        public const byte IM_SetProfile = 12;  // Set profile details
        public const byte IM_SaveProfile = 13;  // Save profile details
        public const byte IM_ChangeStatus = 14;// Change status
        public const byte IM_ChangePrivacy = 15; //Change privacy
        public const byte IM_AddcontactSearch = 16; //Search request to add new contact
        public const byte IM_AddcontactResult = 17; //Search result to add new contact
        public const byte IM_AddcontactAdd = 18; //Order to add new contact
        public const byte IM_DeleteContact = 19; //Order to delete contact
    }
}
