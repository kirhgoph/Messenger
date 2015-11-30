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
    /// Логика взаимодействия для Unseeing.xaml
    /// </summary>
    public partial class Unseeing : Window
    {
        public MainWindow ParentWindow { get; set; }
        public Unseeing()
        {
            InitializeComponent();
        }
        private void cbx_AddUnseeing_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btn_AddUnseeing.IsEnabled = true;
        }

        private void cbx_DeleteUnseeing_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btn_DeleteUnseeing.IsEnabled = true;
        }

        private void btn_AddUnseeing_Click(object sender, RoutedEventArgs e)
        {
            ParentWindow.im.AddPrivacy("Unseeing", ParentWindow.im.ContactList.Find(p => p.Name_for_user == cbx_AddUnseeing.SelectedItem.ToString()).Id_contact);
            MessageBox.Show("Added");
            this.Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btn_DeleteUnseeing_Click(object sender, RoutedEventArgs e)
        {
            ParentWindow.im.DeletePrivacy("Unseeing", ParentWindow.im.ContactList.Find(p => p.Name_for_user == cbx_DeleteUnseeing.SelectedItem.ToString()).Id_contact);
            MessageBox.Show("Deleted");
            this.Close();
        }
    }
}
