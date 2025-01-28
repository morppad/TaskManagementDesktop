using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
                var response = await SupabaseClient.supabase
                    .From<User>()
                    .Get();

                Users.Clear();
                foreach (var user in response.Models)
                {
                    Users.Add(user);
                }

                UsersDataGrid.ItemsSource = Users;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки пользователей: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void DeleteUser(User selectedUser)
        {
            try
            {
                await SupabaseClient.supabase.From<User>().Delete(selectedUser);
                Users.Remove(selectedUser);
                MessageBox.Show("Пользователь успешно удалён.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления пользователя: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditUser(User selectedUser)
        {
            // Открываем контекстное меню редактирования
            ContextMenu editMenu = new ContextMenu();

            var usernameLabel = new TextBlock { Text = "Имя:", Margin = new Thickness(5) };
            var usernameInput = new TextBox { Text = selectedUser.Name, Width = 200, Margin = new Thickness(5) };

            var emailLabel = new TextBlock { Text = "Почта:", Margin = new Thickness(5) };
            var emailInput = new TextBox { Text = selectedUser.Email, Width = 200, Margin = new Thickness(5) };

            var roleLabel = new TextBlock { Text = "Роль:", Margin = new Thickness(5) };
            var roleComboBox = new ComboBox
            {
                ItemsSource = new[] { "user", "manager" },
                SelectedItem = selectedUser.Role,
                Width = 200,
                Margin = new Thickness(5)
            };

            var saveButton = new Button
            {
                Content = "Сохранить",
                Width = 100,
                Margin = new Thickness(5),
                HorizontalAlignment = HorizontalAlignment.Center
            };
            saveButton.Click += async (sender, e) =>
            {
                try
                {
                    selectedUser.Name = usernameInput.Text;
                    selectedUser.Email = emailInput.Text;
                    selectedUser.Role = roleComboBox.SelectedItem.ToString();

                    await SupabaseClient.supabase.From<User>().Update(selectedUser);
                    MessageBox.Show("Изменения успешно сохранены.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка сохранения изменений: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };

            var stackPanel = new StackPanel();
            stackPanel.Children.Add(usernameLabel);
            stackPanel.Children.Add(usernameInput);
            stackPanel.Children.Add(emailLabel);
            stackPanel.Children.Add(emailInput);
            stackPanel.Children.Add(roleLabel);
            stackPanel.Children.Add(roleComboBox);
            stackPanel.Children.Add(saveButton);

            var popupWindow = new Window
            {
                Title = "Редактировать пользователя",
                Content = stackPanel,
                Width = 300,
                Height = 300,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };

            popupWindow.ShowDialog();
        }

        private void UsersDataGrid_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (UsersDataGrid.SelectedItem is not User selectedUser) return;

            ContextMenu contextMenu = new ContextMenu();

            MenuItem deleteMenuItem = new MenuItem { Header = "Удалить" };
            deleteMenuItem.Click += (s, args) => DeleteUser(selectedUser);

            MenuItem editMenuItem = new MenuItem { Header = "Редактировать" };
            editMenuItem.Click += (s, args) => EditUser(selectedUser);

            contextMenu.Items.Add(deleteMenuItem);
            contextMenu.Items.Add(editMenuItem);

            contextMenu.IsOpen = true;

            e.Handled = true;
        }

        private void AddUserButton_Click(object sender, RoutedEventArgs e)
        {
            // Создаём элементы для ввода данных
            var usernameLabel = new TextBlock { Text = "Имя пользователя:", Margin = new Thickness(5) };
            var usernameInput = new TextBox { Width = 200, Margin = new Thickness(5) };

            var emailLabel = new TextBlock { Text = "Email:", Margin = new Thickness(5) };
            var emailInput = new TextBox { Width = 200, Margin = new Thickness(5) };

            var passwordLabel = new TextBlock { Text = "Пароль:", Margin = new Thickness(5) };
            var passwordInput = new PasswordBox { Width = 200, Margin = new Thickness(5) };

            var roleLabel = new TextBlock { Text = "Роль:", Margin = new Thickness(5) };
            var roleComboBox = new ComboBox
            {
                ItemsSource = new[] { "user", "manager" },
                SelectedIndex = 0,
                Width = 200,
                Margin = new Thickness(5)
            };

            var saveButton = new Button
            {
                Content = "Сохранить",
                Width = 100,
                Margin = new Thickness(5),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            // Обработка нажатия кнопки "Сохранить"
            saveButton.Click += async (s, args) =>
            {
                try
                {
                    // Получаем данные из полей
                    var username = usernameInput.Text;
                    var email = emailInput.Text;
                    var password = passwordInput.Password;
                    var role = roleComboBox.SelectedItem.ToString();

                    if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                    {
                        MessageBox.Show("Все поля должны быть заполнены.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // Создаём нового пользователя
                    var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
                    var newUser = new User
                    {
                        Name = username,
                        Email = email,
                        Password = hashedPassword,
                        Role = role
                    };

                    // Добавляем пользователя в базу данных
                    await SupabaseClient.supabase.From<User>().Insert(newUser);

                    // Обновляем таблицу
                    LoadUsers();

                    MessageBox.Show("Пользователь успешно добавлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка добавления пользователя: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };

            // Создаём окно для добавления пользователя
            var stackPanel = new StackPanel();
            stackPanel.Children.Add(usernameLabel);
            stackPanel.Children.Add(usernameInput);
            stackPanel.Children.Add(emailLabel);
            stackPanel.Children.Add(emailInput);
            stackPanel.Children.Add(passwordLabel);
            stackPanel.Children.Add(passwordInput);
            stackPanel.Children.Add(roleLabel);
            stackPanel.Children.Add(roleComboBox);
            stackPanel.Children.Add(saveButton);

            var popupWindow = new Window
            {
                Title = "Добавить пользователя",
                Content = stackPanel,
                Width = 300,
                Height = 400,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };

            popupWindow.ShowDialog();
        }
    }
}
