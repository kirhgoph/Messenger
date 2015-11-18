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
using System.Windows.Shapes;

namespace InstantMessenger
{
    /// <summary>
    /// Логика взаимодействия для Window1.xaml
    /// </summary>
    public partial class AddContact : Window
    {
        public event AddContactSearchEventHandler AddcontactSearch;
        virtual protected void OnAddcontactSearch(AddContactSearchEventArgs e)
        {
            if (AddcontactSearch != null)
                AddcontactSearch(this, e);
        }
        public AddContact()
        {
            InitializeComponent();
            AddcontactResult += new AddContactResultEventHandler(im_AddcontactResult);
        }
        public event AddContactResultEventHandler AddcontactResult;
        public virtual void OnAddcontactResult(AddContactResultEventArgs e)
        {
            if (AddcontactResult != null)
                AddcontactResult(this, e);
        }
        private void btn_AddContact_Search_Click(object sender, RoutedEventArgs e)
        {
            if (tbx_SearchString.Text != "")
                OnAddcontactSearch(new AddContactSearchEventArgs(tbx_SearchString.Text));
            else MessageBox.Show("Enter search query!");
        }
    }
}
