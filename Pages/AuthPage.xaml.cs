using System;
using System.Windows;
using System.Windows.Controls;
using TaskManagment.Data;
using TaskManagment.Services;

namespace TaskManagment.Pages
{
    public partial class AuthPage : Page
    {
        public AuthPage()
        {
            InitializeComponent();
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string email = EmailTextBox.Text.Trim();
            string password = PasswordBox.Password.Trim();

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ErrorTextBlock.Text = "Email и пароль не могут быть пустыми.";
                ErrorTextBlock.Visibility = Visibility.Visible;
                return;
            }

            try
            {
                // Используем AuthServices для авторизации
                await AuthServises.LoginUser(
                    email,
                    password,
                    onSuccess: async (userId, role) =>
                    {
                        // Получаем текущего пользователя из базы
                        var currentUser = await SupabaseClient.supabase
                            .From<User>()
                            .Filter("id", Supabase.Postgrest.Constants.Operator.Equals, userId)
                            .Single();

                        // Проверяем успешность получения пользователя
                        if (currentUser == null)
                        {
                            ErrorTextBlock.Text = "Ошибка: текущий пользователь не найден.";
                            ErrorTextBlock.Visibility = Visibility.Visible;
                            return;
                        }

                        // Навигация в зависимости от роли
                        if (role == "manager")
                        {
                            NavigationService?.Navigate(new ItsTimeToChoose(currentUser));
                        }
                        else
                        {
                            NavigationService?.Navigate(new UserWindow(int.Parse(userId)));
                        }
                    },
                    onError: (error) =>
                    {
                        ErrorTextBlock.Text = error;
                        ErrorTextBlock.Visibility = Visibility.Visible;
                    });
            }
            catch (Exception ex)
            {
                ErrorTextBlock.Text = $"Ошибка входа: {ex.Message}";
                ErrorTextBlock.Visibility = Visibility.Visible;
            }
        }

        private void SwitchToRegister_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new RegisterWindow());
        }
    }
}
