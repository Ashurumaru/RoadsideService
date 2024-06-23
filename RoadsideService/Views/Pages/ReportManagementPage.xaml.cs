using RoadsideService.Views.Reports;
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
    /// Логика взаимодействия для ReportManagementPage.xaml
    /// </summary>
    public partial class ReportManagementPage : Page
    {
        public ReportManagementPage()
        {
            InitializeComponent();
            ReportFrame.Navigate(new BasePage());

        }

        private void BookingRevenueReportButton_Click(object sender, RoutedEventArgs e)
        {
            ReportFrame.Navigate(new BookingRevenueReportPage());
        }

        private void ServiceRevenueReportButton_Click(object sender, RoutedEventArgs e)
        {
            ReportFrame.Navigate(new ServiceRevenueReportPage());
        }

        private void RoomOccupancyReportButton_Click(object sender, RoutedEventArgs e)
        {
            ReportFrame.Navigate(new RoomOccupancyReportPage());
        }
    }
}