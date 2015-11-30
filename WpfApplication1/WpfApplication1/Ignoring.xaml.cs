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
    /// Логика взаимодействия для Ignored.xaml
    /// </summary>
    public partial class Ignoring : Window
    {
        public MainWindow ParentWindow { get; set; }
        public Ignoring()
        {
            InitializeComponent();
        }

        private void cbx_AddIgnoring_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btn_AddIgnoring.IsEnabled = true;
        }

        private void cbx_DeleteIgnoring_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btn_DeleteIgnoring.IsEnabled = true;
        }

        private void btn_AddIgnoring_Click(object sender, RoutedEventArgs e)
        {
            ParentWindow.im.AddPrivacy("Ignoring", ParentWindow.im.ContactList.Find(p => p.Name_for_user == cbx_AddIgnoring.SelectedItem.ToString()).Id_contact);
            MessageBox.Show("Added");
            this.Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btn_DeleteIgnoring_Click(object sender, RoutedEventArgs e)
        {
            ParentWindow.im.DeletePrivacy("Ignoring", ParentWindow.im.ContactList.Find(p => p.Name_for_user == cbx_DeleteIgnoring.SelectedItem.ToString()).Id_contact);
            MessageBox.Show("Deleted");
            this.Close();
        }
    }
}
