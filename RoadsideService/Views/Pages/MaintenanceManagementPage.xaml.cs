using RoadsideService.Data;
using RoadsideService.Models;
using RoadsideService.Views.Crud;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace RoadsideService.Views.Pages
{
    public partial class MaintenanceManagementPage : Page
    {
        private RoadsideServiceEntities _context;

        public MaintenanceManagementPage()
        {
            InitializeComponent();
            _context = new RoadsideServiceEntities();
            LoadMaintenance();
        }

        private void LoadMaintenance()
        {
            var maintenanceRecords = _context.Maintenance
                .Join(_context.Rooms, m => m.RoomID, r => r.RoomID, (m, r) => new { m, r })
                .Join(_context.Employees, mr => mr.m.EmployeeID, e => e.EmployeeID, (mr, e) => new MaintenanceModel
                {
                    MaintenanceID = mr.m.MaintenanceID,
                    RoomNumber = mr.r.RoomNumber,
                    EmployeeFullName = e.LastName + " " + e.FirstName,
                    MaintenanceDate = mr.m.MaintenanceDate,
                    Description = mr.m.Description,
                    Cost = (decimal)mr.m.Cost
                })
                .ToList();

            MaintenanceDataGrid.ItemsSource = maintenanceRecords;
        }

        private void AddMaintenanceButton_Click(object sender, RoutedEventArgs e)
        {
            var maintenanceCreateEditPage = new MaintenanceCreateEditPage();
            NavigationService.Navigate(maintenanceCreateEditPage);
        }

        private void EditMaintenanceButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var maintenanceId = (int)button.CommandParameter;
            var selectedMaintenance = _context.Maintenance.FirstOrDefault(m => m.MaintenanceID == maintenanceId);

            if (selectedMaintenance != null)
            {
                var maintenanceCreateEditPage = new MaintenanceCreateEditPage(selectedMaintenance);
                NavigationService.Navigate(maintenanceCreateEditPage);
            }
        }

        private void DeleteMaintenanceButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var maintenanceId = (int)button.CommandParameter;
            var selectedMaintenance = _context.Maintenance.FirstOrDefault(m => m.MaintenanceID == maintenanceId);

            if (selectedMaintenance != null)
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить эту запись об обслуживании?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    _context.Maintenance.Remove(selectedMaintenance);
                    _context.SaveChanges();
                    LoadMaintenance();
                }
            }
        }

        private void MaintenanceDataGrid_Sorting(object sender, DataGridSortingEventArgs e)
        {
            if (e.Column.SortMemberPath == "RoomNumber")
            {
                var direction = e.Column.SortDirection == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;
                e.Column.SortDirection = direction;

                var sortedItems = direction == ListSortDirection.Ascending
                    ? _context.Maintenance.OrderBy(m => m.RoomID).ToList()
                    : _context.Maintenance.OrderByDescending(m => m.RoomID).ToList();

                MaintenanceDataGrid.ItemsSource = sortedItems;
                e.Handled = true;
            }
        }
    }
}
