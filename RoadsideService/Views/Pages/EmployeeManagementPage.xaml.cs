using RoadsideService.Data;
using RoadsideService.Views.Crud;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace RoadsideService.Views.Pages
{
    public partial class EmployeeManagementPage : Page
    {
        private RoadsideServiceEntities _context;

        public EmployeeManagementPage()
        {
            InitializeComponent();
            _context = new RoadsideServiceEntities();
            LoadEmployees();
        }

        private void LoadEmployees(string searchTerm = "")
        {
            var employeesQuery = _context.Employees.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                employeesQuery = employeesQuery.Where(e => e.FirstName.Contains(searchTerm) ||
                                                           e.LastName.Contains(searchTerm) ||
                                                           e.MiddleName.Contains(searchTerm) ||
                                                           e.Email.Contains(searchTerm) ||
                                                           e.Phone.Contains(searchTerm) ||
                                                           e.Login.Contains(searchTerm));
            }

            var employees = employeesQuery.ToList();
            EmployeesItemsControl.ItemsSource = employees;
        }

        private void AddEmployeeButton_Click(object sender, RoutedEventArgs e)
        {
            var employeeCreateEditPage = new EmployeeCreateEditPage();
            NavigationService.Navigate(employeeCreateEditPage);
        }

        private void EditEmployeeButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var employeeId = (int)button.CommandParameter;
            var selectedEmployee = _context.Employees.FirstOrDefault(u => u.EmployeeID == employeeId);

            if (selectedEmployee != null)
            {
                var employeeCreateEditPage = new EmployeeCreateEditPage(selectedEmployee);
                NavigationService.Navigate(employeeCreateEditPage);
            }
        }

        private void DeleteEmployeeButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var employeeId = (int)button.CommandParameter;
            var selectedEmployee = _context.Employees.FirstOrDefault(u => u.EmployeeID == employeeId);

            if (selectedEmployee != null)
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить этого сотрудника?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    _context.Employees.Remove(selectedEmployee);
                    _context.SaveChanges();
                    LoadEmployees();
                }
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchTerm = SearchTextBox.Text;
            LoadEmployees(searchTerm);
        }

        private void RoleManagementShow_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new EmployeeRolesManagementPage());

        }
    }
}
