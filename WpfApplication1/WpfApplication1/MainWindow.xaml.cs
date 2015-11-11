using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace InstantMessenger
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        IMClient im = new IMClient();
        public MainWindow()
        {
            InitializeComponent();
            // InstantMessenger Events
            im.LoginOK += new EventHandler(im_LoginOK);
            im.RegisterOK += new EventHandler(im_RegisterOK);
            im.ConnectionFailed += new EventHandler(im_ConnectionFailed);
            im.LoginFailed += new IMErrorEventHandler(im_LoginFailed);
            //im.RegisterFailed += new IMErrorEventHandler(im_RegisterFailed);
            //im.Disconnected += new EventHandler(im_Disconnected);
        }
        void im_LoginOK(object sender, EventArgs e)
        {
                //status.Text = "Logged in!";
                //registerButton.Enabled = false;
                //loginButton.Enabled = false;
                //logoutButton.Enabled = true;
                //talkButton.Enabled = true;
            MessageBox.Show("Logged!");

            Dispatcher.BeginInvoke(new ThreadStart(delegate 
                {
                    btn_LogIn.Visibility = System.Windows.Visibility.Hidden;
                    btn_LogOut.Visibility = System.Windows.Visibility.Visible;
                    btn_ContactAdd.IsEnabled = true;
                    btn_ContactDelete.IsEnabled = true;
                    btn_Profile.IsEnabled = true;
                    btn_Seeing.IsEnabled = true;
                    btn_Unseeing.IsEnabled = true;
                    btn_Ignored.IsEnabled = true;
                    cBox_Status.IsEnabled = true;
                    cBox_Privacy.IsEnabled = true;
                    cBox_Privacy.SelectedIndex = 0;
                    cBox_Status.SelectedIndex = 0;
                }));
        }
        void im_ConnectionFailed(object sender, EventArgs e)
        {
            //status.Text = "Registered!";
            //registerButton.Enabled = false;
            //loginButton.Enabled = false;
            //logoutButton.Enabled = true;
            //talkButton.Enabled = true;
            MessageBox.Show("Connection Failed!");
        }
        void im_LoginFailed(object sender, EventArgs e)
        {
            //status.Text = "Registered!";
            //registerButton.Enabled = false;
            //loginButton.Enabled = false;
            //logoutButton.Enabled = true;
            //talkButton.Enabled = true;
            MessageBox.Show("Log In failed!");
        }
        void im_RegisterOK(object sender, EventArgs e)
        {
                //status.Text = "Registered!";
                //registerButton.Enabled = false;
                //loginButton.Enabled = false;
                //logoutButton.Enabled = true;
                //talkButton.Enabled = true;
            MessageBox.Show("Registered!");
        }

        private void Registration_Click(object sender, RoutedEventArgs e)
        {
            im.Register(txt_Login.Text, txt_Password.Text);
        }

        private void LogIn_Click(object sender, RoutedEventArgs e)
        {
            if ((txt_Password.Text!="") && (txt_Login.Text!=""))
            {
            im.Login(txt_Login.Text, txt_Password.Text);
            }
            else
                MessageBox.Show("Enter login and password!");
        }

        private void Profile_Click(object sender, RoutedEventArgs e)
        {
            im.GetProfile();
        }

    }
}