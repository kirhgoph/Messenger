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
        public int user;
        public string msg;
        public int Magic_pointer;
        public string mess_date;

        public IMReceivedEventArgs(int user, string msg, int Magic_pointer,string mess_date)
        {
            this.user = user;
            this.msg = msg;
            this.Magic_pointer = Magic_pointer;
            this.mess_date = mess_date;
        }
    }
    public class AddContactAddEventArgs : EventArgs
    {
        public string Login { get; set; }
        public string NameForUser { get; set; }

        public AddContactAddEventArgs(string Login, string NameForUser)
        {
            this.Login = Login;
            this.NameForUser = NameForUser;
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
        public List<String> result { get; set; }

        public AddContactResultEventArgs(List<String> result)
        {
            this.result = result;
        }
    }
    public class ProfileReceivedEventArgs : EventArgs
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string BirthDate { get; set; }
        public int Own { get; set; }

            public ProfileReceivedEventArgs(string FirstName, string LastName,string BirthDate, int Own)
        {
            this.FirstName = FirstName;
            this.LastName = LastName;
            this.BirthDate= BirthDate;
            this.Own = Own;
        }
    }
    public class PrivacyListReceivedEventArgs : EventArgs
    {
        public String type { get; set; }//type of privacy list 
        public List<Privacy_record> PrivacyList{ get; set; }
        public PrivacyListReceivedEventArgs(String type,List<Privacy_record> PrivacyList)
        {
            this.type = type;
            this.PrivacyList = PrivacyList;
        }
    }
    
    public delegate void IMErrorEventHandler(object sender, IMErrorEventArgs e);
    public delegate void IMReceivedEventHandler(object sender, IMReceivedEventArgs e);
    public delegate void ProfileReceivedEventHandler(object sender, ProfileReceivedEventArgs e);
    public delegate void AddContactAddEventHandler(object sender, AddContactAddEventArgs e);
    public delegate void AddContactSearchEventHandler(object sender, AddContactSearchEventArgs e);
    public delegate void AddContactResultEventHandler(object sender, AddContactResultEventArgs e);
    public delegate void PrivacyListReceivedEventHandler(object sender, PrivacyListReceivedEventArgs e);
}
