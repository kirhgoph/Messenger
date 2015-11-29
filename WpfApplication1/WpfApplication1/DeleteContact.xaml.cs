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
    /// Логика взаимодействия для DeleteContact.xaml
    /// </summary>
    public partial class DeleteContact : Window
    {
        public MainWindow ParentWindow { get; set; }
        public DeleteContact()
        {
            InitializeComponent();
        }

        private void btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btn_Ok_Click(object sender, RoutedEventArgs e)
        {
            ParentWindow.im.DeleteContact(cbx_DeleteContact_List.SelectedValue.ToString());
            this.Close();
        }

        private void cbx_DeleteContact_List_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btn_Ok.IsEnabled = true;
        }
    }
}
