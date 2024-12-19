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
    /// Логика взаимодействия для UserDetails.xaml
    /// </summary>
    public partial class UserDetails : Window
    {
        public UserDetails()
        {
            InitializeComponent();
            this.Icon = new System.Windows.Media.Imaging.BitmapImage(new Uri("Icon.png", UriKind.Relative));
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnBan_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
