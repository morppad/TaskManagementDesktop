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
    /// Логика взаимодействия для RegisterWindow.xaml
    /// </summary>
    public partial class RegisterWindow : Page
    {
        public RegisterWindow()
        {
            InitializeComponent();
        }
        private async void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string email = EmailTextBox.Text;
            string name = NameTextBox.Text;
            string password = PasswordBox.Password;

            await Services.AuthServises.RegisterUser(
                email,
                password,
                name,
                onSuccess: () =>
                {
                    MessageBox.Show("Registration successful!");
                    //TODO
                    
                },
                onError: (error) =>
                {
                    ErrorTextBlock.Text = error;
                    ErrorTextBlock.Visibility = Visibility.Visible;
                });
        }

        private void SwitchToLogin_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}
