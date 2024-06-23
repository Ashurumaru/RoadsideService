using RoadsideService.Data;
using RoadsideService.Models;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RoadsideService.Views
{
    public partial class LoginWindow : Window
    {
        private string _password;

        public LoginWindow()
        {
            InitializeComponent();
        }

        private void SignInButton_Click(object sender, RoutedEventArgs e)
        {
            string login = loginTextBox.Text;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(_password))
            {
                errorMessageTextBlock.Text = "Пожалуйста, заполните все поля.";
                return;
            }

            using (var context = new RoadsideServiceEntities())
            {
                var user = context.Employees
                    .FirstOrDefault(p => p.Login == login && p.Password == _password);

                if (user != null)
                {

                    UserModel.EmployeeId = user.EmployeeID;
                    UserModel.FirstName = user.FirstName;
                    UserModel.MiddleName = user.MiddleName;
                    UserModel.LastName = user.LastName;
                    UserModel.Email = user.Email;
                    UserModel.RoleId = user.RoleID;


                    MainWindow mainWindow = new MainWindow();
                    mainWindow.Show();
                    this.Close();
                }
                else
                {
                    errorMessageTextBlock.Text = "Неправильный логин или пароль";
                }
            }
        }

        private void password_PasswordChanged(object sender, RoutedEventArgs e)
        {
            var passwordBox = (PasswordBox)sender;
            _password = passwordBox.Password;
        }

        private void btn_close_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void btn_maximize_Click(object sender, RoutedEventArgs e)
        {
            SwitchWindowState();
        }

        private void btn_minimize_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this).WindowState = WindowState.Minimized;
        }

        private void DockPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount >= 2)
            {
                SwitchWindowState();
                return;
            }

            if (Window.GetWindow(this).WindowState == WindowState.Maximized)
            {
                return;
            }
            else
            {
                if (e.LeftButton == MouseButtonState.Pressed) Window.GetWindow(this).DragMove();
            }
        }

        private void MaximizeWindow()
        {
            Window.GetWindow(this).WindowState = WindowState.Maximized;
        }

        private void RestoreWindow()
        {
            Window.GetWindow(this).WindowState = WindowState.Normal;
        }

        private void SwitchWindowState()
        {
            if (Window.GetWindow(this).WindowState == WindowState.Normal) MaximizeWindow();
            else RestoreWindow();
        }
    }
}
