using RoadsideService.Data;
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
    /// Логика взаимодействия для ClientManagementPage.xaml
    /// </summary>
    public partial class ClientManagementPage : Page
    {
        private RoadsideServiceEntities _context;

        public ClientManagementPage()
        {
            InitializeComponent();
            _context = new RoadsideServiceEntities();
            LoadClients();
        }

        private void LoadClients(string searchTerm = "")
        {
            var clientsQuery = _context.Customers.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                clientsQuery = clientsQuery.Where(c => c.FirstName.Contains(searchTerm) ||
                                                       c.LastName.Contains(searchTerm) ||
                                                       c.MiddleName.Contains(searchTerm) ||
                                                       c.Email.Contains(searchTerm) ||
                                                       c.Phone.Contains(searchTerm));
            }

            var clients = clientsQuery.ToList();
            ClientsItemsControl.ItemsSource = clients;
        }

        private void AddClientButton_Click(object sender, RoutedEventArgs e)
        {
            var clientCreateEditPage = new ClientCreateEditPage();
            NavigationService.Navigate(clientCreateEditPage);
        }

        private void EditClientButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var clientId = (int)button.CommandParameter;
            var selectedClient = _context.Customers.FirstOrDefault(c => c.CustomerID == clientId);

            if (selectedClient != null)
            {
                var clientCreateEditPage = new ClientCreateEditPage(selectedClient);
                NavigationService.Navigate(clientCreateEditPage);
            }
        }

        private void DeleteClientButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var clientId = (int)button.CommandParameter;
            var selectedClient = _context.Customers.FirstOrDefault(c => c.CustomerID == clientId);

            if (selectedClient != null)
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить этого клиента?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    _context.Customers.Remove(selectedClient);
                    _context.SaveChanges();
                    LoadClients();
                }
            }
        }
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchTerm = SearchTextBox.Text;
            LoadClients(searchTerm);
        }
    }
}