using RoadsideService.Data;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace RoadsideService.Views.Crud
{
    public partial class MaintenanceCreateEditPage : Page
    {
        private RoadsideServiceEntities _context;
        private Maintenance _maintenance;

        public MaintenanceCreateEditPage()
        {
            InitializeComponent();
            _context = new RoadsideServiceEntities();
            LoadRooms();
            LoadEmployees();
        }

        public MaintenanceCreateEditPage(Maintenance maintenance) : this()
        {
            _maintenance = maintenance;
            LoadMaintenanceDetails();
        }

        private void LoadRooms()
        {
            RoomComboBox.ItemsSource = _context.Rooms.ToList();
            RoomComboBox.DisplayMemberPath = "RoomNumber";
            RoomComboBox.SelectedValuePath = "RoomID";
        }

        private void LoadEmployees()
        {
            var employees = _context.Employees
                .Select(e => new
                {
                    e.EmployeeID,
                    FullName = e.LastName + " " + e.FirstName + " " + e.MiddleName
                })
                .ToList();

            EmployeeComboBox.ItemsSource = employees;
            EmployeeComboBox.DisplayMemberPath = "FullName";
            EmployeeComboBox.SelectedValuePath = "EmployeeID";
        }

        private void LoadMaintenanceDetails()
        {
            if (_maintenance != null)
            {
                DescriptionTextBox.Text = _maintenance.Description;
                MaintenanceDatePicker.SelectedDate = _maintenance.MaintenanceDate;
                CostTextBox.Text = _maintenance.Cost.ToString();
                RoomComboBox.SelectedValue = _maintenance.RoomID;
                EmployeeComboBox.SelectedValue = _maintenance.EmployeeID;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (_maintenance == null)
            {
                _maintenance = new Maintenance();
                _context.Maintenance.Add(_maintenance);
            }

            _maintenance.Description = DescriptionTextBox.Text;
            _maintenance.MaintenanceDate = (DateTime)MaintenanceDatePicker.SelectedDate;
            _maintenance.Cost = decimal.Parse(CostTextBox.Text);
            _maintenance.RoomID = (int)RoomComboBox.SelectedValue;
            _maintenance.EmployeeID = (int)EmployeeComboBox.SelectedValue;

            _context.SaveChanges();

            NavigationService.GoBack();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}
