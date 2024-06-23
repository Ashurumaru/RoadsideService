﻿using RoadsideService.Data;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace RoadsideService.Views.Crud
{
    /// <summary>
    /// Логика взаимодействия для OrderCreateEditPage.xaml
    /// </summary>
    public partial class OrderCreateEditPage : Page
    {
        private RoadsideServiceEntities _context;
        private ServiceOrders _serviceOrder;
        private ServicePayments _selectedPayment;

        public OrderCreateEditPage()
        {
            InitializeComponent();
            _context = new RoadsideServiceEntities();
            LoadStatuses();
            LoadServices();
            LoadPaymentMethods();
            LoadPaymentStatuses();
        }

        public OrderCreateEditPage(ServiceOrders serviceOrder) : this()
        {
            _serviceOrder = serviceOrder;
            LoadServiceOrderDetails();
            LoadPayments();
        }

        private void LoadStatuses()
        {
            StatusComboBox.ItemsSource = _context.ServiceStatus.ToList();
            StatusComboBox.DisplayMemberPath = "StatusName";
            StatusComboBox.SelectedValuePath = "ServiceStatusID";
        }

        private void LoadServices()
        {
            ServiceComboBox.ItemsSource = _context.Services.ToList();
            ServiceComboBox.DisplayMemberPath = "ServiceName";
            ServiceComboBox.SelectedValuePath = "ServiceID";
        }

        private void LoadPaymentMethods()
        {
            PaymentMethodComboBox.ItemsSource = _context.PaymentMethods.ToList();
            PaymentMethodComboBox.DisplayMemberPath = "MethodName";
            PaymentMethodComboBox.SelectedValuePath = "PaymentMethodID";
        }

        private void LoadPaymentStatuses()
        {
            PaymentStatusComboBox.ItemsSource = _context.PaymentStatus.ToList();
            PaymentStatusComboBox.DisplayMemberPath = "StatusName";
            PaymentStatusComboBox.SelectedValuePath = "PaymentStatusID";
        }

        private void LoadServiceOrderDetails()
        {
            if (_serviceOrder != null)
            {
                CustomerNameTextBox.Text = _serviceOrder.CustomerName;
                CustomerPhoneTextBox.Text = _serviceOrder.CustomerPhone;
                OrderDatePicker.SelectedDate = _serviceOrder.OrderDate;
                StatusComboBox.SelectedValue = _serviceOrder.StatusID;
                TotalPriceTextBox.Text = _serviceOrder.ServiceOrderDetails.Sum(detail => detail.Services.Price).ToString("F2");

                var serviceOrderDetails = _context.ServiceOrderDetails
                    .Where(d => d.OrderID == _serviceOrder.OrderID)
                    .Select(d => new
                    {
                        d.DetailID,
                        ServiceName = d.Services.ServiceName,
                        d.Services.Price
                    }).ToList();

                ServiceOrderDetailsDataGrid.ItemsSource = serviceOrderDetails;
            }
        }

        private void LoadPayments()
        {
            if (_serviceOrder != null)
            {
                var payments = _context.ServicePayments
                    .Where(p => p.AppointmentID == _serviceOrder.OrderID)
                    .Select(p => new
                    {
                        p.PaymentID,
                        p.PaymentDate,
                        p.Amount,
                        MethodName = p.PaymentMethods.MethodName,
                        StatusName = p.PaymentStatus.StatusName
                    }).ToList();

                PaymentsDataGrid.ItemsSource = payments;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (_serviceOrder == null)
            {
                _serviceOrder = new ServiceOrders();
                _context.ServiceOrders.Add(_serviceOrder);
            }

            _serviceOrder.CustomerName = CustomerNameTextBox.Text;
            _serviceOrder.CustomerPhone = CustomerPhoneTextBox.Text;
            _serviceOrder.OrderDate = (DateTime)OrderDatePicker.SelectedDate;
            _serviceOrder.StatusID = (int)StatusComboBox.SelectedValue;

            _context.SaveChanges();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void AddServiceButton_Click(object sender, RoutedEventArgs e)
        {
            if (_serviceOrder == null)
            {
                MessageBox.Show("Сначала сохраните заказ, чтобы добавить услугу.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var serviceId = (int)ServiceComboBox.SelectedValue;
            var service = _context.Services.FirstOrDefault(s => s.ServiceID == serviceId);

            if (service != null)
            {
                var detail = new ServiceOrderDetails
                {
                    OrderID = _serviceOrder.OrderID,
                    ServiceID = service.ServiceID
                };
                _context.ServiceOrderDetails.Add(detail);
                _context.SaveChanges();
                LoadServiceOrderDetails();
            }
        }

        private void DeleteServiceDetailButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var detailId = (int)button.CommandParameter;
            var detail = _context.ServiceOrderDetails.FirstOrDefault(d => d.DetailID == detailId);

            if (detail != null)
            {
                _context.ServiceOrderDetails.Remove(detail);
                _context.SaveChanges();
                LoadServiceOrderDetails();
            }
        }

        private void AddPaymentButton_Click(object sender, RoutedEventArgs e)
        {
            if (_serviceOrder == null)
            {
                MessageBox.Show("Сначала сохраните заказ, чтобы добавить оплату.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var payment = new ServicePayments
            {
                AppointmentID = _serviceOrder.OrderID,
                PaymentDate = PaymentDatePicker.SelectedDate ?? DateTime.Now,
                Amount = decimal.Parse(PaymentAmountTextBox.Text),
                PaymentMethodID = (int)PaymentMethodComboBox.SelectedValue,
                PaymentStatusID = (int)PaymentStatusComboBox.SelectedValue
            };
            _context.ServicePayments.Add(payment);
            _context.SaveChanges();
            LoadPayments();
        }

        private void EditPaymentButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var paymentId = (int)button.CommandParameter;
            _selectedPayment = _context.ServicePayments.FirstOrDefault(p => p.PaymentID == paymentId);

            if (_selectedPayment != null)
            {
                PaymentAmountTextBox.Text = _selectedPayment.Amount.ToString("F2");
                PaymentDatePicker.SelectedDate = _selectedPayment.PaymentDate;
                PaymentMethodComboBox.SelectedValue = _selectedPayment.PaymentMethodID;
                PaymentStatusComboBox.SelectedValue = _selectedPayment.PaymentStatusID;
            }
        }

        private void DeletePaymentButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var paymentId = (int)button.CommandParameter;
            var payment = _context.ServicePayments.FirstOrDefault(p => p.PaymentID == paymentId);

            if (payment != null)
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить эту оплату?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    _context.ServicePayments.Remove(payment);
                    _context.SaveChanges();
                    LoadPayments();
                }
            }
        }
    }
}