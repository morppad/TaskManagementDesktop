using System.Windows;
using System.Windows.Controls;
using TaskManagment.Data;

namespace TaskManagment.Pages
{
    public partial class ItsTimeToChoose : Page
    {
        private User CurrentUser { get; set; }

        public ItsTimeToChoose(User currentUser)
        {
            InitializeComponent();
            CurrentUser = currentUser;
        }

        // Обработчик кнопки "Управление пользователями"
        private void ManageUserButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new ManageUser());
        }

        // Обработчик кнопки "Управление задачами"
        private void ManageTaskButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new TaskManage(CurrentUser));
        }
    }
}
