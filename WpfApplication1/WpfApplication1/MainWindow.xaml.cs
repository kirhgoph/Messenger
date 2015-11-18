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
            im.RegisterFailed += new IMErrorEventHandler(im_RegisterFailed);
            im.Disconnected += new EventHandler(im_Disconnected);
            im.ProfileReceived += new ProfileReceivedEventHandler(im_ProfileReceived);
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
                    btn_LogIn.IsEnabled = false;
                    btn_LogIn.Visibility = System.Windows.Visibility.Hidden;
                    btn_Registration.IsEnabled = false;
                    btn_LogOut.IsEnabled = true;
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
            MessageBox.Show("Login failed!");
        }
        void im_RegisterFailed(object sender, IMErrorEventArgs e)
        {
                MessageBox.Show("Register failed!");
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
        void im_Disconnected(object sender, EventArgs e)
        {
            if (btn_Registration.IsEnabled==false)
            { 
                MessageBox.Show ("Disconnected!");
                Dispatcher.BeginInvoke(new ThreadStart(delegate
                {
                    btn_Registration.IsEnabled = true;
                    btn_LogIn.IsEnabled = true;
                    btn_LogIn.Visibility = System.Windows.Visibility.Visible;
                    btn_LogOut.IsEnabled = false;
                    btn_LogOut.Visibility = System.Windows.Visibility.Hidden;
                    btn_ContactAdd.IsEnabled = false;
                    btn_ContactDelete.IsEnabled = false;
                    btn_Profile.IsEnabled = false;
                    btn_Seeing.IsEnabled = false;
                    btn_Unseeing.IsEnabled = false;
                    btn_Ignored.IsEnabled = false;
                    cBox_Status.IsEnabled = false;
                    cBox_Privacy.IsEnabled = false;
                    cBox_Privacy.SelectedIndex = -1;
                    cBox_Status.SelectedIndex = -1;
                }));
                }
        }
        void im_ProfileReceived(object sender, ProfileReceivedEventArgs e)
        {
            Dispatcher.BeginInvoke(new ThreadStart(delegate
            {
                Profile prof = new Profile();
                prof.dpckr_Profile_BirthDate.Text = e.BirthDate;
                prof.tbx_Profile_FirstName.Text = e.FirstName;
                prof.tbx_Profile_LastName.Text = e.LastName;
                prof.ProfileSave += new ProfileReceivedEventHandler(im_ProfileSave);
                prof.Show();
            }));
        }
        void im_ProfileSave(object sender, ProfileReceivedEventArgs e)
        {
            im.SaveProfile(e);
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

        private void btn_LogOut_Click(object sender, RoutedEventArgs e)
        {
            im.Disconnect();
        }

        private void cBox_Status_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            im.ChangeStatus(cBox_Status.SelectedIndex);
        }

        private void cBox_Privacy_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            im.ChangePrivacy(cBox_Privacy.SelectedIndex);
        }

    }
}