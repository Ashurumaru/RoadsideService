using RoadsideService.Data;
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

namespace RoadsideService.Views.Crud
{
    /// <summary>
    /// Логика взаимодействия для BookingCreateEditPage.xaml
    /// </summary>
    public partial class BookingCreateEditPage : Page
    {
        private RoadsideServiceEntities _context;
        private Bookings _booking;
        private Rooms _room;
        private Payments _selectedPayment;

        public BookingCreateEditPage()
        {
            InitializeComponent();
            _context = new RoadsideServiceEntities();
            LoadRooms();
            LoadCustomers();
            LoadBookingStatuses();
            LoadPaymentMethods();
        }

        public BookingCreateEditPage(Bookings booking) : this()
        {
            _booking = booking;
            LoadBookingDetails();
            LoadPayments();
        }

        public BookingCreateEditPage(Rooms room) : this()
        {
            _room = _context.Rooms.FirstOrDefault(u => u.RoomID == room.RoomID);
            RoomComboBox.SelectedValue = _room.RoomID;
            RoomComboBox.IsEnabled = false;
            StatusComboBox.SelectedIndex = 0;
        }

        private void LoadRooms()
        {
            RoomComboBox.ItemsSource = _context.Rooms.ToList();
            RoomComboBox.DisplayMemberPath = "RoomNumber";
            RoomComboBox.SelectedValuePath = "RoomID";
        }

        private void LoadCustomers()
        {
            var customers = _context.Customers
                .Select(c => new
                {
                    c.CustomerID,
                    FullName = c.FirstName + " " + c.MiddleName + " " + c.LastName
                })
                .ToList();

            CustomerComboBox.ItemsSource = customers;
            CustomerComboBox.DisplayMemberPath = "FullName";
            CustomerComboBox.SelectedValuePath = "CustomerID";
        }
        private void LoadPaymentMethods()
        {
            PaymentMethodComboBox.ItemsSource = _context.PaymentMethods.ToList();
            PaymentMethodComboBox.DisplayMemberPath = "MethodName";
            PaymentMethodComboBox.SelectedValuePath = "PaymentMethodID";
        }

        private void LoadBookingStatuses()
        {
            StatusComboBox.ItemsSource = _context.BookingStatus.ToList();
            StatusComboBox.DisplayMemberPath = "StatusName";
            StatusComboBox.SelectedValuePath = "StatusID";
        }
        private void LoadPayments()
        {
                PaymentsDataGrid.ItemsSource = null;
                PaymentsDataGrid.ItemsSource = _booking.Payments.ToList();

        }

        private void LoadBookingDetails()
        {
            if (_booking != null)
            {
                RoomComboBox.SelectedValue = _booking.RoomID;
                CustomerComboBox.SelectedValue = _booking.CustomerID;
                CheckInDatePicker.SelectedDate = _booking.CheckInDate;
                CheckOutDatePicker.SelectedDate = _booking.CheckOutDate;
                NumberOfGuestsTextBox.Text = _booking.NumberOfGuests.ToString();
                StatusComboBox.SelectedValue = _booking.StatusID;
                TotalPriceTextBox.Text = _booking.TotalPrice.ToString("F2");
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (_booking == null)
            {
                _booking = new Bookings();
                _context.Bookings.Add(_booking);
            }

            _booking.RoomID = (int)RoomComboBox.SelectedValue;
            _booking.CustomerID = (int)CustomerComboBox.SelectedValue;
            _booking.CheckInDate = (DateTime)CheckInDatePicker.SelectedDate;
            _booking.CheckOutDate = (DateTime)CheckOutDatePicker.SelectedDate;
            _booking.NumberOfGuests = int.Parse(NumberOfGuestsTextBox.Text);
            _booking.StatusID = (int)StatusComboBox.SelectedValue;
            _booking.TotalPrice = CalculateTotalPrice();

            _context.SaveChanges();

            NavigationService.GoBack();
        }

        private decimal CalculateTotalPrice()
        {
            var room = _context.Rooms.FirstOrDefault(r => r.RoomID == (int)RoomComboBox.SelectedValue);
            if (room == null) return 0;

            var checkInDate = CheckInDatePicker.SelectedDate;
            var checkOutDate = CheckOutDatePicker.SelectedDate;
            if (checkInDate == null || checkOutDate == null) return 0;

            var days = (checkOutDate.Value - checkInDate.Value).Days;
            return days * room.PricePerNight;
        }

        private void CheckInDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateTotalPrice();
        }

        private void CheckOutDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateTotalPrice();
        }

        private void UpdateTotalPrice()
        {
            TotalPriceTextBox.Text = CalculateTotalPrice().ToString("F2");
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
        private void AddPaymentButton_Click(object sender, RoutedEventArgs e)
        {
            if (_booking == null)
            {
                MessageBox.Show("Сначала сохраните бронирование, чтобы добавить оплату.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var payment = new Payments
            {
                BookingID = _booking.BookingID,
                PaymentDate = PaymentDatePicker.SelectedDate ?? DateTime.Now,
                Amount = decimal.Parse(PaymentAmountTextBox.Text),
                PaymentMethodID = (int)PaymentMethodComboBox.SelectedValue,
            };
            _context.Payments.Add(payment);
            _context.SaveChanges();
            LoadPayments();
        }

        private void EditPaymentButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var paymentId = (int)button.CommandParameter;
            _selectedPayment = _context.Payments.FirstOrDefault(p => p.PaymentID == paymentId);

            if (_selectedPayment != null)
            {
                PaymentAmountTextBox.Text = _selectedPayment.Amount.ToString("F2");
                PaymentDatePicker.SelectedDate = _selectedPayment.PaymentDate;
                PaymentMethodComboBox.SelectedValue = _selectedPayment.PaymentMethodID;
            }
        }

        private void DeletePaymentButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var paymentId = (int)button.CommandParameter;
            var payment = _context.Payments.FirstOrDefault(p => p.PaymentID == paymentId);

            if (payment != null)
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить эту оплату?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    _context.Payments.Remove(payment);
                    _context.SaveChanges();
                    LoadPayments();
                }
            }
        }
    }
}