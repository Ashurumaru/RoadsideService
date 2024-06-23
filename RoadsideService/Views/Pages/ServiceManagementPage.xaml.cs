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
    /// Логика взаимодействия для ServiceManagementPage.xaml
    /// </summary>
    public partial class ServiceManagementPage : Page
    {
        private RoadsideServiceEntities _context;

        public ServiceManagementPage()
        {
            InitializeComponent();
            _context = new RoadsideServiceEntities();
            LoadServices();
        }

        private void LoadServices()
        {
            var services = _context.Services.ToList();
            ServicesItemsControl.ItemsSource = services;
        }

        private void AddServiceButton_Click(object sender, RoutedEventArgs e)
        {
            var serviceCreateEditPage = new ServiceCreateEditPage();
            NavigationService.Navigate(serviceCreateEditPage);
        }

        private void EditServiceButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var serviceId = (int)button.CommandParameter;
            var selectedService = _context.Services.FirstOrDefault(s => s.ServiceID == serviceId);

            if (selectedService != null)
            {
                var serviceCreateEditPage = new ServiceCreateEditPage(selectedService);
                NavigationService.Navigate(serviceCreateEditPage);
            }
        }

        private void DeleteServiceButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var serviceId = (int)button.CommandParameter;
            var selectedService = _context.Services.FirstOrDefault(s => s.ServiceID == serviceId);

            if (selectedService != null)
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить эту услугу?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    _context.Services.Remove(selectedService);
                    _context.SaveChanges();
                    LoadServices();
                }
            }
        }

        private void ServicesItemsControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }


    }
}