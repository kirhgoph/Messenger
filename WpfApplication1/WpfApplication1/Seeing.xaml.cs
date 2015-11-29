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
    /// Логика взаимодействия для Seeing.xaml
    /// </summary>
    public partial class Seeing : Window
    {
        public MainWindow ParentWindow { get; set; }
        public Seeing()
        {
            InitializeComponent();
        }

        private void cbx_AddSeeing_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btn_AddSeeing.IsEnabled = true;
        }

        private void cbx_DeleteSeeing_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btn_DeleteSeeing.IsEnabled = true;
        }

        private void btn_AddSeeing_Click(object sender, RoutedEventArgs e)
        {
            ParentWindow.im.AddPrivacy("Seeing", ParentWindow.im.ContactList.Find(p => p.Name_for_user == cbx_AddSeeing.SelectedItem.ToString()).Id_user);
            MessageBox.Show("Added");
            this.Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
