using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace WpfApplication1
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
            //im.LoginFailed += new IMErrorEventHandler(im_LoginFailed);
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
            MessageBox.Show("Registering!");
        }

        private void LogIn_Click_1(object sender, RoutedEventArgs e)
        {

        }
    }
}
