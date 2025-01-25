using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using TaskManagment.Data;

namespace TaskManagment.Pages
{
    public partial class ManageUser : Page
    {
        public ObservableCollection<User> Users { get; set; } = new ObservableCollection<User>();

        public ManageUser()
        {
            InitializeComponent();
            LoadUsers();
        }

        private async void LoadUsers()
        {
            try
            {
                // Получаем пользователей из базы данных
                var response = await SupabaseClient.supabase
                    .From<User>()
                    .Get();

                // Очищаем коллекцию и добавляем пользователей
                Users.Clear();
                foreach (var user in response.Models)
                {
                    Users.Add(user);
                }

                // Привязываем данные к DataGrid
                UsersDataGrid.ItemsSource = Users;
            }
            catch (Exception ex)
            {
                // Показываем сообщение об ошибке
                MessageBox.Show($"Ошибка загрузки пользователей: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

