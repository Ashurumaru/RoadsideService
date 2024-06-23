using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using RoadsideService.Data;
using RoadsideService.Views.Crud;

namespace RoadsideService.Views.Pages
{
    public partial class BookingManagementPage : Page
    {
private RoadsideServiceEntities _context;

        public BookingManagementPage()
        {
            InitializeComponent();
            _context = new RoadsideServiceEntities();
            LoadRoomFilter();
            LoadStatusFilter();
            LoadBookings();
            this.Loaded += BookingManagementPage_Loaded; 
        }

        private void BookingManagementPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadBookings();
            if (this.NavigationService != null)
            {
                this.NavigationService.Navigated += OnNavigated;
            }
        }

        private void LoadRoomFilter()
        {
            RoomFilterComboBox.Items.Add("Все");
            var rooms = _context.Rooms.Select(r => r.RoomNumber).ToList();
            foreach (var room in rooms)
            {
                RoomFilterComboBox.Items.Add(room);
            }
            RoomFilterComboBox.SelectedIndex = 0;
        }

        private void LoadStatusFilter()
        {
            StatusFilterComboBox.Items.Add("Все");
            var statuses = _context.BookingStatus.Select(s => s.StatusName).ToList();
            foreach (var status in statuses)
            {
                StatusFilterComboBox.Items.Add(status);
            }
            StatusFilterComboBox.SelectedIndex = 0;
        }

        private void LoadBookings()
        {
            var bookings = _context.Bookings
                .Include("Rooms")
                .Include("Customers")
                .Include("BookingStatus")
                .ToList()
                .Select(b => new
                {
                    b.BookingID,
                    RoomNumber = b.Rooms.RoomNumber,
                    CustomerFullName = $"{b.Customers.FirstName} {b.Customers.MiddleName} {b.Customers.LastName}",
                    b.CheckInDate,
                    b.CheckOutDate,
                    StatusName = b.BookingStatus.StatusName
                }).ToList();

            BookingsDataGrid.ItemsSource = bookings;
        }

        private void FilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            string roomFilter = RoomFilterComboBox.SelectedItem as string;
            string statusFilter = StatusFilterComboBox.SelectedItem as string;

            var bookings = _context.Bookings
                .Include("Rooms")
                .Include("Customers")
                .Include("BookingStatus")
                .ToList()
                .Select(b => new
                {
                    b.BookingID,
                    RoomNumber = b.Rooms.RoomNumber,
                    CustomerFullName = $"{b.Customers.FirstName} {b.Customers.MiddleName} {b.Customers.LastName}",
                    b.CheckInDate,
                    b.CheckOutDate,
                    StatusName = b.BookingStatus.StatusName
                }).ToList();

            if (roomFilter != "Все")
            {
                bookings = bookings.Where(b => b.RoomNumber == roomFilter).ToList();
            }

            if (statusFilter != "Все")
            {
                bookings = bookings.Where(b => b.StatusName == statusFilter).ToList();
            }

            BookingsDataGrid.ItemsSource = bookings;
        }

        private void AddBookingButton_Click(object sender, RoutedEventArgs e)
        {
            var bookingCreateEditPage = new BookingCreateEditPage();
            NavigationService.Navigate(bookingCreateEditPage);
        }

        private void EditBookingButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var bookingId = (int)button.CommandParameter;
            var selectedBooking = _context.Bookings
                .Include("Rooms")
                .Include("Customers")
                .Include("BookingStatus")
                .FirstOrDefault(b => b.BookingID == bookingId);

            if (selectedBooking != null)
            {
                var bookingCreateEditPage = new BookingCreateEditPage(selectedBooking);
                NavigationService.Navigate(bookingCreateEditPage);
            }
        }

        private void DeleteBookingButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var bookingId = (int)button.CommandParameter;
            var selectedBooking = _context.Bookings
                                          .Include("Payments")
                                          .FirstOrDefault(b => b.BookingID == bookingId);

            if (selectedBooking != null)
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить это бронирование и все связанные с ним записи?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    var payments = selectedBooking.Payments.ToList();
                    foreach (var payment in payments)
                    {
                        _context.Payments.Remove(payment);
                    }

                    _context.Bookings.Remove(selectedBooking);
                    _context.SaveChanges();

                    LoadBookings();
                }
            }
        }


        private void BookingsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void OnNavigated(object sender, NavigationEventArgs e)
        {
            LoadBookings();
        }
    }
}