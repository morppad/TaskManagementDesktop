using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using TaskManagment.Data;

namespace TaskManagment.Pages
{
    public partial class UserCommentPage : Page
    {
        private int CurrentUserId { get; set; }
        public TaskModel CurrentTask { get; set; }
        public ObservableCollection<CommentViewModel> Comments { get; set; } = new ObservableCollection<CommentViewModel>();

        public UserCommentPage(TaskModel task, int userId)
        {
            InitializeComponent();
            CurrentTask = task;
            CurrentUserId = userId; // Сохраняем ID текущего пользователя
            DataContext = CurrentTask;

            LoadComments(task.Id);
        }

        private async void LoadComments(long taskId)
        {
            try
            {
                var response = await SupabaseClient.supabase
                    .From<CommentsModel>()
                    .Filter("task_id", Supabase.Postgrest.Constants.Operator.Equals, taskId.ToString())
                    .Get();

                Comments.Clear();
                foreach (var comment in response.Models)
                {
                    Comments.Add(new CommentViewModel
                    {
                        Content = comment.Content,
                        IsCurrentUserComment = comment.userId == CurrentUserId.ToString()
                    });
                }
                CommentsListBox.ItemsSource = Comments;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки комментариев: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void SendComment_Click(object sender, RoutedEventArgs e)
        {
            string content = CommentTextBox.Text.Trim();

            if (string.IsNullOrEmpty(content))
            {
                MessageBox.Show("Комментарий не может быть пустым.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var newComment = new CommentsModel
                {
                    Content = content,
                    TaskId = CurrentTask.Id.ToString(),
                    userId = CurrentUserId.ToString(),
                    CreatedAt = DateTime.UtcNow
                };

                await SupabaseClient.supabase.From<CommentsModel>().Insert(newComment);

                MessageBox.Show("Комментарий успешно добавлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                CommentTextBox.Text = string.Empty;
                LoadComments(CurrentTask.Id);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка отправки комментария: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshComments_Click(object sender, RoutedEventArgs e)
        {
            LoadComments(CurrentTask.Id);
        }
    }
}

