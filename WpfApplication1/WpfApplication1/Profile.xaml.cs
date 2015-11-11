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
        public Profile()
        {
            InitializeComponent();
        }
        void Profile_ProfileReceived(object sender, InstantMessenger.ProfileReceivedEventArgs e)
        {
            Dispatcher.BeginInvoke(new ThreadStart(delegate
            {
                dpckr_Profile_BirthDate.Text = e.BirthDate;
                tbx_Profile_FirstName.Text = e.FirstName;
                tbx_Profile_LastName.Text = e.LastName;
            }));
        }
    }
}
