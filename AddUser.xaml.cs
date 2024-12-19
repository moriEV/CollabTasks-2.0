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

namespace CollabTasks_2._0
{
    /// <summary>
    /// Логика взаимодействия для AddUser.xaml
    /// </summary>
    public partial class AddUser : Window
    {
        public bool Admin { get; private set; }
        public string Email { get; private set; }
        public AddUser()
        {
            InitializeComponent();
            this.Icon = new System.Windows.Media.Imaging.BitmapImage(new Uri("Icon.png", UriKind.Relative));
        }

        private void AddUserButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            button.IsEnabled = false;
            if (!string.IsNullOrWhiteSpace(EmailTextBox.Text))
            {
                Email = EmailTextBox.Text;
                Admin = AdminCheckBox.IsChecked == true;
                DialogResult = true; // Закрыть окно и вернуть результат
                Close();
            }
            else
            {
                MessageBox.Show("Введите корректный email.","Добавление пользователя",MessageBoxButton.OK,MessageBoxImage.Information);
            }
        }
    }
}
