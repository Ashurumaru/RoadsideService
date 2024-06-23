using RoadsideService.Data;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace RoadsideService.Views.Crud
{
    public partial class EmployeeCreateEditPage : Page
    {
        private RoadsideServiceEntities _context;
        private Employees _employee;
        private bool _isPasswordVisible = false;

        public EmployeeCreateEditPage()
        {
            InitializeComponent();
            _context = new RoadsideServiceEntities();
            LoadRoles();
        }

        public EmployeeCreateEditPage(Employees employee) : this()
        {
            _employee = employee;
            LoadEmployeeDetails();
        }

        private void LoadRoles()
        {
            RoleComboBox.ItemsSource = _context.EmployeeRoles.ToList();
            RoleComboBox.DisplayMemberPath = "RoleName";
            RoleComboBox.SelectedValuePath = "RoleID";
        }

        private void LoadEmployeeDetails()
        {
            if (_employee != null)
            {
                FirstNameTextBox.Text = _employee.FirstName;
                MiddleNameTextBox.Text = _employee.MiddleName;
                LastNameTextBox.Text = _employee.LastName;
                PhoneTextBox.Text = _employee.Phone;
                EmailTextBox.Text = _employee.Email;
                HireDatePicker.SelectedDate = _employee.HireDate;
                RoleComboBox.SelectedValue = _employee.RoleID;
                LoginTextBox.Text = _employee.Login;
                PasswordBox.Password = _employee.Password;
                PasswordTextBox.Text = _employee.Password;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (_employee == null)
            {
                _employee = new Employees();
                _context.Employees.Add(_employee);
            }

            _employee.FirstName = FirstNameTextBox.Text;
            _employee.MiddleName = MiddleNameTextBox.Text;
            _employee.LastName = LastNameTextBox.Text;
            _employee.Phone = PhoneTextBox.Text;
            _employee.Email = EmailTextBox.Text;
            _employee.HireDate = (DateTime)HireDatePicker.SelectedDate;
            _employee.RoleID = (int)RoleComboBox.SelectedValue;
            _employee.Login = LoginTextBox.Text;

            if (_isPasswordVisible)
            {
                _employee.Password = PasswordTextBox.Text;
            }
            else
            {
                _employee.Password = PasswordBox.Password;
            }

            _context.SaveChanges();

            NavigationService.GoBack();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void TogglePasswordVisibility_Click(object sender, RoutedEventArgs e)
        {
            _isPasswordVisible = !_isPasswordVisible;

            if (_isPasswordVisible)
            {
                PasswordTextBox.Text = PasswordBox.Password;
                PasswordTextBox.Visibility = Visibility.Visible;
                PasswordBox.Visibility = Visibility.Collapsed;
                ((Button)sender).Content = "Скрыть";
            }
            else
            {
                PasswordBox.Password = PasswordTextBox.Text;
                PasswordTextBox.Visibility = Visibility.Collapsed;
                PasswordBox.Visibility = Visibility.Visible;
                ((Button)sender).Content = "Показать";
            }
        }
    }
}
