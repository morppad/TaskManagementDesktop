using Supabase;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TaskManagment.Data;

namespace TaskManagment
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //public ObservableCollection<string> Tables { get; set; } = new ObservableCollection<string>();

        public MainWindow()
        {
            InitializeComponent();
            //TablesListBox.ItemsSource = Tables;
            //LoadTables();
        }

        //private async void LoadTables()
        //{
        //    try
        //    {
        //        // Извлечение данных из таблицы `public.tasks`
        //        var response = await Class1.SupabaseClient.From<TaskModel>().Get();
        //        if (response != null)
        //        {
        //            Tables.Clear();
        //            foreach (var task in response.Models)
        //            {
        //                Tables.Add($"Task ID: {task.Id}, Title: {task.Title}, Status: {task.Status}");
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"Ошибка загрузки таблиц: {ex.Message}");
        //    }
        //}
    }
}