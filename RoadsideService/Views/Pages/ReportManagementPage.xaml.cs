using RoadsideService.Views.Reports;
using System.Windows;
using System.Windows.Controls;

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