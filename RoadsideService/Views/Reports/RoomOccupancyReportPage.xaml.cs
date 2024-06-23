using Microsoft.Win32;
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
    /// Логика взаимодействия для RoomOccupancyReportPage.xaml
    /// </summary>
    public partial class RoomOccupancyReportPage : Page
    {
        private RoadsideServiceEntities _context;

        public RoomOccupancyReportPage()
        {
            InitializeComponent();
            _context = new RoadsideServiceEntities();
            SelectedDatePicker.SelectedDate = DateTime.Today;
            LoadCurrentOccupancyData(DateTime.Today);
        }

        private void LoadCurrentOccupancyData(DateTime selectedDate)
        {
            var roomOccupancy = _context.Bookings
                .Where(b => b.CheckInDate <= selectedDate && b.CheckOutDate >= selectedDate && (b.StatusID == 2 || b.StatusID == 3 || b.StatusID == 5) )
                .Join(_context.Rooms, b => b.RoomID, r => r.RoomID, (b, r) => new { b, r })
                .Join(_context.Customers, br => br.b.CustomerID, c => c.CustomerID, (br, c) => new { br, c })
                .Join(_context.BookingStatus, brc => brc.br.b.StatusID, bs => bs.StatusID, (brc, bs) => new RoomOccupancyModel
                {
                    RoomNumber = brc.br.r.RoomNumber,
                    Status = bs.StatusName,
                    CustomerName = brc.c.FirstName + " " + brc.c.LastName,
                    CheckInDate = brc.br.b.CheckInDate,
                    CheckOutDate = brc.br.b.CheckOutDate
                })
                .ToList();

            var occupancyList = roomOccupancy.OrderBy(r => r.RoomNumber).ToList();
            OccupancyDataGrid.ItemsSource = occupancyList;
            CalculateTotalOccupancy(occupancyList);
        }

        private void CalculateTotalOccupancy(List<RoomOccupancyModel> occupancyList)
        {
            var totalOccupied = occupancyList.Count;
            TotalOccupiedTextBox.Text = totalOccupied.ToString();
        }

        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectedDatePicker.SelectedDate.HasValue)
            {
                LoadCurrentOccupancyData(SelectedDatePicker.SelectedDate.Value);
            }
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            DateTime selectedDate = SelectedDatePicker.SelectedDate ?? DateTime.Today;

            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Word Document (*.docx)|*.docx",
                Title = "Сохранить отчет",
                FileName = $"RoomReport_{selectedDate:yyyy-MM-dd}.docx"
            };

            if (saveFileDialog.ShowDialog() != true) return;

            var filePath = saveFileDialog.FileName;
            using (var doc = DocX.Create(filePath))
            {
                var title = doc.InsertParagraph("Отчет об занятости комнат")
                               .FontSize(16)
                               .Bold()
                               .Alignment = Alignment.center;
                doc.InsertParagraph("\n");


                var periodInfo = doc.InsertParagraph($"Отчет за: {selectedDate:dd.MM.yyyy}")
                                   .FontSize(12)
                                   .Alignment = Alignment.both;
                doc.InsertParagraph("\n");

                var roomOccupancy = _context.Bookings
                    .Where(b => b.CheckInDate <= selectedDate && b.CheckOutDate >= selectedDate && (b.StatusID == 2 || b.StatusID == 3 || b.StatusID == 5))
                    .Join(_context.Rooms, b => b.RoomID, r => r.RoomID, (b, r) => new { b, r })
                    .Join(_context.Customers, br => br.b.CustomerID, c => c.CustomerID, (br, c) => new { br, c })
                    .Join(_context.BookingStatus, brc => brc.br.b.StatusID, bs => bs.StatusID, (brc, bs) => new RoomOccupancyModel
                    {
                        RoomNumber = brc.br.r.RoomNumber,
                        Status = bs.StatusName,
                        CustomerName = brc.c.FirstName + " " + brc.c.LastName,
                        CheckInDate = brc.br.b.CheckInDate,
                        CheckOutDate = brc.br.b.CheckOutDate
                    })
                    .ToList();

                var occupancyList = roomOccupancy.OrderBy(r => r.RoomNumber).ToList();

                if (occupancyList.Any())
                {
                    var table = doc.AddTable(occupancyList.Count + 1, 5);
                    table.Rows[0].Cells[0].Paragraphs.First().Append("Номер комнаты").Bold();
                    table.Rows[0].Cells[1].Paragraphs.First().Append("Статус").Bold();
                    table.Rows[0].Cells[2].Paragraphs.First().Append("Имя клиента").Bold();
                    table.Rows[0].Cells[3].Paragraphs.First().Append("Дата заезда").Bold();
                    table.Rows[0].Cells[4].Paragraphs.First().Append("Дата выезда").Bold();

                    for (int i = 0; i < occupancyList.Count; i++)
                    {
                        var occupancy = occupancyList[i];
                        table.Rows[i + 1].Cells[0].Paragraphs.First().Append(occupancy.RoomNumber);
                        table.Rows[i + 1].Cells[1].Paragraphs.First().Append(occupancy.Status);
                        table.Rows[i + 1].Cells[2].Paragraphs.First().Append(occupancy.CustomerName);
                        table.Rows[i + 1].Cells[3].Paragraphs.First().Append(occupancy.CheckInDate.ToString("dd.MM.yyyy"));
                        table.Rows[i + 1].Cells[4].Paragraphs.First().Append(occupancy.CheckOutDate.ToString("dd.MM.yyyy"));
                    }

                    doc.InsertTable(table);
                    table.Design = TableDesign.TableGrid;
                    table.Alignment = Alignment.center;
                    table.AutoFit = AutoFit.Contents;

                    var totalOccupied = occupancyList.Count;
                    doc.InsertParagraph($"\nОбщее количество занятых номеров: {totalOccupied}")
                        .FontSize(12)
                        .SpacingAfter(10)
                        .Bold();
                }
                else
                {
                    doc.InsertParagraph("На выбранную дату номера не заняты.")
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
