using RoadsideService.Data;
using RoadsideService.Models;
using RoadsideService.Views.Pages;
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
    /// Логика взаимодействия для RoomCreateEditPage.xaml
    /// </summary>
    public partial class RoomCreateEditPage : Page
    {
        private RoadsideServiceEntities _context;
        private Rooms _room;

        public RoomCreateEditPage()
        {
            InitializeComponent();
            _context = new RoadsideServiceEntities();
            LoadRoomTypes();
        }

        public RoomCreateEditPage(Rooms room) : this()
        {
            _room = room;
            LoadRoomDetails();
        }

        private void LoadRoomTypes()
        {
            RoomTypeComboBox.ItemsSource = _context.RoomTypes.ToList();
            RoomTypeComboBox.DisplayMemberPath = "RoomType";
            RoomTypeComboBox.SelectedValuePath = "RoomTypeID";
        }

        private void LoadRoomDetails()
        {
            if (_room != null)
            {
                RoomNumberTextBox.Text = _room.RoomNumber;
                RoomPriceTextBox.Text = _room.PricePerNight.ToString();
                RoomTypeComboBox.SelectedValue = _room.RoomTypeID;
                MaxOccupancyTextBox.Text = _room.MaxOccupancy.ToString();
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (_room == null)
            {
                _room = new Rooms();
                _context.Rooms.Add(_room);
            }

            _room.RoomNumber = RoomNumberTextBox.Text;
            _room.PricePerNight = decimal.Parse(RoomPriceTextBox.Text);
            _room.RoomTypeID = (int)RoomTypeComboBox.SelectedValue;
            _room.MaxOccupancy = int.Parse(MaxOccupancyTextBox.Text);
            _room.IsCleaning = false;

            _context.SaveChanges();

            NavigationService.GoBack();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}