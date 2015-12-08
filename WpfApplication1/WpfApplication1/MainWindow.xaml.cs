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
using System.Runtime.InteropServices;

public class FlashWindowHelper
{
    private IntPtr mainWindowHWnd;
    private Application theApp;

    public FlashWindowHelper(Application app)
    {
        this.theApp = app;
    }

    public void FlashApplicationWindow()
    {
        InitializeHandle();
        Flash(this.mainWindowHWnd, 5);
    }

    public void StopFlashing()
    {
        InitializeHandle();

        if (Win2000OrLater)
        {
            FLASHWINFO fi = CreateFlashInfoStruct(this.mainWindowHWnd, FLASHW_STOP, uint.MaxValue, 0);
            FlashWindowEx(ref fi);
        }
    }

    private void InitializeHandle()
    {
        if (this.mainWindowHWnd == IntPtr.Zero)
        {
            // Delayed creation of Main Window IntPtr as Application.Current passed in to ctor does not have the MainWindow set at that time
            var mainWindow = this.theApp.MainWindow;
            this.mainWindowHWnd = new System.Windows.Interop.WindowInteropHelper(mainWindow).Handle;
        }
    }

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool FlashWindowEx(ref FLASHWINFO pwfi);

    [StructLayout(LayoutKind.Sequential)]
    private struct FLASHWINFO
    {
        /// <summary>
        /// The size of the structure in bytes.
        /// </summary>
        public uint cbSize;
        /// <summary>
        /// A Handle to the Window to be Flashed. The window can be either opened or minimized.
        /// </summary>
        public IntPtr hwnd;
        /// <summary>
        /// The Flash Status.
        /// </summary>
        public uint dwFlags;
        /// <summary>
        /// The number of times to Flash the window.
        /// </summary>
        public uint uCount;
        /// <summary>
        /// The rate at which the Window is to be flashed, in milliseconds. If Zero, the function uses the default cursor blink rate.
        /// </summary>
        public uint dwTimeout;
    }

    /// <summary>
    /// Stop flashing. The system restores the window to its original stae.
    /// </summary>
    public const uint FLASHW_STOP = 0;

    /// <summary>
    /// Flash the window caption.
    /// </summary>
    public const uint FLASHW_CAPTION = 1;

    /// <summary>
    /// Flash the taskbar button.
    /// </summary>
    public const uint FLASHW_TRAY = 2;

    /// <summary>
    /// Flash both the window caption and taskbar button.
    /// This is equivalent to setting the FLASHW_CAPTION | FLASHW_TRAY flags.
    /// </summary>
    public const uint FLASHW_ALL = 3;

    /// <summary>
    /// Flash continuously, until the FLASHW_STOP flag is set.
    /// </summary>
    public const uint FLASHW_TIMER = 4;

    /// <summary>
    /// Flash continuously until the window comes to the foreground.
    /// </summary>
    public const uint FLASHW_TIMERNOFG = 12;

    /// <summary>
    /// Flash the spacified Window (Form) until it recieves focus.
    /// </summary>
    /// <param name="hwnd"></param>
    /// <returns></returns>
    public static bool Flash(IntPtr hwnd)
    {
        // Make sure we're running under Windows 2000 or later
        if (Win2000OrLater)
        {
            FLASHWINFO fi = CreateFlashInfoStruct(hwnd, FLASHW_ALL | FLASHW_TIMERNOFG, uint.MaxValue, 0);

            return FlashWindowEx(ref fi);
        }
        return false;
    }

    private static FLASHWINFO CreateFlashInfoStruct(IntPtr handle, uint flags, uint count, uint timeout)
    {
        FLASHWINFO fi = new FLASHWINFO();
        fi.cbSize = Convert.ToUInt32(Marshal.SizeOf(fi));
        fi.hwnd = handle;
        fi.dwFlags = flags;
        fi.uCount = count;
        fi.dwTimeout = timeout;
        return fi;
    }

    /// <summary>
    /// Flash the specified Window (form) for the specified number of times
    /// </summary>
    /// <param name="hwnd">The handle of the Window to Flash.</param>
    /// <param name="count">The number of times to Flash.</param>
    /// <returns></returns>
    public static bool Flash(IntPtr hwnd, uint count)
    {
        if (Win2000OrLater)
        {
            FLASHWINFO fi = CreateFlashInfoStruct(hwnd, FLASHW_ALL | FLASHW_TIMERNOFG, count, 0);

            return FlashWindowEx(ref fi);
        }

        return false;
    }

