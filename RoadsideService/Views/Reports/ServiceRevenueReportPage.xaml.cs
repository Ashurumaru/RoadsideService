﻿using Microsoft.Win32;
using RoadsideService.Data;
using RoadsideService.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using Xceed.Document.NET;
using Xceed.Words.NET;

namespace RoadsideService.Views.Reports
{
    /// <summary>
    /// Логика взаимодействия для ServiceRevenueReportPage.xaml
    /// </summary>
    public partial class ServiceRevenueReportPage : Page
    {
        private RoadsideServiceEntities _context;

        public ServiceRevenueReportPage()
        {
            InitializeComponent();
            _context = new RoadsideServiceEntities();
            LoadRevenueData();
        }

        private void LoadRevenueData()
        {
            DateTime? startDate = StartDatePicker.SelectedDate;
            DateTime? endDate = EndDatePicker.SelectedDate;

            var serviceRevenues = _context.ServicePayments
                .Join(_context.PaymentMethods, sp => sp.PaymentMethodID, pm => pm.PaymentMethodID, (sp, pm) => new { sp, pm })
                .Join(_context.ServiceOrders, sp_pm => sp_pm.sp.AppointmentID, so => so.OrderID, (sp_pm, so) => new { sp_pm, so })
                .Select(sp_pm_so => new RevenueModel
                {
                    Date = sp_pm_so.sp_pm.sp.PaymentDate,
                    CustomerName = sp_pm_so.so.CustomerName,
                    Description = "Доход от услуги",
                    Amount = sp_pm_so.sp_pm.sp.Amount,
                    PaymentMethod = sp_pm_so.sp_pm.pm.MethodName
                });

            if (startDate.HasValue)
            {
                serviceRevenues = serviceRevenues.Where(r => r.Date >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                serviceRevenues = serviceRevenues.Where(r => r.Date <= endDate.Value);
            }

            var revenueList = serviceRevenues.OrderBy(r => r.Date).ToList();
            RevenueDataGrid.ItemsSource = revenueList;
            CalculateTotalRevenue(revenueList);
        }

        private void CalculateTotalRevenue(List<RevenueModel> revenueList)
        {
            var totalRevenue = revenueList.Sum(r => r.Amount);
            TotalRevenueTextBox.Text = $"{totalRevenue:C}";
        }

        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadRevenueData();
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Word Document (*.docx)|*.docx",
                Title = "Сохранить отчет о доходах",
                FileName = $"ServiceRevenueReport_{DateTime.Now:yyyy-MM-dd}.docx"
            };

            if (saveFileDialog.ShowDialog() != true) return;

