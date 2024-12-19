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
    /// Логика взаимодействия для CreateGroupDialogxaml.xaml
    /// </summary>
    public partial class CreateGroupDialogxaml : Window
    {
        public string groupName { get; private set; }
        public CreateGroupDialogxaml()
        {
            InitializeComponent();
            this.Icon = new System.Windows.Media.Imaging.BitmapImage(new Uri("Icon.png", UriKind.Relative));
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            button.IsEnabled = false;
            groupName = groupNameTextBox.Text;
            DialogResult = true;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }
    }
}
