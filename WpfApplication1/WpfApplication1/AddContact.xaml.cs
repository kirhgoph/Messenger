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
        public event ProfileReceivedEventHandler ProfileSave;
        virtual protected void OnProfileSave(ProfileReceivedEventArgs e)
        {
            if (ProfileSave != null)
                ProfileSave(this, e);
        }
        public AddContact()
        {
            InitializeComponent();
        }

        private void btn_AddContact_Search_Click(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
