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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using TaskManagment.Data;
using Supabase.Gotrue;
using Supabase.Postgrest;

namespace TaskManagment.Pages
{
    /// <summary>
    /// Логика взаимодействия для UserWindow.xaml
    /// </summary>
    public partial class UserWindow : Page
    {
        public ObservableCollection<TaskModel> Tasks { get; set; } = new ObservableCollection<TaskModel>();
        public UserWindow(int userId)
        {
            InitializeComponent();
            TasksListView.ItemsSource = Tasks;
            LoadTasks(userId);
        }
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
    }
}

