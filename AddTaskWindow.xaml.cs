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
    /// Логика взаимодействия для AddTaskWindow.xaml
    /// </summary>
    public partial class AddTaskWindow : Window
    {
        private readonly List<string> _userIds;
        private readonly List<string> _userNames;

        public AddTaskWindow(List<string> userNames, List<string> userIds)
        {
            InitializeComponent();
            this.Icon = new System.Windows.Media.Imaging.BitmapImage(new Uri("Icon.png", UriKind.Relative));
            _userIds = userIds;
            _userNames = userNames;
            PopulateAssigneesComboBox();
        }

        private void PopulateAssigneesComboBox()
        {
            AssigneesComboBox.Items.Add(new ComboBoxItem { Content = "All" });
            foreach (var userName in _userNames)
            {
                AssigneesComboBox.Items.Add(new ComboBoxItem { Content = userName });
            }
        }

        public string SelectedAssignee => AssigneesComboBox.SelectedItem is ComboBoxItem item
            ? item.Content.ToString()
            : null;


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Проверка, что описание задачи не пустое и выбран исполнитель
            if (string.IsNullOrEmpty(TaskDescriptionTextBox.Text))
            {
                MessageBox.Show("Пожалуйста, введите описание задачи.", "Создание задачи", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (AssigneesComboBox.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, выберите исполнителя.", "Создание задачи", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            DialogResult = true;
        }
    }
}
