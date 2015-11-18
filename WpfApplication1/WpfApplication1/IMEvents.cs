using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstantMessenger
{
    public enum IMError : byte
    {
        TooUserName = IMClient.IM_TooUsername,
        TooPassword = IMClient.IM_TooPassword,
        Exists = IMClient.IM_Exists,
        NoExists = IMClient.IM_NoExists,
        WrongPassword = IMClient.IM_WrongPass
    }
    public class IMErrorEventArgs : EventArgs
    {
        IMError err;
        public IMErrorEventArgs(IMError error)
        {
            this.err = error;
        }
    }
    public class IMReceivedEventArgs : EventArgs
    {
        string user;
        string msg;

        public IMReceivedEventArgs(string user, string msg)
        {
            this.user = user;
            this.msg = msg;
        }

        public string From
        {
            get { return user; }
        }
        public string Message
        {
            get { return msg; }
        }
    }
    public class AddContactSearchEventArgs : EventArgs
    {
        public string SearchString { get; set; }

        public AddContactSearchEventArgs(string SearchString)
        {
            this.SearchString = SearchString;
        }
    }
    public class AddContactResultEventArgs : EventArgs
    {
        public string result { get; set; }

        public AddContactResultEventArgs(string result)
        {
            this.result = result;
        }
    }
    public class ProfileReceivedEventArgs : EventArgs
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string BirthDate { get; set; }

            public ProfileReceivedEventArgs(string FirstName, string LastName,string BirthDate)
        {
            this.FirstName = FirstName;
            this.LastName = LastName;
            this.BirthDate= BirthDate;
        }
    }
    
    public delegate void IMErrorEventHandler(object sender, IMErrorEventArgs e);

    public delegate void IMReceivedEventHandler(object sender, IMReceivedEventArgs e);
    public delegate void ProfileReceivedEventHandler(object sender, ProfileReceivedEventArgs e);
    public delegate void AddContactSearchEventHandler(object sender, AddContactSearchEventArgs e);
    public delegate void AddContactResultEventHandler(object sender, AddContactResultEventArgs e);
}
