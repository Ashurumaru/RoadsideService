using RoadsideService.Data;
using RoadsideService.Models;
using RoadsideService.Views.Crud;
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

namespace RoadsideService.Views.Pages
{
    /// <summary>
    /// Логика взаимодействия для MotelManagementPage.xaml
    /// </summary>
    public partial class MotelManagementPage : Page
    {
        private RoomModel selectedRoom;
        private RoadsideServiceEntities _context;

        public MotelManagementPage()
        {
            InitializeComponent();
            _context = new RoadsideServiceEntities();
            LoadRoomTypes();
            LoadRooms();
            this.Loaded += RoomManagementPage_Loaded; // Подписка на событие Loaded
        }

        private void RoomManagementPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.NavigationService != null)
            {
                this.NavigationService.Navigated += OnNavigated;
            }
            // Инициализация обработчиков событий для ComboBox
            FilterStatusComboBox.SelectionChanged += FilterComboBox_SelectionChanged;
            FilterTypeComboBox.SelectionChanged += FilterComboBox_SelectionChanged;
        }

        private void OnNavigated(object sender, NavigationEventArgs e)
        {
            LoadRooms();
        }

        private void LoadRoomTypes()
        {
            var roomTypes = _context.RoomTypes.ToList();
            FilterTypeComboBox.Items.Add(new ComboBoxItem { Content = "Все" });
            foreach (var roomType in roomTypes)
            {
                FilterTypeComboBox.Items.Add(new ComboBoxItem { Content = roomType.RoomType });
            }
            FilterTypeComboBox.SelectedIndex = 0;
        }

        private void LoadRooms()
        {
            var rooms = _context.Rooms
                .Include("RoomTypes")
                .Include("Bookings")
                .ToList();

            var roomList = rooms.Select(r => new RoomModel
            {
                RoomID = r.RoomID,
                RoomNumber = r.RoomNumber,
                PricePerNight = r.PricePerNight,
                RoomType = r.RoomTypes.RoomType,
                RoomTypeID = r.RoomTypeID,
                MaxOccupancy = r.MaxOccupancy,
                RoomStatus = GetRoomStatus(r),
                Residents = GetRoomResidents(r)
            }).ToList();

            RoomsItemControl.ItemsSource = roomList;
        }

        private string GetRoomStatus(Rooms room)
        {
            var today = DateTime.Today;
            var activeBooking = room.Bookings.FirstOrDefault(b => b.CheckInDate <= today && b.CheckOutDate >= today && (b.StatusID == 2 || b.StatusID == 3));

            if ((bool)room.IsCleaning)
            {
                return "На уборке";
            }

            if (activeBooking != null)
            {
                return "Занято"; // 2 - Подтверждено, 3 - В процессе
            }

            if (room.Bookings.Any(b => b.StatusID == 1 && b.CheckInDate > today))
            {
                return "На брони"; // 1 - Забронировано
            }

            return "Свободно";
        }

        private List<ResidentModel> GetRoomResidents(Rooms room)
        {
            var today = DateTime.Today;
            var activeBookings = room.Bookings
                .Where(b => b.CheckInDate <= today && b.CheckOutDate >= today && (b.StatusID == 2 || b.StatusID == 3))
                .ToList();

            var residents = new List<ResidentModel>();

            foreach (var booking in activeBookings)
            {
                var customer = _context.Customers.FirstOrDefault(c => c.CustomerID == booking.CustomerID);
                if (customer != null)
                {
                    residents.Add(new ResidentModel
                    {
                        FullName = $"{customer.FirstName} {customer.LastName}",
                        CheckInDate = booking.CheckInDate,
                        CheckOutDate = booking.CheckOutDate
                    });
                }
            }

            return residents;
        }

        private void ApplyFilter()
        {
            string statusFilter = (FilterStatusComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            string typeFilter = (FilterTypeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

            var rooms = _context.Rooms
                .Include("RoomTypes")
                .Include("Bookings")
                .ToList();

            var roomList = rooms.Select(r => new RoomModel
            {
                RoomID = r.RoomID,
                RoomNumber = r.RoomNumber,
                PricePerNight = r.PricePerNight,
                RoomType = r.RoomTypes.RoomType,
                MaxOccupancy = r.MaxOccupancy,
                RoomStatus = GetRoomStatus(r),
                Residents = GetRoomResidents(r)
            }).ToList();

            if (statusFilter != "Все")
            {
                roomList = roomList.Where(r => r.RoomStatus == statusFilter).ToList();
            }

            if (typeFilter != "Все")
            {
                roomList = roomList.Where(r => r.RoomType == typeFilter).ToList();
            }

            RoomsItemControl.ItemsSource = roomList;
        }

        private void FilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FilterStatusComboBox != null && FilterTypeComboBox != null)
            {
                ApplyFilter();
            }
        }

        private void RoomItem_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            selectedRoom = border.DataContext as RoomModel;

            var roomDetails = _context.Rooms
                .Include("RoomTypes")
                .Include("Bookings")
                .FirstOrDefault(r => r.RoomID == selectedRoom.RoomID);

            if (roomDetails != null)
            {
                RoomNumberTextBox.Text = roomDetails.RoomNumber;
                RoomPriceTextBox.Text = roomDetails.PricePerNight.ToString("C");
                RoomTypeTextBox.Text = roomDetails.RoomTypes.RoomType;
                RoomStatusTextBox.Text = selectedRoom.RoomStatus;

                var residents = GetRoomResidents(roomDetails);
                ResidentsItemControl.ItemsSource = residents;

                UpdateCleaningButtonText();
            }
        }

        private void ToggleCleaningStatus_Click(object sender, RoutedEventArgs e)
        {
            if (selectedRoom == null) return;

            var room = _context.Rooms
                              .Include("Bookings")
                              .FirstOrDefault(r => r.RoomID == selectedRoom.RoomID);
            if (room != null)
            {
                if ((bool)room.IsCleaning)
                {
                    room.IsCleaning = false;
                    selectedRoom.RoomStatus = "Свободно";
                }
                else
                {
                    room.IsCleaning = true;
                    selectedRoom.RoomStatus = "На уборке";
                }
                _context.SaveChanges();
            }

            UpdateCleaningButtonText();
            LoadRooms();
        }

        private void UpdateCleaningButtonText()
        {
            ToggleCleaningButton.Content = selectedRoom.RoomStatus == "На уборке" ? "Снять с уборки" : "Отправить на уборку";
        }

        private void AddRoom_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new RoomCreateEditPage());

        }

        private void EditRoom_Click(object sender, RoutedEventArgs e)
        {
            if (selectedRoom == null)
            {
                MessageBox.Show("Пожалуйста, выберите комнату для редактирования.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var room = _context.Rooms.FirstOrDefault(r => r.RoomID == selectedRoom.RoomID);
            if (room != null)
            {
                NavigationService.Navigate(new RoomCreateEditPage(room));
            }
        }

        private void BookRoom_Click(object sender, RoutedEventArgs e)
        {
            if (selectedRoom == null)
            {
                MessageBox.Show("Пожалуйста, выберите комнату для бронирования.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var room = _context.Rooms.Include("Bookings").FirstOrDefault(r => r.RoomID == selectedRoom.RoomID);
            if (room != null)
            {
                var today = DateTime.Today;
                var activeBooking = room.Bookings.FirstOrDefault(b => b.CheckOutDate >= today);

                if (activeBooking != null)
                {
                    NavigationService.Navigate(new BookingCreateEditPage(activeBooking));
                }
                else
                {
                    NavigationService.Navigate(new BookingCreateEditPage(room));
                }
            }
        }
        private void ManageBookings_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new BookingManagementPage());
        }
        private void DeleteRoom_Click(object sender, RoutedEventArgs e)
        {
            if (selectedRoom == null)
            {
                MessageBox.Show("Пожалуйста, выберите комнату для удаления.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var room = _context.Rooms
                .Include("Bookings")
                .FirstOrDefault(r => r.RoomID == selectedRoom.RoomID);

            if (room != null)
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить эту комнату и все связанные с ней записи?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    foreach (var booking in room.Bookings.ToList())
                    {
                        _context.Bookings.Remove(booking);
                    }

                    _context.Rooms.Remove(room);
                    _context.SaveChanges();
                    LoadRooms();
                }
            }
        }
    }
}
