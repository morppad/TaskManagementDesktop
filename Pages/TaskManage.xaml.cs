using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TaskManagment.Data;

namespace TaskManagment.Pages
{
    public partial class TaskManage : Page
    {
        public ObservableCollection<TaskViewModel> Tasks { get; set; } = new ObservableCollection<TaskViewModel>();
        private User CurrentUser { get; set; }

        public TaskManage(User currentUser)
        {
            InitializeComponent();
            CurrentUser = currentUser;
            LoadTasks();
        }

        private async void LoadTasks()
        {
            try
            {
                var tasksResponse = await SupabaseClient.supabase.From<TaskModel>().Get();
                var usersResponse = await SupabaseClient.supabase.From<User>().Get();

                var users = usersResponse.Models.ToDictionary(u => u.Id, u => u.Name);

                Tasks.Clear();
                foreach (var task in tasksResponse.Models)
                {
                    Tasks.Add(new TaskViewModel
                    {
                        Title = task.Title,
                        Priority = task.Priority,
                        UserName = users.ContainsKey(task.userId) ? users[task.userId] : "Unknown",
                        DueDate = task.DueDate?.ToString("yyyy-MM-dd") ?? "No due date",
                        Status = task.Status
                    });
                }

                TasksListView.ItemsSource = Tasks;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки задач: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        private async void TasksListView_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (TasksListView.SelectedItem is not TaskViewModel selectedTask)
            {
                MessageBox.Show("Задача не выбрана. Пожалуйста, выберите задачу.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var taskResponse = await SupabaseClient.supabase
                    .From<TaskModel>()
                    .Filter("title", Supabase.Postgrest.Constants.Operator.Equals, selectedTask.Title)
                    .Get();

                if (!taskResponse.Models.Any())
                {
                    MessageBox.Show("Не удалось найти задачу в базе данных.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var task = taskResponse.Models.First();

                var commentsResponse = await SupabaseClient.supabase
                    .From<CommentsModel>()
                    .Filter("task_id", Supabase.Postgrest.Constants.Operator.Equals, task.Id.ToString())
                    .Get();

                var comments = commentsResponse.Models.Select(comment => new CommentViewModel
                {
                    Content = comment.Content,
                    IsCurrentUserComment = comment.userId == CurrentUser.Id.ToString(),
                    userId = comment.userId
                }).ToList();

                OpenTaskContextMenu(task, comments);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке задачи: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private async void AddTaskButton_Click(object sender, RoutedEventArgs e)
        {
            // Создаем окно добавления задачи
            var stackPanel = new StackPanel { Margin = new Thickness(10) };

            // Поля для ввода
            stackPanel.Children.Add(new TextBlock { Text = "Наименование задачи", Margin = new Thickness(0, 5, 0, 5) });
            var titleInput = new TextBox { Margin = new Thickness(0, 5, 0, 10) };
            stackPanel.Children.Add(titleInput);

            stackPanel.Children.Add(new TextBlock { Text = "Дата сдачи (формат: YYYY-MM-DD)", Margin = new Thickness(0, 5, 0, 5) });
            var dueDateInput = new TextBox { Margin = new Thickness(0, 5, 0, 10) };
            stackPanel.Children.Add(dueDateInput);

            stackPanel.Children.Add(new TextBlock { Text = "Исполнитель", Margin = new Thickness(0, 5, 0, 5) });
            var userComboBox = new ComboBox { Margin = new Thickness(0, 5, 0, 10) };
            var usersResponse = await SupabaseClient.supabase
                .From<User>()
                .Filter("role", Supabase.Postgrest.Constants.Operator.Equals, "user")
                .Get();
            userComboBox.ItemsSource = usersResponse.Models.Select(u => new { u.Id, u.Name }).ToList();
            userComboBox.DisplayMemberPath = "Name";
            userComboBox.SelectedValuePath = "Id";
            stackPanel.Children.Add(userComboBox);

            stackPanel.Children.Add(new TextBlock { Text = "Приоритет", Margin = new Thickness(0, 5, 0, 5) });
            var priorityComboBox = new ComboBox { Margin = new Thickness(0, 5, 0, 10) };
            priorityComboBox.Items.Add("low");
            priorityComboBox.Items.Add("high");
            stackPanel.Children.Add(priorityComboBox);

            stackPanel.Children.Add(new TextBlock { Text = "Описание", Margin = new Thickness(0, 5, 0, 5) });
            var descriptionInput = new TextBox { Margin = new Thickness(0, 5, 0, 10), AcceptsReturn = true, Height = 100 };
            stackPanel.Children.Add(descriptionInput);

            // Кнопка создания
            var createButton = new Button { Content = "Создать", Width = 100, Margin = new Thickness(5) };
            stackPanel.Children.Add(createButton);

            var window = new Window
            {
                Title = "Добавить задачу",
                Content = stackPanel,
                Width = 400,
                Height = 400,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };

            createButton.Click += async (s, args) =>
            {
                try
                {
                    // Проверка заполнения полей
                    if (string.IsNullOrWhiteSpace(titleInput.Text) || string.IsNullOrWhiteSpace(dueDateInput.Text) ||
                        userComboBox.SelectedValue == null || priorityComboBox.SelectedItem == null)
                    {
                        MessageBox.Show("Заполните все поля!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // Создание новой задачи
                    var newTask = new TaskModel
                    {
                        Title = titleInput.Text.Trim(),
                        DueDate = DateTime.Parse(dueDateInput.Text.Trim()),
                        userId = int.Parse(userComboBox.SelectedValue.ToString()),
                        Priority = priorityComboBox.SelectedItem.ToString(),
                        Description = descriptionInput.Text.Trim(),
                        Status = "in-progress"
                    };

                    await SupabaseClient.supabase.From<TaskModel>().Insert(newTask);

                    // Обновляем список задач
                    LoadTasks();

                    window.Close();
                    MessageBox.Show("Задача успешно добавлена!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка добавления задачи: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };

            window.ShowDialog();
        }

        private async void OpenTaskContextMenu(TaskModel task, List<CommentViewModel> comments)
        {
            var stackPanel = new StackPanel { Margin = new Thickness(10) };

            // Информация о задаче
            stackPanel.Children.Add(new TextBlock { Text = $"Задача: {task.Title}", FontSize = 20, FontWeight = FontWeights.Bold });
            stackPanel.Children.Add(new TextBlock { Text = $"Priority: {task.Priority}" });
            stackPanel.Children.Add(new TextBlock { Text = $"Assigned User ID: {task.userId}" });
            stackPanel.Children.Add(new TextBlock { Text = $"Due Date: {task.DueDate}" });
            stackPanel.Children.Add(new TextBlock { Text = $"Status: {task.Status}" });
            stackPanel.Children.Add(new TextBlock { Text = $"Created At: {task.CreatedAt}" });
            stackPanel.Children.Add(new TextBlock { Text = $"Description: {task.Description}" });

            // Поле комментариев
            var commentsListBox = new ListBox
            {
                Margin = new Thickness(0, 10, 0, 10),
                MaxHeight = 300 // Ограничение высоты для прокрутки
            };

            // Получаем данные пользователей
            var usersResponse = await SupabaseClient.supabase.From<User>().Get();
            var users = usersResponse.Models.ToDictionary(u => u.Id.ToString(), u => u.Name);

            foreach (var comment in comments)
            {
                var border = new Border
                {
                    Margin = new Thickness(5),
                    Padding = new Thickness(10),
                    CornerRadius = new CornerRadius(5),
                    Background = comment.IsCurrentUserComment ? Brushes.LightBlue : Brushes.LightGray
                };

                var commentStack = new StackPanel();

                // Отображаем имя пользователя
                var userName = users.ContainsKey(comment.userId) ? users[comment.userId] : "Unknown User";
                commentStack.Children.Add(new TextBlock
                {
                    Text = $"{userName}:",
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(0, 0, 0, 5)
                });

                // Отображаем текст комментария
                commentStack.Children.Add(new TextBlock
                {
                    Text = comment.Content,
                    TextWrapping = TextWrapping.Wrap
                });

                border.Child = commentStack;
                commentsListBox.Items.Add(border);
            }

            // Добавляем прокрутку для комментариев
            var scrollViewer = new ScrollViewer
            {
                Content = commentsListBox,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };
            stackPanel.Children.Add(scrollViewer);

            // Поле для ввода комментария
            var commentInput = new TextBox { Margin = new Thickness(0, 10, 0, 10), Text = "Введите комментарий" };
            commentInput.GotFocus += (s, e) =>
            {
                if (commentInput.Text == "Введите комментарий") commentInput.Text = string.Empty;
            };
            commentInput.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(commentInput.Text)) commentInput.Text = "Введите комментарий";
            };

            // Панель для кнопок
            var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal };
            var addButton = new Button { Content = "Добавить", Width = 100, Margin = new Thickness(5) };
            var refreshButton = new Button { Content = "Обновить", Width = 100, Margin = new Thickness(5) };

            // Логика кнопки "Добавить"
            addButton.Click += async (sender, e) =>
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(commentInput.Text) || commentInput.Text == "Введите комментарий")
                    {
                        MessageBox.Show("Комментарий не может быть пустым.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    if (CurrentUser == null)
                    {
                        MessageBox.Show("Ошибка: текущий пользователь не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    var newComment = new CommentsModel
                    {
                        Content = commentInput.Text.Trim(),
                        TaskId = task.Id.ToString(),
                        userId = CurrentUser.Id.ToString(), // Используем ID текущего пользователя
                        CreatedAt = DateTime.UtcNow
                    };

                    await SupabaseClient.supabase.From<CommentsModel>().Insert(newComment);

                    // Добавляем новый комментарий в список
                    var newCommentViewModel = new CommentViewModel
                    {
                        Content = newComment.Content,
                        IsCurrentUserComment = true,
                        userId = newComment.userId,
                        UserName = CurrentUser.Name
                    };

                    var border = new Border
                    {
                        Margin = new Thickness(5),
                        Padding = new Thickness(10),
                        CornerRadius = new CornerRadius(5),
                        Background = Brushes.LightBlue
                    };

                    var commentStack = new StackPanel();
                    commentStack.Children.Add(new TextBlock
                    {
                        Text = $"{newCommentViewModel.UserName}:",
                        FontWeight = FontWeights.Bold,
                        Margin = new Thickness(0, 0, 0, 5)
                    });
                    commentStack.Children.Add(new TextBlock
                    {
                        Text = newCommentViewModel.Content,
                        TextWrapping = TextWrapping.Wrap
                    });

                    border.Child = commentStack;
                    commentsListBox.Items.Add(border);

                    commentInput.Text = "Введите комментарий";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка добавления комментария: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };

            // Логика кнопки "Обновить"
            refreshButton.Click += async (sender, e) =>
            {
                try
                {
                    var updatedComments = await SupabaseClient.supabase
                        .From<CommentsModel>()
                        .Filter("task_id", Supabase.Postgrest.Constants.Operator.Equals, task.Id.ToString())
                        .Get();

                    commentsListBox.Items.Clear();

                    foreach (var updatedComment in updatedComments.Models.Select(c => new CommentViewModel
                    {
                        Content = c.Content,
                        IsCurrentUserComment = c.userId == CurrentUser.Id.ToString(),
                        userId = c.userId,
                        UserName = users.ContainsKey(c.userId) ? users[c.userId] : "Unknown User"
                    }))
                    {
                        var border = new Border
                        {
                            Margin = new Thickness(5),
                            Padding = new Thickness(10),
                            CornerRadius = new CornerRadius(5),
                            Background = updatedComment.IsCurrentUserComment ? Brushes.LightBlue : Brushes.LightGray
                        };

                        var commentStack = new StackPanel();
                        commentStack.Children.Add(new TextBlock
                        {
                            Text = $"{updatedComment.UserName}:",
                            FontWeight = FontWeights.Bold,
                            Margin = new Thickness(0, 0, 0, 5)
                        });
                        commentStack.Children.Add(new TextBlock
                        {
                            Text = updatedComment.Content,
                            TextWrapping = TextWrapping.Wrap
                        });

                        border.Child = commentStack;
                        commentsListBox.Items.Add(border);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка обновления комментариев: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };

            buttonPanel.Children.Add(addButton);
            buttonPanel.Children.Add(refreshButton);
            stackPanel.Children.Add(commentInput);
            stackPanel.Children.Add(buttonPanel);

            // Окно отображения задачи
            var window = new Window
            {
                Title = $"Задача: {task.Title}",
                Content = stackPanel,
                Width = 400,
                Height = 600,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };

            window.ShowDialog();
        }

        private async void DeleteTaskMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (TasksListView.SelectedItem is not TaskViewModel selectedTask)
            {
                MessageBox.Show("Пожалуйста, выберите задачу для удаления.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"Вы уверены, что хотите удалить задачу \"{selectedTask.Title}\"?",
                "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    // Получаем задачу из базы данных
                    var taskResponse = await SupabaseClient.supabase
                        .From<TaskModel>()
                        .Filter("title", Supabase.Postgrest.Constants.Operator.Equals, selectedTask.Title)
                        .Get();

                    if (taskResponse.Models.Any())
                    {
                        var task = taskResponse.Models.First();

                        // Удаляем задачу
                        await SupabaseClient.supabase.From<TaskModel>().Delete(task);

                        // Обновляем список задач
                        LoadTasks();

                        MessageBox.Show("Задача успешно удалена!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления задачи: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }



        private async void EditTaskMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (TasksListView.SelectedItem is not TaskViewModel selectedTask)
            {
                MessageBox.Show("Пожалуйста, выберите задачу для редактирования.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var taskResponse = await SupabaseClient.supabase
                .From<TaskModel>()
                .Filter("title", Supabase.Postgrest.Constants.Operator.Equals, selectedTask.Title)
                .Get();

            if (!taskResponse.Models.Any())
            {
                MessageBox.Show("Ошибка: задача не найдена.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var task = taskResponse.Models.First();

            // Открываем окно редактирования
            var stackPanel = new StackPanel { Margin = new Thickness(10) };

            stackPanel.Children.Add(new TextBlock { Text = "Наименование задачи", Margin = new Thickness(0, 5, 0, 5) });
            var titleInput = new TextBox { Text = task.Title, Margin = new Thickness(0, 5, 0, 10) };
            stackPanel.Children.Add(titleInput);

            stackPanel.Children.Add(new TextBlock { Text = "Дата сдачи (формат: YYYY-MM-DD)", Margin = new Thickness(0, 5, 0, 5) });
            var dueDateInput = new TextBox { Text = task.DueDate?.ToString("yyyy-MM-dd"), Margin = new Thickness(0, 5, 0, 10) };
            stackPanel.Children.Add(dueDateInput);

            stackPanel.Children.Add(new TextBlock { Text = "Приоритет", Margin = new Thickness(0, 5, 0, 5) });
            var priorityComboBox = new ComboBox { Margin = new Thickness(0, 5, 0, 10) };
            priorityComboBox.Items.Add("low");
            priorityComboBox.Items.Add("high");
            priorityComboBox.SelectedItem = task.Priority;
            stackPanel.Children.Add(priorityComboBox);

            stackPanel.Children.Add(new TextBlock { Text = "Описание", Margin = new Thickness(0, 5, 0, 5) });
            var descriptionInput = new TextBox { Text = task.Description, Margin = new Thickness(0, 5, 0, 10), AcceptsReturn = true, Height = 100 };
            stackPanel.Children.Add(descriptionInput);

            var updateButton = new Button { Content = "Обновить", Width = 100, Margin = new Thickness(5) };
            stackPanel.Children.Add(updateButton);

            var window = new Window
            {
                Title = "Редактировать задачу",
                Content = stackPanel,
                Width = 400,
                Height = 400,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };

            updateButton.Click += async (s, args) =>
            {
                try
                {
                    // Проверка полей
                    if (string.IsNullOrWhiteSpace(titleInput.Text) || string.IsNullOrWhiteSpace(dueDateInput.Text) ||
                        priorityComboBox.SelectedItem == null)
                    {
                        MessageBox.Show("Заполните все поля!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // Обновляем задачу
                    task.Title = titleInput.Text.Trim();
                    task.DueDate = DateTime.Parse(dueDateInput.Text.Trim());
                    task.Priority = priorityComboBox.SelectedItem.ToString();
                    task.Description = descriptionInput.Text.Trim();

                    await SupabaseClient.supabase.From<TaskModel>().Update(task);
                    LoadTasks();

                    window.Close();
                    MessageBox.Show("Задача успешно обновлена!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка обновления задачи: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };

            window.ShowDialog();
        }

        private async void ViewTaskDetailsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (TasksListView.SelectedItem is not TaskViewModel selectedTask)
            {
                MessageBox.Show("Пожалуйста, выберите задачу для просмотра.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var taskResponse = await SupabaseClient.supabase
                    .From<TaskModel>()
                    .Filter("title", Supabase.Postgrest.Constants.Operator.Equals, selectedTask.Title)
                    .Get();

                if (!taskResponse.Models.Any())
                {
                    MessageBox.Show("Ошибка: задача не найдена.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var task = taskResponse.Models.First();

                var commentsResponse = await SupabaseClient.supabase
                    .From<CommentsModel>()
                    .Filter("task_id", Supabase.Postgrest.Constants.Operator.Equals, task.Id.ToString())
                    .Get();

                var comments = commentsResponse.Models.Select(comment => new CommentViewModel
                {
                    Content = comment.Content,
                    IsCurrentUserComment = comment.userId == CurrentUser.Id.ToString(),
                    userId = comment.userId
                }).ToList();

                OpenTaskContextMenu(task, comments);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке задачи: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}