            var filePath = saveFileDialog.FileName;
            using (var doc = DocX.Create(filePath))
            {
                var title = doc.InsertParagraph("Отчет о доходах от услуг")
                               .FontSize(16)
                               .Bold()
                               .Alignment = Alignment.center;
                doc.InsertParagraph("\n");

                DateTime? startDate = StartDatePicker.SelectedDate;
                DateTime? endDate = EndDatePicker.SelectedDate;

                var periodInfo = doc.InsertParagraph($"Отчетный период: с {startDate:dd.MM.yyyy} по {endDate:dd.MM.yyyy}")
                                   .FontSize(12)
                                   .Alignment = Alignment.both;
                doc.InsertParagraph("\n");

                var serviceRevenues = _context.ServicePayments
                    .Join(_context.PaymentMethods, sp => sp.PaymentMethodID, pm => pm.PaymentMethodID, (sp, pm) => new { sp, pm })
                    .Join(_context.ServiceOrders, sp_pm => sp_pm.sp.AppointmentID, so => so.OrderID, (sp_pm, so) => new { sp_pm, so })
                    .Select(sp_pm_so => new RevenueModel
                    {
                        Date = sp_pm_so.sp_pm.sp.PaymentDate,
                        CustomerName = sp_pm_so.so.CustomerName,
                        Description = "Доход от услуги",
                        Amount = sp_pm_so.sp_pm.sp.Amount,
                        PaymentMethod = sp_pm_so.sp_pm.pm.MethodName
                    });

                if (startDate.HasValue)
                {
                    serviceRevenues = serviceRevenues.Where(r => r.Date >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    serviceRevenues = serviceRevenues.Where(r => r.Date <= endDate.Value);
                }

                var revenueList = serviceRevenues.OrderBy(r => r.Date).ToList();

                if (revenueList.Any())
                {
                    var table = doc.AddTable(revenueList.Count + 1, 5);
                    table.Rows[0].Cells[0].Paragraphs.First().Append("Дата").Bold();
                    table.Rows[0].Cells[1].Paragraphs.First().Append("Имя клиента").Bold();
                    table.Rows[0].Cells[2].Paragraphs.First().Append("Описание").Bold();
                    table.Rows[0].Cells[3].Paragraphs.First().Append("Сумма").Bold();
                    table.Rows[0].Cells[4].Paragraphs.First().Append("Метод оплаты").Bold();

                    for (int i = 0; i < revenueList.Count; i++)
                    {
                        var revenue = revenueList[i];
                        table.Rows[i + 1].Cells[0].Paragraphs.First().Append(revenue.Date.ToString("dd.MM.yyyy"));
                        table.Rows[i + 1].Cells[1].Paragraphs.First().Append(revenue.CustomerName);
                        table.Rows[i + 1].Cells[2].Paragraphs.First().Append(revenue.Description);
                        table.Rows[i + 1].Cells[3].Paragraphs.First().Append($"{revenue.Amount:F2} руб.");
                        table.Rows[i + 1].Cells[4].Paragraphs.First().Append(revenue.PaymentMethod);
                    }

                    doc.InsertTable(table);
                    table.Design = TableDesign.TableGrid;
                    table.Alignment = Alignment.center;
                    table.AutoFit = AutoFit.Contents;

                    var totalRevenue = revenueList.Sum(r => r.Amount);
                    doc.InsertParagraph($"\nОбщая выручка за указанный период: {totalRevenue:F2} руб.")
                        .FontSize(12)
                        .SpacingAfter(10)
                        .Bold();
                }
                else
                {
                    doc.InsertParagraph("За выбранный период доходы отсутствуют.")
                        .FontSize(12);
                }

                var signatureTable = doc.AddTable(5, 4);

                signatureTable.Rows[1].Cells[0].Paragraphs.First().Append("_________________________");
                signatureTable.Rows[1].Cells[1].Paragraphs.First().Append("_______________");
                signatureTable.Rows[1].Cells[2].Paragraphs.First().Append("__________________________");

                signatureTable.Rows[2].Cells[0].Paragraphs.First().Append("(должность)").Italic().FontSize(10);
                signatureTable.Rows[2].Cells[1].Paragraphs.First().Append("(личная подпись)").Italic().FontSize(10);
                signatureTable.Rows[2].Cells[2].Paragraphs.First().Append("(расшифровка подписи)").Italic().FontSize(10);

                signatureTable.Rows[3].Cells[0].Paragraphs.First().Append("_________________________");
                signatureTable.Rows[3].Cells[1].Paragraphs.First().Append("_______________");
                signatureTable.Rows[3].Cells[2].Paragraphs.First().Append("__________________________");

                signatureTable.Rows[4].Cells[0].Paragraphs.First().Append("(должность)").Italic().FontSize(10);
                signatureTable.Rows[4].Cells[1].Paragraphs.First().Append("(личная подпись)").Italic().FontSize(10);
                signatureTable.Rows[4].Cells[2].Paragraphs.First().Append("(расшифровка подписи)").Italic().FontSize(10);

                signatureTable.SetBorder(TableBorderType.InsideH, new Xceed.Document.NET.Border(BorderStyle.Tcbs_none, 0, 0, System.Drawing.Color.White));
                signatureTable.SetBorder(TableBorderType.InsideV, new Xceed.Document.NET.Border(BorderStyle.Tcbs_none, 0, 0, System.Drawing.Color.White));
                signatureTable.SetBorder(TableBorderType.Bottom, new Xceed.Document.NET.Border(BorderStyle.Tcbs_none, 0, 0, System.Drawing.Color.White));
                signatureTable.SetBorder(TableBorderType.Top, new Xceed.Document.NET.Border(BorderStyle.Tcbs_none, 0, 0, System.Drawing.Color.White));
                signatureTable.SetBorder(TableBorderType.Left, new Xceed.Document.NET.Border(BorderStyle.Tcbs_none, 0, 0, System.Drawing.Color.White));
                signatureTable.SetBorder(TableBorderType.Right, new Xceed.Document.NET.Border(BorderStyle.Tcbs_none, 0, 0, System.Drawing.Color.White));

                signatureTable.SetColumnWidth(0, 200);
                signatureTable.SetColumnWidth(1, 100);
                signatureTable.SetColumnWidth(2, 220);

                doc.InsertTable(signatureTable);

                doc.Save();
            }
            Process.Start("explorer.exe", filePath);
            MessageBox.Show("Отчет успешно сохранен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}