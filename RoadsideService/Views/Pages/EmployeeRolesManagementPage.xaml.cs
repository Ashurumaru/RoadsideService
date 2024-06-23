using RoadsideService.Data;
using RoadsideService.Views.Crud;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace RoadsideService.Views.Pages
{
    public partial class EmployeeRolesManagementPage : Page
    {
        private RoadsideServiceEntities _context;

        public EmployeeRolesManagementPage()
        {
            InitializeComponent();
            _context = new RoadsideServiceEntities();
            LoadRoles();
        }

        private void LoadRoles()
        {
            var roles = _context.EmployeeRoles.ToList();
            RolesItemsControl.ItemsSource = roles;
        }

        private void AddRoleButton_Click(object sender, RoutedEventArgs e)
        {
            var roleCreateEditPage = new RoleCreateEditPage();
            NavigationService.Navigate(roleCreateEditPage);
        }

        private void EditRoleButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var roleId = (int)button.CommandParameter;
            var selectedRole = _context.EmployeeRoles.FirstOrDefault(r => r.RoleID == roleId);

            if (selectedRole != null)
            {
                var roleCreateEditPage = new RoleCreateEditPage(selectedRole);
                NavigationService.Navigate(roleCreateEditPage);
            }
        }

        private void DeleteRoleButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var roleId = (int)button.CommandParameter;
            var selectedRole = _context.EmployeeRoles.FirstOrDefault(r => r.RoleID == roleId);

            if (selectedRole != null)
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить эту должность?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    _context.EmployeeRoles.Remove(selectedRole);
                    _context.SaveChanges();
                    LoadRoles();
                }
            }
        }
    }
}
