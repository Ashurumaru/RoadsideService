using RoadsideService.Data;
using RoadsideService.Models;
using RoadsideService.Views.Crud;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace RoadsideService.Views.Pages
{
    public partial class ShiftManagementPage : Page
    {
        private RoadsideServiceEntities _context;

        public ShiftManagementPage()
        {
            InitializeComponent();
            _context = new RoadsideServiceEntities();
            LoadShifts();
        }

        private void LoadShifts(string searchTerm = "")
        {
            var shiftsQuery = _context.Shifts
                .Join(_context.Employees, shift => shift.EmployeeID, employee => employee.EmployeeID,
                    (shift, employee) => new ShiftModel
                    {
                        ShiftID = shift.ShiftID,
                        ShiftDate = shift.ShiftDate,
                        StartTime = shift.StartTime,
                        EndTime = shift.EndTime,
                        IsHoliday = shift.IsHoliday,
                        EmployeeName = employee.FirstName + " " + employee.LastName
                    })
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                shiftsQuery = shiftsQuery.Where(s => s.EmployeeName.Contains(searchTerm));
            }

            var shifts = shiftsQuery.ToList();
            ShiftsDataGrid.ItemsSource = shifts;
        }

        private void AddShiftButton_Click(object sender, RoutedEventArgs e)
        {
            var shiftCreateEditPage = new ShiftCreateEditPage();
            NavigationService.Navigate(shiftCreateEditPage);
        }

        private void EditShiftButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var shiftId = (int)button.CommandParameter;
            var selectedShift = _context.Shifts.FirstOrDefault(s => s.ShiftID == shiftId);

            if (selectedShift != null)
            {
                var shiftCreateEditPage = new ShiftCreateEditPage(selectedShift);
                NavigationService.Navigate(shiftCreateEditPage);
            }
        }

        private void DeleteShiftButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var shiftId = (int)button.CommandParameter;
            var selectedShift = _context.Shifts.FirstOrDefault(s => s.ShiftID == shiftId);

            if (selectedShift != null)
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить эту смену?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    _context.Shifts.Remove(selectedShift);
                    _context.SaveChanges();
                    LoadShifts();
                }
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchTerm = SearchTextBox.Text;
            LoadShifts(searchTerm);
        }
    }
}