    /// <summary>
    /// A boolean value indicating whether the application is running on Windows 2000 or later.
    /// </summary>
    private static bool Win2000OrLater
    {
        get { return Environment.OSVersion.Version.Major >= 5; }
    }
}

namespace InstantMessenger
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    
    public partial class MainWindow : Window
    {
        FlashWindowHelper helper = new FlashWindowHelper(Application.Current);

        public IMClient im = new IMClient();
        public MainWindow()
        {
            InitializeComponent();
            im.ParentWindow = this;
            // InstantMessenger Events
            im.LoginOK += new EventHandler(im_LoginOK);
            im.RegisterOK += new EventHandler(im_RegisterOK);
            im.ConnectionFailed += new EventHandler(im_ConnectionFailed);
            im.LoginFailed += new IMErrorEventHandler(im_LoginFailed);
            im.RegisterFailed += new IMErrorEventHandler(im_RegisterFailed);
            im.Disconnected += new EventHandler(im_Disconnected);
            im.ProfileReceived += new ProfileReceivedEventHandler(im_ProfileReceived);
            im.PrivacyListReceived += new PrivacyListReceivedEventHandler(im_PrivacyListReceived);
            im.MessageReceived += new IMReceivedEventHandler(im_MessageReceived);
        }
        void im_AddcontactAdd(object sender, AddContactAddEventArgs e)
        {
            im.Addcontact_Add(e);
        }
        void im_AddcontactSearch(object sender, AddContactSearchEventArgs e)
        {
            im.Addcontact_Search(e);
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
        void im_AddcontactResult(object sender, AddContactResultEventArgs e)
        {
            
            Dispatcher.BeginInvoke(new ThreadStart(delegate
            {
                Profile prof = new Profile();
                //prof.dpckr_Profile_BirthDate.Text = e.BirthDate;
                //prof.tbx_Profile_FirstName.Text = e.FirstName;
                //prof.tbx_Profile_LastName.Text = e.LastName;
                //prof.ProfileSave += new ProfileReceivedEventHandler(im_ProfileSave);
                prof.Show();
            }));
        }
        void im_ProfileReceived(object sender, ProfileReceivedEventArgs e)
        {
            Dispatcher.BeginInvoke(new ThreadStart(delegate
            {
                Profile prof = new Profile();
                if (e.Own == 0) prof.btn_SaveProfile.IsEnabled = false;
                prof.dpckr_Profile_BirthDate.Text = e.BirthDate;
                prof.tbx_Profile_FirstName.Text = e.FirstName;
                prof.tbx_Profile_LastName.Text = e.LastName;
                prof.ProfileSave += new ProfileReceivedEventHandler(im_ProfileSave);
                prof.Show();
            }));
        }
        void im_PrivacyListReceived(object sender, PrivacyListReceivedEventArgs e)
        {
            switch(e.type)
            {
                case "Seeing":
                    Dispatcher.BeginInvoke(new ThreadStart(delegate
                    {
                        
                        Seeing seeing = new Seeing();
                        seeing.lbx_SeeingList.Items.Clear();
                        seeing.cbx_AddSeeing.Items.Clear();
                        seeing.cbx_DeleteSeeing.Items.Clear();
                        e.PrivacyList.ForEach(delegate(Privacy_record prv)
                        {
                            seeing.lbx_SeeingList.Items.Add(im.ContactList.Find(p => p.Id_contact == prv.Id_contact).Name_for_user);
                            seeing.cbx_DeleteSeeing.Items.Add(im.ContactList.Find(p => p.Id_contact == prv.Id_contact).Name_for_user);
                        });
                        im.ContactList.ForEach(delegate(Contact usr)
                        {
                            if (e.PrivacyList.Find(p=> p.Id_contact==usr.Id_contact)==null)
                            {
                                seeing.cbx_AddSeeing.Items.Add(usr.Name_for_user);
                            }
                        });
                        seeing.ParentWindow = this;
                        seeing.Show();
                    }));
                    break;
                case "Unseeing":
                    Dispatcher.BeginInvoke(new ThreadStart(delegate
                    {

                        Unseeing unseeing = new Unseeing();
                        unseeing.lbx_UnseeingList.Items.Clear();
                        unseeing.cbx_AddUnseeing.Items.Clear();
                        unseeing.cbx_DeleteUnseeing.Items.Clear();
                        e.PrivacyList.ForEach(delegate(Privacy_record prv)
                        {
                            unseeing.lbx_UnseeingList.Items.Add(im.ContactList.Find(p => p.Id_contact == prv.Id_contact).Name_for_user);
                            unseeing.cbx_DeleteUnseeing.Items.Add(im.ContactList.Find(p => p.Id_contact == prv.Id_contact).Name_for_user);
                        });
                        im.ContactList.ForEach(delegate(Contact usr)
                        {
                            if (e.PrivacyList.Find(p => p.Id_contact == usr.Id_contact) == null)
                            {
                                unseeing.cbx_AddUnseeing.Items.Add(usr.Name_for_user);
                            }
                        });
                        unseeing.ParentWindow = this;
                        unseeing.Show();
                    }));
                    break;
                case "Ignoring":
                    Dispatcher.BeginInvoke(new ThreadStart(delegate
                    {

                        Ignoring ignoring = new Ignoring();
                        ignoring.lbx_IgnoringList.Items.Clear();
                        ignoring.cbx_AddIgnoring.Items.Clear();
                        ignoring.cbx_DeleteIgnoring.Items.Clear();
                        e.PrivacyList.ForEach(delegate(Privacy_record prv)
                        {
                            ignoring.lbx_IgnoringList.Items.Add(im.ContactList.Find(p => p.Id_contact == prv.Id_contact).Name_for_user);
                            ignoring.cbx_DeleteIgnoring.Items.Add(im.ContactList.Find(p => p.Id_contact == prv.Id_contact).Name_for_user);
                        });
                        im.ContactList.ForEach(delegate(Contact usr)
                        {
                            if (e.PrivacyList.Find(p => p.Id_contact == usr.Id_contact) == null)
                            {
                                ignoring.cbx_AddIgnoring.Items.Add(usr.Name_for_user);
                            }
                        });
                        ignoring.ParentWindow = this;
                        ignoring.Show();
                    }));
                    break;
            }
        }
        void im_MessageReceived(object sender, IMReceivedEventArgs e)
        {
            Message mes = new Message(){Id=-1,Id_from=e.user,Id_whom=im.ContactList.Find(p=> p.Id>-1).Id_user,Mess_text=e.msg,Magic_pointer=e.Magic_pointer,Mess_date=e.mess_date};
            im.Messages.Add(mes);
            Dispatcher.BeginInvoke(new ThreadStart(delegate
            {
                String name_of_recipient = im.ContactList.Find(p => p.Id_contact == mes.Id_from).Name_for_user;
                if ((trv_ContactList.SelectedItem == null) || (trv_ContactList.SelectedItem.ToString() != name_of_recipient))
                {
                    im.UnreadMessages.Add(name_of_recipient);
                    if (im.UnreadMessages.Count != 0) helper.FlashApplicationWindow();
                    MessageBox.Show("New message from " + name_of_recipient);
                }
            }));
            refreshHistory();
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
            txt_Login.Text = "";
            txt_Password.Text = "";
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
            if (im._conn==true)
            im.ChangeStatus(cBox_Status.SelectedIndex);
        }
        private void cBox_Privacy_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (im._conn == true)
            im.ChangePrivacy(cBox_Privacy.SelectedIndex);
        }
        private void btn_ContactAdd_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(new ThreadStart(delegate
            {
                AddContact AddCont = new AddContact();
                AddCont.ParentWindow = this;
                AddCont.registerHandler();
                AddCont.AddcontactSearch += new AddContactSearchEventHandler(im_AddcontactSearch);
                AddCont.AddcontactAdd += new AddContactAddEventHandler(im_AddcontactAdd);
                AddCont.lbl_NameForUser.Content = "";
                AddCont.Show();
            }));
        }

        private void btn_ContactDelete_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(new ThreadStart(delegate
            {
                DeleteContact DelCont = new DeleteContact();
                DelCont.ParentWindow = this;
                im.ContactList.ForEach(delegate(Contact cnt)
                {
                    DelCont.cbx_DeleteContact_List.Items.Add(cnt.Name_for_user);
                });
                //DelCont.registerHandler();
                //AddCont.AddcontactSearch += new AddContactSearchEventHandler(im_AddcontactSearch);
                //AddCont.AddcontactAdd += new AddContactAddEventHandler(im_AddcontactAdd);
                DelCont.Show();
            }));
        }

        private void trv_ContactList_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {

        }

        private void trv_ContactList_Loaded(object sender, RoutedEventArgs e)
        {
            im.ContactList.ForEach(delegate(Contact cnt)
            {
                trv_ContactList.Items.Add(cnt.Name_for_user);
            });
        }
        public void RefreshTreeView(List<Contact> ContactList)
        {
            Dispatcher.BeginInvoke(new ThreadStart(delegate
            {
                trv_ContactList.Items.Clear();
                ContactList.ForEach(delegate(Contact cnt)
                {
                    trv_ContactList.Items.Add(cnt.Name_for_user);
                });
                trv_Statuses.Items.Clear();
                ContactList.ForEach(delegate(Contact cnt)
                {
                    switch (cnt.status / 10)
                    {
                        case 0:
                            trv_Statuses.Items.Add("Offline");
                            break;
                        case 1:
                            trv_Statuses.Items.Add("Online");
                            break;
                        case 2:
                            trv_Statuses.Items.Add("Busy");
                            break;
                        case 3:
                            trv_Statuses.Items.Add("DnD");
                            break;
                    }
                });
            }));
        }

        private void btn_Seeing_Click(object sender, RoutedEventArgs e)
        {
            im.GetSeeingList();
        }

        private void btn_Unseeing_Click(object sender, RoutedEventArgs e)
        {
            im.GetUnseeingList();
        }

        private void btn_Ignored_Click(object sender, RoutedEventArgs e)
        {
            im.GetIgnoringList();
        }

        private void btn_OtherProfile_Click(object sender, RoutedEventArgs e)
        {
            im.GetOtherProfile();
        }

        private void trv_ContactList_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            txb_History.Text = "";
            if (e.NewValue != null)
            {
                Contact recipient = im.ContactList.Find(p => p.Name_for_user == e.NewValue);
                btn_OtherProfile.IsEnabled = true;
                im.Messages.FindAll(p => (p.Id_from == recipient.Id_contact) || (p.Id_whom == recipient.Id_contact)).ForEach(delegate(Message msg)
                {
                    if (msg.Id_from == recipient.Id_user)
                        txb_History.Text = txb_History.Text + "Me (" + msg.Mess_date + ")\n" + msg.Mess_text+"\n";
                    else txb_History.Text = txb_History.Text + recipient.Name_for_user + " (" + msg.Mess_date + ")\n" + msg.Mess_text+"\n";
                });
                im.UnreadMessages.Remove(recipient.Name_for_user);
                if (im.UnreadMessages.Count == 0) helper.StopFlashing();
            }
        }
        public void refreshHistory()
        {
            Dispatcher.BeginInvoke(new ThreadStart(delegate 
            {
            txb_History.Text = "";
            if (trv_ContactList.SelectedItem!=null)
            {
                Contact recipient = im.ContactList.Find(p => p.Name_for_user == trv_ContactList.SelectedItem.ToString());
                btn_OtherProfile.IsEnabled = true;
                im.Messages.FindAll(p => (p.Id_from == recipient.Id_contact) || (p.Id_whom == recipient.Id_contact)).ForEach(delegate(Message msg)
                {
                    if (msg.Id_from == recipient.Id_user)
                        txb_History.Text = txb_History.Text + "Me (" + msg.Mess_date + ")\n" + msg.Mess_text + "\n";
                    else txb_History.Text = txb_History.Text + recipient.Name_for_user + " (" + msg.Mess_date + ")\n" + msg.Mess_text + "\n";
                });
            }
            ScrollViewer.ScrollToEnd();
            }));
        }

        private void txb_Message_GotFocus(object sender, RoutedEventArgs e)
        {
            txb_Message.Text = "";
        }

        private void btn_Send_Click(object sender, RoutedEventArgs e)
        {
            im.SendMessage(txb_Message.Text, trv_ContactList.SelectedItem.ToString(),0);
        }


    }
}