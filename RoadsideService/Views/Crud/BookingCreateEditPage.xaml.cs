using RoadsideService.Data;
using System;
using System.Collections.ObjectModel; // Добавлено для ObservableCollection
using System.Data.Entity; // Добавлено для Include

using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace RoadsideService.Views.Crud
{
    /// <summary>
    /// Логика взаимодействия для BookingCreateEditPage.xaml
    /// </summary>
    public partial class BookingCreateEditPage : Page
    {
        private RoadsideServiceEntities _context;
        private Bookings _booking; // Текущее редактируемое/создаваемое бронирование
        private Rooms _room; // Если создаем из конкретной комнаты
        private Payments _selectedPaymentForEdit; // Платеж, выбранный для редактирования
        private bool _isEditingPayment = false; // Флаг режима редактирования платежа

        // Используем ObservableCollection для автоматического обновления DataGrid
        public ObservableCollection<Payments> BookingPayments { get; set; }

        // Конструктор для нового бронирования
        public BookingCreateEditPage()
        {
            InitializeComponent();
            _context = new RoadsideServiceEntities();
            BookingPayments = new ObservableCollection<Payments>(); // Инициализация коллекции
            PaymentsDataGrid.ItemsSource = BookingPayments; // Привязка коллекции к DataGrid
            LoadComboBoxData();
            // Блокируем вкладку оплаты для нового, еще не сохраненного бронирования
            PaymentTabItem.IsEnabled = false;
        }

        // Конструктор для редактирования существующего бронирования
        public BookingCreateEditPage(Bookings booking) : this()
        {
            _booking = _context.Bookings
                               .Include(b => b.Payments.Select(p => p.PaymentMethods)) // Включаем связанные платежи и их методы
                               .FirstOrDefault(b => b.BookingID == booking.BookingID);

            if (_booking == null)
            {
                MessageBox.Show("Не удалось загрузить бронирование.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                // Возможно, стоит вернуться назад или закрыть страницу
                // NavigationService?.GoBack();
                return;
            }

            LoadBookingDetails();
            LoadPayments();
            // Разблокируем вкладку оплаты для существующего бронирования
            PaymentTabItem.IsEnabled = true;
        }

        // Конструктор для создания бронирования из конкретной комнаты
        public BookingCreateEditPage(Rooms room) : this()
        {
            // Находим комнату в текущем контексте
            _room = _context.Rooms.Local.FirstOrDefault(r => r.RoomID == room.RoomID);
            if (_room == null)
            {
                _context.Rooms.Attach(room); // Присоединяем, если не отслеживается
                _room = room;
            }

            // Устанавливаем комнату по умолчанию и блокируем выбор
            RoomComboBox.SelectedValue = _room?.RoomID;
            RoomComboBox.IsEnabled = false;

            // Устанавливаем статус по умолчанию (например, "Забронировано")
            // Убедитесь, что такой статус существует и его ID корректен
            var defaultStatus = _context.BookingStatus.FirstOrDefault(bs => bs.StatusName == "Забронировано"); // Или другой статус по умолчанию
            if (defaultStatus != null)
            {
                StatusComboBox.SelectedValue = defaultStatus.StatusID;
            }
            else if (StatusComboBox.Items.Count > 0)
            {
                StatusComboBox.SelectedIndex = 0; // Выбираем первый доступный статус
            }
            // Вкладка оплаты остается заблокированной до первого сохранения
            PaymentTabItem.IsEnabled = false;
        }

        // --- Методы загрузки данных для ComboBox ---
        private void LoadComboBoxData()
        {
            LoadRooms();
            LoadCustomers();
            LoadBookingStatuses();
            LoadPaymentMethods();
        }

        private void LoadRooms()
        {
            try
            {
                RoomComboBox.ItemsSource = _context.Rooms.ToList();
                // DisplayMemberPath и SelectedValuePath уже установлены в XAML
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки комнат: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadCustomers()
        {
            try
            {
                // Загружаем клиентов и формируем полное имя
                var customers = _context.Customers
                    .Select(c => new
                    {
                        c.CustomerID,
                        // Обработка возможного отсутствия отчества
                        FullName = (c.FirstName ?? "") + " " + (c.MiddleName ?? "").Trim() + " " + (c.LastName ?? "")
                    })
                    .OrderBy(c => c.FullName) // Сортируем для удобства
                    .ToList();

                CustomerComboBox.ItemsSource = customers;
                // DisplayMemberPath и SelectedValuePath уже установлены в XAML
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки клиентов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadPaymentMethods()
        {
            try
            {
                PaymentMethodComboBox.ItemsSource = _context.PaymentMethods.ToList();
                // DisplayMemberPath и SelectedValuePath уже установлены в XAML
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки методов оплаты: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadBookingStatuses()
        {
            try
            {
                StatusComboBox.ItemsSource = _context.BookingStatus.ToList();
                // DisplayMemberPath и SelectedValuePath уже установлены в XAML
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки статусов бронирования: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // --- Загрузка деталей бронирования и платежей ---

        private void LoadBookingDetails()
        {
            if (_booking != null)
            {
                RoomComboBox.SelectedValue = _booking.RoomID;
                CustomerComboBox.SelectedValue = _booking.CustomerID;
                CheckInDatePicker.SelectedDate = _booking.CheckInDate;
                CheckOutDatePicker.SelectedDate = _booking.CheckOutDate;
                NumberOfGuestsTextBox.Text = _booking.NumberOfGuests.ToString();
                StatusComboBox.SelectedValue = _booking.StatusID;
                UpdateTotalPrice(); // Обновляем и отображаем общую стоимость
            }
        }

        private void LoadPayments()
        {
            BookingPayments.Clear(); // Очищаем коллекцию
            if (_booking != null && _booking.Payments != null)
            {
                // Добавляем платежи из загруженного бронирования в ObservableCollection
                foreach (var payment in _booking.Payments.ToList()) // ToList() для создания копии, если нужно
                {
                    BookingPayments.Add(payment);
                }
            }
        }

        // --- Обработчики событий ---

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // --- Валидация ввода ---
            if (!ValidateBookingInput())
            {
                return; // Прерываем сохранение, если валидация не пройдена
            }

            bool isNewBooking = (_booking == null); // Проверяем, новое ли это бронирование

            if (isNewBooking)
            {
                _booking = new Bookings();
                _context.Bookings.Add(_booking);
            }

            // --- Присвоение значений ---
            try
            {
                _booking.RoomID = (int)RoomComboBox.SelectedValue;
                _booking.CustomerID = (int)CustomerComboBox.SelectedValue;
                _booking.CheckInDate = CheckInDatePicker.SelectedDate.Value; // .Value, т.к. валидация прошла
                _booking.CheckOutDate = CheckOutDatePicker.SelectedDate.Value; // .Value, т.к. валидация прошла
                _booking.NumberOfGuests = int.Parse(NumberOfGuestsTextBox.Text); // Parse безопасен после валидации
                _booking.StatusID = (int)StatusComboBox.SelectedValue;
                _booking.TotalPrice = CalculateTotalPrice(); // Пересчитываем на всякий случай

                // --- Сохранение ---
                _context.SaveChanges();

                // Если это было новое бронирование, разблокируем вкладку оплаты
                if (isNewBooking)
                {
                    PaymentTabItem.IsEnabled = true;
                    // Перезагружаем бронирование, чтобы получить BookingID и связанные данные
                    _booking = _context.Bookings
                                      .Include(b => b.Payments.Select(p => p.PaymentMethods))
                                      .FirstOrDefault(b => b.BookingID == _booking.BookingID);
                    LoadPayments(); // Загружаем платежи (их пока нет)
                }

                MessageBox.Show("Бронирование успешно сохранено.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                // Можно не закрывать страницу сразу, а дать пользователю добавить оплату
                // NavigationService?.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения бронирования: {ex.Message}\n\nВнутреннее исключение: {ex.InnerException?.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                // Если это было новое бронирование и произошла ошибка, откатываем добавление
                if (isNewBooking && _booking != null)
                {
                    _context.Bookings.Remove(_booking);
                    _booking = null; // Сбрасываем ссылку
                }
            }
        }

        private bool ValidateBookingInput()
        {
            StringBuilder errors = new StringBuilder();

            if (RoomComboBox.SelectedValue == null)
                errors.AppendLine("Пожалуйста, выберите комнату.");
            if (CustomerComboBox.SelectedValue == null)
                errors.AppendLine("Пожалуйста, выберите клиента.");
            if (CheckInDatePicker.SelectedDate == null)
                errors.AppendLine("Пожалуйста, выберите дату заезда.");
            if (CheckOutDatePicker.SelectedDate == null)
                errors.AppendLine("Пожалуйста, выберите дату выезда.");
            if (CheckInDatePicker.SelectedDate.HasValue && CheckOutDatePicker.SelectedDate.HasValue && CheckOutDatePicker.SelectedDate.Value <= CheckInDatePicker.SelectedDate.Value)
                errors.AppendLine("Дата выезда должна быть позже даты заезда.");
            if (!int.TryParse(NumberOfGuestsTextBox.Text, out int guests) || guests <= 0)
                errors.AppendLine("Пожалуйста, введите корректное количество гостей (положительное число).");
            if (StatusComboBox.SelectedValue == null)
                errors.AppendLine("Пожалуйста, выберите статус бронирования.");

            // TODO: Добавить проверку на пересечение дат с другими бронированиями для этой комнаты

            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString(), "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return true;
        }

        private decimal CalculateTotalPrice()
        {
            // Используем _booking.RoomID если редактируем, иначе RoomComboBox.SelectedValue
            int? roomId = _booking?.RoomID ?? (int?)RoomComboBox.SelectedValue;

            if (roomId == null) return 0;

            // Пытаемся найти комнату в локальном кэше или в базе
            var room = _context.Rooms.Local.FirstOrDefault(r => r.RoomID == roomId)
                       ?? _context.Rooms.FirstOrDefault(r => r.RoomID == roomId);

            if (room == null || room.PricePerNight <= 0) return 0; // Проверка цены

            var checkInDate = CheckInDatePicker.SelectedDate;
            var checkOutDate = CheckOutDatePicker.SelectedDate;

            // Проверка на null и корректность дат
            if (checkInDate == null || checkOutDate == null || checkOutDate.Value <= checkInDate.Value) return 0;

            // Рассчитываем количество ночей
            var nights = (checkOutDate.Value - checkInDate.Value).Days;
            return nights * room.PricePerNight;
        }

        private void CheckInDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateTotalPrice();
        }

        private void CheckOutDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateTotalPrice();
        }

        private void UpdateTotalPrice()
        {
            TotalPriceTextBox.Text = CalculateTotalPrice().ToString("F2"); // "F2" для формата с 2 знаками после запятой
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // Проверка на наличие несохраненных изменений (опционально, требует доп. логики)
            var result = MessageBox.Show("Вы уверены, что хотите отменить изменения и вернуться?", "Подтверждение отмены", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                // Освобождаем ресурсы контекста перед уходом со страницы
                _context?.Dispose();
                NavigationService?.GoBack();
            }
        }

        // --- Логика работы с платежами ---

        private void AddOrUpdatePaymentButton_Click(object sender, RoutedEventArgs e)
        {
            // Валидация полей платежа
            if (!ValidatePaymentInput())
            {
                return;
            }

            // Убедимся, что бронирование существует и сохранено (имеет ID)
            if (_booking == null || _booking.BookingID <= 0)
            {
                MessageBox.Show("Пожалуйста, сначала сохраните основную информацию о бронировании.", "Действие невозможно", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                decimal amount = decimal.Parse(PaymentAmountTextBox.Text); // Безопасно после валидации
                DateTime paymentDate = PaymentDatePicker.SelectedDate.Value; // Безопасно после валидации
                int paymentMethodId = (int)PaymentMethodComboBox.SelectedValue; // Безопасно после валидации

                if (_isEditingPayment && _selectedPaymentForEdit != null)
                {
                    // --- РЕЖИМ РЕДАКТИРОВАНИЯ ---
                    // Находим платеж в контексте или в локальной коллекции
                    var paymentToUpdate = _context.Payments.Find(_selectedPaymentForEdit.PaymentID)
                                         ?? BookingPayments.FirstOrDefault(p => p.PaymentID == _selectedPaymentForEdit.PaymentID);

                    if (paymentToUpdate != null)
                    {
                        paymentToUpdate.Amount = amount;
                        paymentToUpdate.PaymentDate = paymentDate;
                        paymentToUpdate.PaymentMethodID = paymentMethodId;
                        // BookingID не меняется

                        _context.SaveChanges(); // Сохраняем изменения в базе

                        // Обновляем элемент в ObservableCollection, чтобы DataGrid обновился
                        // Это может быть не нужно, если объекты в коллекции отслеживаются EF,
                        // но для надежности можно обновить свойства или заменить объект.
                        // Простой способ - перезагрузить платежи (менее оптимально):
                        // LoadPayments();
                        // Более сложный - найти и обновить объект в BookingPayments.
                        // Самый простой при ObservableCollection - DataGrid должен обновиться сам,
                        // если объект реализует INotifyPropertyChanged или EF его отслеживает.
                        // Попробуем без явного обновления коллекции сначала.

                        MessageBox.Show("Платеж успешно обновлен.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        ClearPaymentForm(); // Очищаем форму и выходим из режима редактирования
                    }
                    else
                    {
                        MessageBox.Show("Не удалось найти платеж для обновления.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        ClearPaymentForm(); // Выходим из режима редактирования в случае ошибки
                    }
                }
                else
                {
                    // --- РЕЖИМ ДОБАВЛЕНИЯ ---
                    var newPayment = new Payments
                    {
                        BookingID = _booking.BookingID,
                        PaymentDate = paymentDate,
                        Amount = amount,
                        PaymentMethodID = paymentMethodId,
                        // Связываем с загруженным методом оплаты для отображения в DataGrid
                        PaymentMethods = _context.PaymentMethods.Local.FirstOrDefault(pm => pm.PaymentMethodID == paymentMethodId)
                                       ?? _context.PaymentMethods.Find(paymentMethodId)
                    };

                    _context.Payments.Add(newPayment); // Добавляем в контекст EF
                    _context.SaveChanges(); // Сохраняем в базе

                    // Добавляем новый платеж в ObservableCollection для немедленного отображения
                    BookingPayments.Add(newPayment);

                    MessageBox.Show("Платеж успешно добавлен.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    ClearPaymentForm(clearDate: false); // Очищаем форму, оставляя дату для возможного следующего платежа
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении платежа: {ex.Message}\n\nВнутреннее исключение: {ex.InnerException?.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidatePaymentInput()
        {
            StringBuilder errors = new StringBuilder();

            if (!decimal.TryParse(PaymentAmountTextBox.Text, out decimal amount) || amount <= 0)
                errors.AppendLine("Пожалуйста, введите корректную сумму оплаты (положительное число).");
            if (PaymentDatePicker.SelectedDate == null)
                errors.AppendLine("Пожалуйста, выберите дату оплаты.");
            if (PaymentMethodComboBox.SelectedValue == null)
                errors.AppendLine("Пожалуйста, выберите метод оплаты.");

            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString(), "Ошибка валидации платежа", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return true;
        }

        private void EditPaymentButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.CommandParameter is int paymentId)
            {
                _selectedPaymentForEdit = BookingPayments.FirstOrDefault(p => p.PaymentID == paymentId);

                if (_selectedPaymentForEdit != null)
                {
                    // Заполняем форму данными выбранного платежа
                    PaymentAmountTextBox.Text = _selectedPaymentForEdit.Amount.ToString("F2");
                    PaymentDatePicker.SelectedDate = _selectedPaymentForEdit.PaymentDate;
                    PaymentMethodComboBox.SelectedValue = _selectedPaymentForEdit.PaymentMethodID;

                    // Переключаемся в режим редактирования
                    _isEditingPayment = true;
                    AddOrUpdatePaymentButton.Content = "Обновить оплату"; // Меняем текст кнопки
                                                                          // Можно добавить кнопку "Отмена редактирования" (не реализовано здесь)
                }
            }
        }

        private void DeletePaymentButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.CommandParameter is int paymentId)
            {
                var paymentToDelete = BookingPayments.FirstOrDefault(p => p.PaymentID == paymentId);

                if (paymentToDelete != null)
                {
                    var result = MessageBox.Show($"Вы уверены, что хотите удалить оплату на сумму {paymentToDelete.Amount:F2} от {paymentToDelete.PaymentDate:dd.MM.yyyy}?",
                                                 "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                    if (result == MessageBoxResult.Yes)
                    {
                        try
                        {
                            // Находим платеж в контексте для удаления из базы
                            var paymentInDb = _context.Payments.Find(paymentId);
                            if (paymentInDb != null)
                            {
                                _context.Payments.Remove(paymentInDb);
                                _context.SaveChanges(); // Сохраняем изменения в базе

                                // Удаляем из ObservableCollection для обновления UI
                                BookingPayments.Remove(paymentToDelete);

                                MessageBox.Show("Платеж успешно удален.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                                // Если удаляемый платеж был выбран для редактирования, сбрасываем режим
                                if (_isEditingPayment && _selectedPaymentForEdit?.PaymentID == paymentId)
                                {
                                    ClearPaymentForm();
                                }
                            }
                            else
                            {
                                MessageBox.Show("Не удалось найти платеж в базе данных для удаления.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                // Возможно, стоит также удалить из BookingPayments, если он там есть
                                if (BookingPayments.Contains(paymentToDelete))
                                    BookingPayments.Remove(paymentToDelete);
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Ошибка удаления платежа: {ex.Message}\n\nВнутреннее исключение: {ex.InnerException?.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
        }

        // Вспомогательный метод для очистки формы платежа и сброса режима редактирования
        private void ClearPaymentForm(bool clearDate = true)
        {
            PaymentAmountTextBox.Clear();
            if (clearDate)
            {
                PaymentDatePicker.SelectedDate = null; // Или DateTime.Now;
            }
            PaymentMethodComboBox.SelectedIndex = -1; // Сбрасываем выбор

            _selectedPaymentForEdit = null;
            _isEditingPayment = false;
            AddOrUpdatePaymentButton.Content = "Добавить оплату"; // Возвращаем текст кнопки
        }

        // Вызывается при закрытии страницы для освобождения ресурсов
        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            _context?.Dispose();
        }
    }
}