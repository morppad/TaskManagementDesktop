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
using TaskManagment.Data;
using TaskManagment.Services;

namespace TaskManagment.Pages
{
    /// <summary>
    /// Логика взаимодействия для AuthPage.xaml
    /// </summary>
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
                // Используем AuthServises для авторизации
                await AuthServises.LoginUser(
                    email,
                    password,
                    onSuccess: (userId, role) =>
                    {
                        // Перенаправляем в зависимости от роли
                        if (role == "manager")
                        {
                            NavigationService?.Navigate(new ItsTimeToChoose());
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
