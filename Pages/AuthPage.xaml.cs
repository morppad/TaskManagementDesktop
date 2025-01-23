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
            string email = EmailTextBox.Text;
            string password = PasswordBox.Password;

            await Services.AuthServises.LoginUser(
                email,
                password,
                onSuccess: (userId, role) =>
                {
                    MessageBox.Show($"Welcome {role}!");
                    // Перейти к следующему окну
                },
                onError: (error) =>
                {
                    ErrorTextBlock.Text = error;
                    ErrorTextBlock.Visibility = Visibility.Visible;
                });
        }
        

        private void SwitchToRegister_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new RegisterWindow());
            
        }


    }
}
