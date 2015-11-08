﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApplication1
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
    
    public delegate void IMErrorEventHandler(object sender, IMErrorEventArgs e);

    public delegate void IMReceivedEventHandler(object sender, IMReceivedEventArgs e);
}
