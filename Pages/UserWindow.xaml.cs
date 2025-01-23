using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows;
using TaskManagment.Data;

namespace TaskManagment.Pages
{
    /// <summary>
    /// Логика взаимодействия для UserWindow.xaml
    /// </summary>
    public partial class UserWindow : Page
    {
        public ObservableCollection<TaskModel> Tasks { get; set; } = new ObservableCollection<TaskModel>();
        private int _currentUserId; // Хранит ID текущего пользователя

        public UserWindow(int userId)
        {
            InitializeComponent();
            _currentUserId = userId; // Сохраняем ID пользователя
            TasksListView.ItemsSource = Tasks; // Привязываем список задач к ListView
            LoadTasks(_currentUserId); // Загружаем задачи при инициализации
        }

        // Метод загрузки задач пользователя
        private async void LoadTasks(int userId)
        {
            try
            {
                var tasks = await FetchUserTasks(userId);
                Tasks.Clear();
                foreach (var task in tasks)
                {
                    Tasks.Add(task);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading tasks: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Метод получения задач из Supabase
        private async Task<ObservableCollection<TaskModel>> FetchUserTasks(int userId)
        {
            try
            {
                var response = await SupabaseClient.supabase
                    .From<TaskModel>()
                    .Filter("user_id", Supabase.Postgrest.Constants.Operator.Equals, userId)
                    .Get();

                return new ObservableCollection<TaskModel>(response.Models);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching user tasks: {ex.Message}");
                return new ObservableCollection<TaskModel>();
            }
        }

        // Обработчик нажатия кнопки "Обновить"
        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadTasks(_currentUserId); // Повторная загрузка задач
        }
    }
}