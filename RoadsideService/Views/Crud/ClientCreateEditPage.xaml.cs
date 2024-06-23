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
            LoadGenderComboBox();
        }

        public ClientCreateEditPage(Customers client) : this()
        {
            _client = client;
            LoadClientDetails();
        }

        private void LoadGenderComboBox()
        {
            GenderComboBox.Items.Add(new ComboBoxItem { Content = "M" });
            GenderComboBox.Items.Add(new ComboBoxItem { Content = "F" });
        }

        private void LoadClientDetails()
        {
            if (_client != null)
            {
                FirstNameTextBox.Text = _client.FirstName;
                MiddleNameTextBox.Text = _client.MiddleName;
                LastNameTextBox.Text = _client.LastName;
                PhoneTextBox.Text = _client.Phone;
                EmailTextBox.Text = _client.Email;
                DateOfBirthPicker.SelectedDate = _client.DateOfBirth;

                foreach (ComboBoxItem item in GenderComboBox.Items)
                {
                    if (item.Content.ToString() == _client.Gender)
                    {
                        GenderComboBox.SelectedItem = item;
                        break;
                    }
                }
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
            _client.Email = EmailTextBox.Text;
            _client.DateOfBirth = DateOfBirthPicker.SelectedDate;

            if (GenderComboBox.SelectedItem != null)
            {
                _client.Gender = (GenderComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            }

            _context.SaveChanges();

            NavigationService.GoBack();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}
