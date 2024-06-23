using RoadsideService.Data;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace RoadsideService.Views.Crud
{
    public partial class ClientCreateEditPage : Page
    {
        private RoadsideServiceEntities _context;
        private Customers _client;

        public ClientCreateEditPage()
        {
            InitializeComponent();
            _context = new RoadsideServiceEntities();
        }

        public ClientCreateEditPage(Customers client) : this()
        {
            _client = _context.Customers.FirstOrDefault(c => c.CustomerID == client.CustomerID);
            LoadClientDetails();
        }

        private void LoadClientDetails()
        {
            if (_client != null)
            {
                FirstNameTextBox.Text = _client.FirstName;
                MiddleNameTextBox.Text = _client.MiddleName;
                LastNameTextBox.Text = _client.LastName;
                PhoneTextBox.Text = _client.Phone;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (_client == null)
            {
                _client = new Customers();
                _context.Customers.Add(_client);
            }

            _client.FirstName = FirstNameTextBox.Text;
            _client.MiddleName = MiddleNameTextBox.Text;
            _client.LastName = LastNameTextBox.Text;
            _client.Phone = PhoneTextBox.Text;

            _context.SaveChanges();

            NavigationService.GoBack();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}
