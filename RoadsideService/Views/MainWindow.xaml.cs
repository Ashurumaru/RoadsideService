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
using System.Windows.Shapes;

namespace RoadsideService.Views
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private void OrdersPage_Click(object sender, RoutedEventArgs e)
        {
            //PagesNavigation.Navigate(new OrdersPage());
        }

        private void MotelPage_Click(object sender, RoutedEventArgs e)
        {
            PagesNavigation.Navigate(new MotelManagementPage());
        }

        private void AutoServicePage_Click(object sender, RoutedEventArgs e)
        {
            PagesNavigation.Navigate(new AutoServiceManagementPage());
        }

        private void RoomMotelPage_Click(object sender, RoutedEventArgs e)
        {
            //PagesNavigation.Navigate(new RoomManagementPage());
        }

        private void CustomerPage_Click(object sender, RoutedEventArgs e)
        {
            PagesNavigation.Navigate(new ClientManagementPage());
        }

        private void EmployeePage_Click(object sender, RoutedEventArgs e)
        {
            PagesNavigation.Navigate(new EmployeeManagementPage());
        }

        private void PersonalAccountPage_Click(object sender, RoutedEventArgs e)
        {
            //PagesNavigation.Navigate(new PersonalAccountPage());
        }

        private void ReportPage_Click(object sender, RoutedEventArgs e)
        {
            //PagesNavigation.Navigate(new ReportPage());
        }
        private void btn_close_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void btn_maximize_Click(object sender, RoutedEventArgs e)
        {
            SwitchWindowState();
        }

        private void btn_minimize_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this).WindowState = WindowState.Minimized;
        }

        private void DockPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount >= 2)
            {
                SwitchWindowState();
                return;
            }

            if (Window.GetWindow(this).WindowState == WindowState.Maximized)
            {
                return;
            }
            else
            {
                if (e.LeftButton == MouseButtonState.Pressed) Window.GetWindow(this).DragMove();
            }
        }

        private void MaximizeWindow()
        {
            Window.GetWindow(this).WindowState = WindowState.Maximized;
        }

        private void RestoreWindow()
        {
            Window.GetWindow(this).WindowState = WindowState.Normal;
        }

        private void SwitchWindowState()
        {
            if (Window.GetWindow(this).WindowState == WindowState.Normal) MaximizeWindow();
            else RestoreWindow();
        }


    }
}
