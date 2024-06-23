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
    /// Логика взаимодействия для ServiceCreateEditPage.xaml
    /// </summary>
    public partial class ServiceCreateEditPage : Page
    {
        private RoadsideServiceEntities _context;
        private Services _service;

        public ServiceCreateEditPage()
        {
            InitializeComponent();
            _context = new RoadsideServiceEntities();
        }

        public ServiceCreateEditPage(Services service) : this()
        {
            _service = service;
            LoadServiceDetails();
        }

        private void LoadServiceDetails()
        {
            if (_service != null)
            {
                ServiceNameTextBox.Text = _service.ServiceName;
                DescriptionTextBox.Text = _service.Description;
                PriceTextBox.Text = _service.Price.ToString("F2");
                DurationTextBox.Text = _service.Duration.ToString();
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (_service == null)
            {
                _service = new Services();
                _context.Services.Add(_service);
            }
            else
            {
                _context.Entry(_service).State = System.Data.Entity.EntityState.Modified; 
            }

            _service.ServiceName = ServiceNameTextBox.Text;
            _service.Description = DescriptionTextBox.Text;
            _service.Price = decimal.Parse(PriceTextBox.Text);
            _service.Duration = int.Parse(DurationTextBox.Text);

            _context.SaveChanges();
            NavigationService.GoBack();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}