using RoadsideService.Data;
using RoadsideService.Views.Crud;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace RoadsideService.Views.Pages
{
    public partial class AutoServiceManagementPage : Page
    {
        private RoadsideServiceEntities _context;

        public AutoServiceManagementPage()
        {
            InitializeComponent();
            _context = new RoadsideServiceEntities();
            LoadCustomerFilter();
            LoadServiceOrders();
        }

        private void LoadCustomerFilter()
        {
            CustomerFilterComboBox.Items.Add("Все");
            var customers = _context.ServiceOrders
                .Select(o => o.CustomerName)
                .Distinct()
                .ToList();

            foreach (var customer in customers)
            {
                CustomerFilterComboBox.Items.Add(customer);
            }
            CustomerFilterComboBox.SelectedIndex = 0;
        }

        private void LoadServiceOrders()
        {
            var serviceOrders = _context.ServiceOrders
                .Select(o => new
                {
                    o.OrderID,
                    o.CustomerName,
                    o.CustomerPhone,
                    o.OrderDate,
                    TotalPrice = o.ServiceOrderDetails.Sum(d => (decimal?)d.Services.Price) ?? 0
                }).ToList()
                .Select(o => new
                {
                    o.OrderID,
                    o.CustomerName,
                    o.CustomerPhone,
                    o.OrderDate,
                    TotalPrice = $"{o.TotalPrice}"
                }).ToList();

            ServiceOrdersDataGrid.ItemsSource = serviceOrders;
        }

        private void FilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            string customerFilter = CustomerFilterComboBox.SelectedItem as string;

            var serviceOrdersQuery = _context.ServiceOrders.AsQueryable();

            if (customerFilter != "Все")
            {
                serviceOrdersQuery = serviceOrdersQuery.Where(o => o.CustomerName == customerFilter);
            }

            var serviceOrders = serviceOrdersQuery
                .Select(o => new
                {
                    o.OrderID,
                    o.CustomerName,
                    o.CustomerPhone,
                    o.OrderDate,
                    TotalPrice = o.ServiceOrderDetails.Sum(d => (decimal?)d.Services.Price) ?? 0
                }).ToList()
                .Select(o => new
                {
                    o.OrderID,
                    o.CustomerName,
                    o.CustomerPhone,
                    o.OrderDate,
                    TotalPrice = $"{o.TotalPrice}"
                }).ToList();

            ServiceOrdersDataGrid.ItemsSource = serviceOrders;
        }

        private void AddServiceOrderButton_Click(object sender, RoutedEventArgs e)
        {
            var serviceOrderCreateEditPage = new OrderCreateEditPage();
            NavigationService.Navigate(serviceOrderCreateEditPage);
        }

        private void EditServiceOrderButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var orderId = (int)button.CommandParameter;
            var selectedServiceOrder = _context.ServiceOrders.FirstOrDefault(o => o.OrderID == orderId);

            if (selectedServiceOrder != null)
            {
                var serviceOrderCreateEditPage = new OrderCreateEditPage(selectedServiceOrder);
                NavigationService.Navigate(serviceOrderCreateEditPage);
            }
        }

        private void DeleteServiceOrderButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var orderId = (int)button.CommandParameter;
            var selectedServiceOrder = _context.ServiceOrders.FirstOrDefault(o => o.OrderID == orderId);

            if (selectedServiceOrder != null)
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить этот заказ и все связанные с ним записи?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    var details = selectedServiceOrder.ServiceOrderDetails.ToList();
                    foreach (var detail in details)
                    {
                        _context.ServiceOrderDetails.Remove(detail);
                    }

                    var payments = selectedServiceOrder.ServicePayments.ToList();
                    foreach (var payment in payments)
                    {
                        _context.ServicePayments.Remove(payment);
                    }

                    _context.ServiceOrders.Remove(selectedServiceOrder);
                    _context.SaveChanges();

                    LoadServiceOrders();
                }
            }
        }

        private void ServiceOrdersDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void ManagementServiceOrderButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new ServiceManagementPage());
        }
    }
}
