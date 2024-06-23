using RoadsideService.Data;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace RoadsideService.Views.Crud
{
    public partial class ShiftCreateEditPage : Page
    {
        private RoadsideServiceEntities _context;
        private Shifts _shift;

        public ShiftCreateEditPage()
        {
            InitializeComponent();
            _context = new RoadsideServiceEntities();
            LoadEmployees();
        }

        public ShiftCreateEditPage(Shifts shift) : this()
        {
            _shift = shift;
            LoadShiftDetails();
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


        private void LoadShiftDetails()
        {
            if (_shift != null)
            {
                EmployeeComboBox.SelectedValue = _shift.EmployeeID;
                ShiftDatePicker.SelectedDate = _shift.ShiftDate;
                StartTimeTextBox.Text = _shift.StartTime.ToString(@"hh\:mm");
                EndTimeTextBox.Text = _shift.EndTime.ToString(@"hh\:mm");
                IsHolidayCheckBox.IsChecked = _shift.IsHoliday;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (_shift == null)
            {
                _shift = new Shifts();
                _context.Shifts.Add(_shift);
            }

            _shift.EmployeeID = (int)EmployeeComboBox.SelectedValue;
            _shift.ShiftDate = (DateTime)ShiftDatePicker.SelectedDate;
            _shift.StartTime = TimeSpan.Parse(StartTimeTextBox.Text);
            _shift.EndTime = TimeSpan.Parse(EndTimeTextBox.Text);
            _shift.IsHoliday = IsHolidayCheckBox.IsChecked ?? false;

            _context.SaveChanges();

            NavigationService.GoBack();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}
