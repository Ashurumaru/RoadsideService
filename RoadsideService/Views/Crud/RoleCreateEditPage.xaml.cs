using RoadsideService.Data;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace RoadsideService.Views.Crud
{
    public partial class RoleCreateEditPage : Page
    {
        private RoadsideServiceEntities _context;
        private EmployeeRoles _role;

        public RoleCreateEditPage()
        {
            InitializeComponent();
            _context = new RoadsideServiceEntities();
        }

        public RoleCreateEditPage(EmployeeRoles role) : this()
        {
            _role = role;
            LoadRoleDetails();
        }

        private void LoadRoleDetails()
        {
            if (_role != null)
            {
                RoleNameTextBox.Text = _role.RoleName;
                DescriptionTextBox.Text = _role.Description;

            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (_role == null)
            {
                _role = new EmployeeRoles();
                _context.EmployeeRoles.Add(_role);
            }

            _role.RoleName = RoleNameTextBox.Text;
            _role.Description = DescriptionTextBox.Text;


            _context.SaveChanges();

            NavigationService.GoBack();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}
