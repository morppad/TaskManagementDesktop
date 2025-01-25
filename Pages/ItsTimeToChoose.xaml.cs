using System.Windows;
using System.Windows.Controls;

namespace TaskManagment.Pages
{
    public partial class ItsTimeToChoose : Page
    {
        public ItsTimeToChoose()
        {
            InitializeComponent();
        }

        // Обработчик кнопки "Управление пользователями"
        private void ManageUserButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new ManageUser());
        }

        // Обработчик кнопки "Управление задачами"
        private void ManageTaskButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new ManageTask());
        }
    }
}

