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
    /// Логика взаимодействия для Profile.xaml
    /// </summary>
    public partial class Profile : Window
    {
        public event ProfileReceivedEventHandler ProfileSave;
        virtual protected void OnProfileSave(ProfileReceivedEventArgs e)
        {
            if (ProfileSave != null)
                ProfileSave(this, e);
        }
        public Profile()
        {
            InitializeComponent();
        }

        private void btn_SaveProfile_Click(object sender, RoutedEventArgs e)
        {
            OnProfileSave(new ProfileReceivedEventArgs(tbx_Profile_FirstName.Text,tbx_Profile_LastName.Text,dpckr_Profile_BirthDate.ToString(),1));
            this.Close();
        }
        private void tbx_Profile_FirstName_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void tbx_Profile_FirstName_TextChanged_1(object sender, TextChangedEventArgs e)
        {
            if (tbx_Profile_FirstName.Text.Length > 50) tbx_Profile_FirstName.Text = tbx_Profile_FirstName.Text.Substring(0, 50);
        }

        private void tbx_Profile_LastName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (tbx_Profile_LastName.Text.Length > 50) tbx_Profile_LastName.Text = tbx_Profile_LastName.Text.Substring(0, 50);
        }

    }
}
