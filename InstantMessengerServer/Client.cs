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
                                    Logging.Connection = this;
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
                RefreshMessagesList();
                while (client.Client.Connected)  // While we are connected.
                {
                    byte type = br.ReadByte();  // Get incoming packet type.

                    if (type == IM_Send)
                    {
                        string text = br.ReadString();
                        int to = br.ReadInt32();
                        int Magic_pointer =br.ReadInt32();
                        string mess_date = br.ReadString();
                        Message mes = new Message() { Id = prog.Messages.Count, Id_from = Logging.Id, Id_whom = to, Mess_text = text, Mess_date = mess_date };
                        prog.Messages.Add(mes);
                        prog.SaveMessages(mes);
                        User recipient = prog.Users.Find(p => p.Id == to); ;
                                // Write received packet to recipient
                                recipient.Connection.bw.Write(IM_Received);
                                recipient.Connection.bw.Write(Logging.Id);  // From
                                recipient.Connection.bw.Write(text);
                                recipient.Connection.bw.Write(Magic_pointer);
                                recipient.Connection.bw.Write(mess_date);
                                recipient.Connection.bw.Flush();
                                Console.WriteLine("[{0}] ({1} -> {2}) Message sent!", DateTime.Now, Logging.Login, recipient.Login);
                    }
                    else if (type == IM_GetProfile)
                    {
                        bw.Write(IM_SetProfile);
                        bw.Write(Logging.First_Name);
                        bw.Write(Logging.Last_name);
                        bw.Write(Logging.Birth_date.ToString());
                        bw.Write(1);
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
                        int stat = br.ReadInt32();
                        if (stat != -1) Logging.Status = Logging.Status % 10 + (stat + 1) * 10;
                        else Logging.Status = 0;
                        prog.EditUsers(Logging);
                        prog.Contacts.FindAll(p => p.Id_contact == Logging.Id).ForEach(delegate(Contact cnt)
                        {
                            User usr = prog.Users.Find(p => p.Id == cnt.Id_user);
                            if (usr.Connection != null)
                            {
                                usr.Connection.bw.Write(IM_RefreshContactList);
                                usr.Connection.RefreshContactList();
                            }
                         });
                    }
                    else if (type == IM_ChangePrivacy)
                    {
                        Logging.Status = Logging.Status - Logging.Status % 10 + (br.ReadInt32()+1);
                        prog.EditUsers(Logging);
                        prog.Contacts.FindAll(p=> p.Id_contact==Logging.Id).ForEach(delegate(Contact cnt)
                        {
                            User usr = prog.Users.Find(p => p.Id == cnt.Id_user);
                            if (usr.Connection != null)
                            {
                                usr.Connection.bw.Write(IM_RefreshContactList);
                                usr.Connection.RefreshContactList();
                            }
                        });
                    }
                    else if (type == IM_AddcontactSearch)
                    {
                        bw.Write(IM_AddcontactResult);
                        String login = br.ReadString();
                        List<User>result = prog.Users.FindAll(p=> p.Login.ToLower().Contains(login.ToLower()));
                            bw.Write(result.Count);
                            result.ForEach(delegate(User usr) 
                            {
                                bw.Write(usr.Login); 
                            });
                    }
                    else if (type == IM_AddcontactAdd)
                    {
                        String Login = br.ReadString();
                        String NameForUser = br.ReadString();
                        Contact cont = new Contact { Id = prog.Contacts.Count+(new Random().Next(1000)), Id_user = Logging.Id, Id_contact = prog.FindLogin(Login).Id, Id_grupp = 0, Name_for_user = NameForUser };
                        while (prog.Contacts.Find(p => p.Id == cont.Id) != null) cont.Id = prog.Contacts.Count + (new Random().Next(1000));
                        prog.SaveContacts(cont);
                        prog.Contacts.Add(cont);
                        bw.Write(IM_RefreshContactList);
                        RefreshContactList();
                    }
                    else if (type == IM_DeleteContact)
                    {
                        String str = br.ReadString();
                        Contact cnt = prog.Contacts.Find(p => p.Name_for_user == str);
                        prog.Contacts.Remove(cnt);
                        prog.DeleteContact(cnt.Id);
                        Privacy_record prv = new Privacy_record(){Id=0, Id_user=Logging.Id,Id_contact=cnt.Id};
                        prog.Seeing.Remove(prog.Seeing.Find(p=> (p.Id_user==prv.Id_user)&&(p.Id_contact==prv.Id_contact)));
                        prog.Unseeing.Remove(prog.Seeing.Find(p => (p.Id_user == prv.Id_user) && (p.Id_contact == prv.Id_contact)));
                        prog.Ignoring.Remove(prog.Seeing.Find(p => (p.Id_user == prv.Id_user) && (p.Id_contact == prv.Id_contact)));
                        bw.Write(IM_RefreshContactList);
                        RefreshContactList();
                    }
                    else if (type == IM_GetSeeingList)
                    {
                        bw.Write(IM_SetPrivacyList);
                        bw.Write("Seeing");
                        List<Privacy_record> SeeingList = prog.Seeing.FindAll(p => p.Id_user == Logging.Id);
                        bw.Write(SeeingList.Count);
                        SeeingList.ForEach(delegate(Privacy_record prv){
                            bw.Write(prv.Id);
                            bw.Write(prv.Id_user);
                            bw.Write(prv.Id_contact);
                        });
                    }
                    else if (type == IM_GetUnseeingList)
                    {
                        bw.Write(IM_SetPrivacyList);
                        bw.Write("Unseeing");
                        List<Privacy_record> UnseeingList = prog.Unseeing.FindAll(p => p.Id_user == Logging.Id);
                        bw.Write(UnseeingList.Count);
                        UnseeingList.ForEach(delegate(Privacy_record prv)
                        {
                            bw.Write(prv.Id);
                            bw.Write(prv.Id_user);
                            bw.Write(prv.Id_contact);
                        });
                    }
                    else if (type == IM_GetIgnoringList)
                    {
                        bw.Write(IM_SetPrivacyList);
                        bw.Write("Ignoring");
                        List<Privacy_record> IgnoringList = prog.Ignoring.FindAll(p => p.Id_user == Logging.Id);
                        bw.Write(IgnoringList.Count);
                        IgnoringList.ForEach(delegate(Privacy_record prv)
                        {
                            bw.Write(prv.Id);
                            bw.Write(prv.Id_user);
                            bw.Write(prv.Id_contact);
                        });
                    }
                    else if (type == IM_AddPrivacy)
                    {
                        String privacyType = br.ReadString();
                        int id_user = br.ReadInt32();
                        Privacy_record prv = new Privacy_record(){Id=0,Id_user=Logging.Id, Id_contact=id_user};
                        switch (privacyType)
                        {
                            case "Seeing":
                                prv.Id = prog.Seeing.Count + (new Random().Next(1000));
                                while (prog.Seeing.Find(p=>p.Id==prv.Id)!=null) prv.Id=prog.Seeing.Count + (new Random().Next(1000));
                                prog.Seeing.Add(prv);
                                break;
                            case "Unseeing":
                                prv.Id = prog.Unseeing.Count + (new Random().Next(1000));
                                while (prog.Unseeing.Find(p=>p.Id==prv.Id)!=null) prv.Id=prog.Unseeing.Count + (new Random().Next(1000));
                                prog.Unseeing.Add(prv);
                                break;
                            case "Ignoring":
                                prv.Id = prog.Ignoring.Count + (new Random().Next(1000));
                                while (prog.Ignoring.Find(p => p.Id == prv.Id) != null) prv.Id = prog.Ignoring.Count + (new Random().Next(1000));
                                prog.Ignoring.Add(prv);
                                break;
                        }
                        prog.AddPrivacy(privacyType, prv);
                        prog.Contacts.FindAll(p => p.Id_contact == Logging.Id).ForEach(delegate(Contact cnt)
                        {
                            User usr = prog.Users.Find(p => p.Id == cnt.Id_user);
                            if (usr.Connection != null)
                            {
                                usr.Connection.bw.Write(IM_RefreshContactList);
                                usr.Connection.RefreshContactList();
                            }
                        });
                    }
                    else if (type == IM_DeletePrivacy)
                    {
                        String privacyType = br.ReadString();
                        int id_user = br.ReadInt32();
                        Privacy_record prv = new Privacy_record() { Id = 0, Id_user = Logging.Id, Id_contact = id_user };
                        switch (privacyType)
                        {
                            case "Seeing":
                                prv.Id = prog.Seeing.Count;
                                prog.Seeing.Remove(prog.Seeing.Find(p=> (p.Id_user==prv.Id_user) &&(p.Id_contact==prv.Id_contact)));
                                break;
                            case "Unseeing":
                                prv.Id = prog.Unseeing.Count;
                                prog.Unseeing.Remove(prog.Unseeing.Find(p => (p.Id_user == prv.Id_user) && (p.Id_contact == prv.Id_contact)));
                                break;
                            case "Ignoring":
                                prv.Id = prog.Ignoring.Count;
                                prog.Ignoring.Remove(prog.Ignoring.Find(p => (p.Id_user == prv.Id_user) && (p.Id_contact == prv.Id_contact)));
                                break;
                        }
                        prog.DeletePrivacy(privacyType, prv);
                        prog.Contacts.FindAll(p => p.Id_contact == Logging.Id).ForEach(delegate(Contact cnt)
                        {
                            User usr = prog.Users.Find(p => p.Id == cnt.Id_user);
                            if (usr.Connection != null)
                            {
                                usr.Connection.bw.Write(IM_RefreshContactList);
                                usr.Connection.RefreshContactList();
                            }
                        });
                    }
                    else if (type == IM_GetOtherProfile)
                    {
                        String profileName = br.ReadString();
                        bw.Write(IM_SetProfile);
                        User Profile = prog.Users.Find(p=> p.Id==(prog.Contacts.Find(q=> (q.Id_user==Logging.Id) && (q.Name_for_user==profileName)).Id_contact));
                        bw.Write(Profile.First_Name);
                        bw.Write(Profile.Last_name);
                        bw.Write(Profile.Birth_date.ToString());
                        bw.Write(0);
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
            ContactList.ForEach(delegate(Contact cnt)
            {
                bw.Write(cnt.Id);
                bw.Write(cnt.Id_user);
                bw.Write(cnt.Id_contact);
                bw.Write(cnt.Id_grupp);
                bw.Write(cnt.Name_for_user);
                bw.Write(prog.GetImaginaryStatus(cnt.Id_contact,cnt.Id_user));
            });
        }
        public void RefreshMessagesList()
        {
            List<Message> MessageList_from = prog.Messages.FindAll(p => (p.Id_from == Logging.Id));
            List<Message> MessageList_whom = prog.Messages.FindAll(p => (p.Id_whom == Logging.Id));
            bw.Write(MessageList_from.Count+MessageList_whom.Count);
            MessageList_from.ForEach(delegate(Message mes)
            {
                bw.Write(mes.Id);
                bw.Write(mes.Id_from);
                bw.Write(mes.Id_whom);
                bw.Write(mes.Magic_pointer);
                bw.Write(mes.Mess_text);
                bw.Write(mes.Mess_date);
            });
            MessageList_whom.ForEach(delegate(Message mes)
            {
                bw.Write(mes.Id);
                bw.Write(mes.Id_from);
                bw.Write(mes.Id_whom);
                bw.Write(mes.Magic_pointer);
                bw.Write(mes.Mess_text);
                bw.Write(mes.Mess_date);
            });
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
        public const byte IM_RefreshContactList = 20; //Order to refresh contact list
        public const byte IM_GetSeeingList = 21; //Get list of Seeing
        public const byte IM_SetPrivacyList = 22;//Set list of Seeing
        public const byte IM_AddPrivacy = 23;//Add entry to one of privacy lists
        public const byte IM_DeletePrivacy = 24;//Delete entry from one of privacy lists
        public const byte IM_GetUnseeingList = 25;//Get list of Unseeing
        public const byte IM_GetIgnoringList = 26;//Get list of Ignoring
        public const byte IM_GetOtherProfile = 27;//Get profile of other user
    }
}
