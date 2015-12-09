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
using System.Windows.Shapes;

namespace InstantMessenger
{
    /// <summary>
    /// Логика взаимодействия для Window1.xaml
    /// </summary>
    public partial class AddContact : Window
    {
        public MainWindow ParentWindow { get; set; }
        public event AddContactAddEventHandler AddcontactAdd;
        virtual protected void OnAddcontactAdd(AddContactAddEventArgs e)
        {
            if (AddcontactAdd != null)
                AddcontactAdd(this, e);
        }
        public event AddContactSearchEventHandler AddcontactSearch;
        virtual protected void OnAddcontactSearch(AddContactSearchEventArgs e)
        {
            if (AddcontactSearch != null)
                AddcontactSearch(this, e);
        }
        public AddContact()
        {
            InitializeComponent();
        }
        public void registerHandler()
        {
            ParentWindow.im.AddcontactResult += new AddContactResultEventHandler(AddContact_AddcontactResult);
        }
        void AddContact_AddcontactResult(object sender, AddContactResultEventArgs e)
        {
            Dispatcher.BeginInvoke(new ThreadStart(delegate
            {
                tbx_NameForUser.Text = e.result.Find(p=> p.Length>-1);
                e.result.ForEach(delegate(String str)
                {
                    lbx_SearchResult.Items.Add(str);
                });
            }));
        }
        private void btn_AddContact_Search_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(new ThreadStart(delegate
                {
                    lbx_SearchResult.Items.Clear();
                }));
                OnAddcontactSearch(new AddContactSearchEventArgs(tbx_SearchString.Text));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void lbx_SearchResult_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lbx_SearchResult.SelectedIndex != -1)
            {
                btn_AddcontactOk.IsEnabled=true;
            }
            else btn_AddcontactOk.IsEnabled = false;
            tbx_NameForUser.Text = lbx_SearchResult.SelectedItem.ToString();
        }

        private void tbx_SearchString_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (tbx_SearchString.Text != "")
                btn_AddContact_Search.IsEnabled = true;
            else btn_AddContact_Search.IsEnabled = false;
        }

        private void btn_AddcontactOk_Click(object sender, RoutedEventArgs e)
        {
            OnAddcontactAdd(new AddContactAddEventArgs(lbx_SearchResult.SelectedItem.ToString(), tbx_NameForUser.Text));
            this.Close();
        }

        private void tbx_NameForUser_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (tbx_NameForUser.Text.Length > 50) tbx_NameForUser.Text = tbx_NameForUser.Text.Substring(0, 50);
        }
    }
}
