using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using coursework.Models;
using coursework.Services;

namespace coursework
{
    public partial class MainWindow : Window
    {
        private readonly AgencyService _service = new();

        private Guid? _selectedClientId;
        private Guid? _selectedPropertyId;
        private Guid? _selectedRequestId;

        public MainWindow()
        {
            InitializeComponent();

            try
            {
                _service.Load();
                DealClientComboBox.ItemsSource = _service.Data.Clients;

                DealPropertyComboBox.ItemsSource = _service.Data.Properties
                    .Where(p => p.IsAvailable)
                    .ToList();

                RefreshDeals();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ошибка загрузки данных: " + ex.Message,
                    "Ошибка");
            }

            OwnerComboBox.ItemsSource = _service.Data.Clients;
            RequestClientComboBox.ItemsSource = _service.Data.Clients;

            RefreshRequests();
            RefreshClients();
            RefreshProperties();
        }

        // ==========================================
        // ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ
        // ==========================================

        private static string ComboText(ComboBox combo)
        {
            return (combo.SelectedItem as ComboBoxItem)?
                .Content?.ToString() ?? "";
        }

        private static void SelectComboValue(
            ComboBox combo, string value)
        {
            foreach (ComboBoxItem item in combo.Items)
            {
                if (item.Content?.ToString() == value)
                {
                    combo.SelectedItem = item;
                    return;
                }
            }

            combo.SelectedIndex = -1;
        }


        private void ReadAdditionalRequestFields(Request request)
        {
            request.PreferredFloor =
                ReadInt(RequestFloorTextBox, "Предпочитаемый этаж");

            request.PreferredBuildingFloors =
                ReadInt(RequestBuildingFloorsTextBox, "Этажность дома");

            request.MinimumLivingArea =
                ReadDouble(RequestLivingAreaTextBox, "Минимальная жилая площадь");

            request.MinimumKitchenArea =
                ReadDouble(RequestKitchenAreaTextBox, "Минимальная площадь кухни");

            string material = ComboText(RequestBuildingMaterialComboBox);

            request.PreferredBuildingMaterial =
                material == "Любой" ? "" : material;

            request.MinimumConstructionYear =
                ReadInt(RequestConstructionYearTextBox, "Минимальный год постройки");

            request.MinimumRepairYear =
                ReadInt(RequestRepairYearTextBox, "Минимальный год ремонта");

            request.RequiresBalcony = RequestBalconyCheckBox.IsChecked;
            request.RequiresLoggia = RequestLoggiaCheckBox.IsChecked;
            request.RequiresElevator = RequestElevatorCheckBox.IsChecked;
            request.RequiresGarbageChute = RequestGarbageChuteCheckBox.IsChecked;
            request.RequiresTelephoneCable = RequestTelephoneCableCheckBox.IsChecked;
        }


        private static int ReadInt(
            TextBox box, string field)
        {
            if (!int.TryParse(box.Text, out int value) ||
                value < 0)
            {
                throw new ArgumentException(
                    $"Поле «{field}» должно содержать " +
                    "целое неотрицательное число.");
            }

            return value;
        }


        private static double ReadDouble(
            TextBox box, string field)
        {
            string text = box.Text.Trim();

            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException(
                    $"Заполните поле «{field}».");

            if (text.Length > 30)
                throw new ArgumentException(
                    $"Слишком длинное значение поля «{field}».");

            if (!double.TryParse(
                    text,
                    NumberStyles.Number,
                    CultureInfo.CurrentCulture,
                    out double value) ||
                double.IsNaN(value) ||
                double.IsInfinity(value) ||
                value < 0)
            {
                throw new ArgumentException(
                    $"Поле «{field}» должно содержать " +
                    "корректное неотрицательное число.");
            }

            return value;
        }


        private static decimal ReadDecimal(
            TextBox box, string field)
        {
            if (!decimal.TryParse(
                    box.Text,
                    NumberStyles.Number,
                    CultureInfo.CurrentCulture,
                    out decimal value) || value <= 0)
            {
                throw new ArgumentException(
                    $"Поле «{field}» должно содержать " +
                    "положительное число.");
            }

            return value;
        }

        
        private static T? FindChild<T>(DependencyObject parent)
            where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                DependencyObject child =
                    VisualTreeHelper.GetChild(parent, i);

                if (child is T result)
                    return result;

                T? found = FindChild<T>(child);

                if (found != null)
                    return found;
            }

            return null;
        }


        private void RefreshRequests()
        {
            Guid? selectedId = _selectedRequestId;

            var clients = _service.Data.Clients
                .ToDictionary(c => c.Id, c => c.DisplayName);

            foreach (Request request in _service.Data.Requests)
            {
                request.ClientName =
                    clients.TryGetValue(request.ClientId, out string? name)
                        ? name
                        : "Клиент не найден";
            }

            RequestsGrid.ItemsSource = null;

            RequestsGrid.ItemsSource = _service.Data.Requests
                .OrderByDescending(r => r.IsSpecial)
                .ThenBy(r => r.CreatedAt)
                .ToList();

            if (selectedId.HasValue)
            {
                RequestsGrid.SelectedItem = RequestsGrid.Items
                    .OfType<Request>()
                    .FirstOrDefault(r => r.Id == selectedId.Value);
            }
        }



        // ==========================================
        // КЛИЕНТЫ
        // ==========================================

        private void RefreshClients()
        {
            if (ClientsGrid == null ||
                ClientSearchTextBox == null ||
                ClientCountTextBlock == null)
                return;

            var clients = _service.SearchClients(
                ClientSearchTextBox.Text);

            ClientsGrid.ItemsSource = clients;

            ClientCountTextBlock.Text =
                $"Найдено клиентов: {clients.Count}";
        }


        private void NewClient_Click(object sender, RoutedEventArgs e)
        {
            NewClientWindow window = new NewClientWindow(_service);
            window.Owner = this;

            if (window.ShowDialog() == true)
            {
                RefreshClients();
            }
        }



        private void EditClient_Click(object sender, RoutedEventArgs e)
        {
            if (ClientsGrid.SelectedItem is not Client client)
            {
                MessageBox.Show(
                    "Сначала выберите клиента в таблице.",
                    "Редактирование клиента",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            EditClientWindow window =
                new EditClientWindow(_service, client);

            window.Owner = this;

            if (window.ShowDialog() == true)
            {
                RefreshClients();
                RefreshProperties();
                RefreshRequests();

                // Обновляем списки клиентов в других вкладках
                OwnerComboBox.ItemsSource = null;
                OwnerComboBox.ItemsSource = _service.Data.Clients;

                RequestClientComboBox.ItemsSource = null;
                RequestClientComboBox.ItemsSource = _service.Data.Clients;

                DealClientComboBox.ItemsSource = null;
                DealClientComboBox.ItemsSource = _service.Data.Clients;

                // Возвращаем выделение отредактированного клиента
                ClientsGrid.SelectedItem = ClientsGrid.Items
                    .OfType<Client>()
                    .FirstOrDefault(c => c.Id == client.Id);
            }
        }


        private void DeleteClient_Click(
            object sender, RoutedEventArgs e)
        {
            if (_selectedClientId == null)
            {
                MessageBox.Show(
                    "Выберите клиента в таблице.");
                return;
            }

            MessageBoxResult answer = MessageBox.Show(
                "Удалить выбранного клиента?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (answer != MessageBoxResult.Yes)
                return;

            try
            {
                _service.DeleteClient(
                    _selectedClientId.Value);

                _selectedClientId = null;
                ClientsGrid.SelectedItem = null;
                RefreshClients();
                RefreshProperties();

                MessageBox.Show("Клиент удалён.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка");
            }
        }

        private void ClientsGrid_SelectionChanged(
            object sender, SelectionChangedEventArgs e)
        {
            _selectedClientId =
                (ClientsGrid.SelectedItem as Client)?.Id;
        }

        private void CreateReceipt_Click(
            object sender, RoutedEventArgs e)
        {
            if (_selectedClientId == null)
            {
                MessageBox.Show("Выберите клиента в таблице.");
                return;
            }

            Client? client = _service.Data.Clients
                .FirstOrDefault(c => c.Id == _selectedClientId.Value);

            if (client == null)
            {
                MessageBox.Show("Клиент не найден.");
                return;
            }

            if (!client.IsRegistrationFeePaid ||
                client.RegistrationFee <= 0 ||
                !client.RegistrationFeePaidDate.HasValue)
            {
                MessageBox.Show(
                    "Регистрационный взнос не оплачен. " +
                    "Укажите сумму, отметьте оплату " +
                    "и сохраните изменения клиента.");
                return;
            }

            RegistrationReceiptWindow window =
                new RegistrationReceiptWindow(client);

            window.Owner = this;
            window.ShowDialog();
        }

        private void ClientSearch_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            RefreshClients();
        }

        // ==========================================
        // НЕДВИЖИМОСТЬ
        // ==========================================


        private FrameworkElement? FindPropertyErrorField(string message)
        {
            string error = message.ToLowerInvariant();

            if (error.Contains("капитальн") ||
                error.Contains("год ремонта"))
                return LastRepairYearTextBox;

            if (error.Contains("год постройки"))
                return ConstructionYearTextBox;

            if (error.Contains("жилой площад") ||
                error.Contains("жилая площад"))
                return LivingAreaTextBox;

            if (error.Contains("кухн"))
                return KitchenAreaTextBox;

            if (error.Contains("общая площад") ||
                error.Contains("общую площад") ||
                error.Contains("площад"))
                return AreaTextBox;

            if (error.Contains("этажность"))
                return BuildingFloorsTextBox;

            if (error.Contains("этаж"))
                return FloorPanel.Visibility == Visibility.Visible
                    ? FloorTextBox
                    : BuildingFloorsTextBox;

            if (error.Contains("комнат"))
                return RoomsTextBox;

            if (error.Contains("цен"))
                return PriceTextBox;

            if (error.Contains("владельц"))
                return OwnerComboBox;

            if (error.Contains("район"))
                return DistrictTextBox;

            if (error.Contains("адрес"))
                return PropertyAddressTextBox;

            if (error.Contains("категори"))
                return CategoryComboBox;

            if (error.Contains("тип операции"))
                return OperationComboBox;

            if (error.Contains("материал"))
                return BuildingMaterialComboBox;

            return null;
        }

        private void ShowPropertyError(string message)
        {
            FrameworkElement? field = FindPropertyErrorField(message);

            if (field != null)
            {
                Point position = field.TranslatePoint(
                    new Point(0, 0),
                    PropertyScrollViewer);

                double targetOffset =
                    PropertyScrollViewer.VerticalOffset
                    + position.Y
                    - PropertyScrollViewer.ViewportHeight / 2
                    + field.ActualHeight / 2;

                PropertyScrollViewer.ScrollToVerticalOffset(
                    Math.Max(0, targetOffset));
            }

            Dispatcher.BeginInvoke(
                new Action(() =>
                {
                    MessageBox.Show(
                        this,
                        message,
                        "Ошибка недвижимости",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }),
                System.Windows.Threading.DispatcherPriority.ContextIdle);
        }


        private void CategoryComboBox_SelectionChanged(
            object sender, SelectionChangedEventArgs e)
        {
            ApplyPropertyCategoryRules();
        }

        private void ApplyPropertyCategoryRules()
        {
            if (CategoryComboBox == null ||
                FloorPanel == null ||
                FloorTextBox == null ||
                RoomsLabel == null)
                return;

            string category = ComboText(CategoryComboBox);

            // Для дома не показываем этаж расположения объекта.
            // Этажность здания остаётся доступной.
            FloorPanel.Visibility = category == "Дом"
                ? Visibility.Collapsed
                : Visibility.Visible;

            // Для комнаты количество комнат означает
            // общее количество комнат в квартире.
            RoomsLabel.Text = category == "Комната"
                ? "Количество комнат (всего в квартире)"
                : "Количество комнат";
        }


        private void RefreshProperties()
        {
            if (PropertiesGrid == null ||
                PropertySearchTextBox == null ||
                PropertyCategoryFilter == null ||
                PropertyOperationFilter == null ||
                PropertyCountTextBlock == null)
                return;

            string search = PropertySearchTextBox.Text;
            string category =
                ComboText(PropertyCategoryFilter);
            string operation =
                ComboText(PropertyOperationFilter);


            var properties = _service.SearchProperties(
                search, category, operation);
            foreach (var property in properties)
            {
                property.OwnerName = _service.Data.Clients
                    .FirstOrDefault(c => c.Id == property.OwnerId)?
                    .DisplayName ?? "Владелец не найден";
            }

            // В таблице должны находиться сами объекты Property,
            // чтобы выбор строки позволял редактировать недвижимость.
            // OwnerName уже заполнено в цикле выше.

            PropertiesGrid.ItemsSource = null;
            PropertiesGrid.ItemsSource = properties;
            PropertiesGrid.Items.Refresh();

            PropertyCountTextBlock.Text =
                $"Найдено объектов: {properties.Count}";
        }

        private void PropertyFilter_Changed(
            object sender, RoutedEventArgs e)
        {
            RefreshProperties();
        }

        private Property ReadPropertyForm()
        {
            if (OwnerComboBox.SelectedItem is not Client owner)
            {
                throw new ArgumentException(
                    "Выберите владельца недвижимости.");
            }

            string category = ComboText(CategoryComboBox);

            if (string.IsNullOrWhiteSpace(category))
                throw new ArgumentException(
                    "Выберите категорию недвижимости.");

            int rooms = ReadInt(
                RoomsTextBox, "Количество комнат");

            // Для дома этаж расположения не указывается.
            // Значение 1 используется только внутри модели.
            int floor = category == "Дом"
                ? 1
                : ReadInt(FloorTextBox, "Этаж");

            int buildingFloors = ReadInt(
                BuildingFloorsTextBox, "Этажность дома");

            double area = ReadDouble(
                AreaTextBox, "Общая площадь");

            double livingArea = ReadDouble(
                LivingAreaTextBox, "Жилая площадь");

            double kitchenArea = ReadDouble(
                KitchenAreaTextBox, "Площадь кухни");

            decimal price = ReadDecimal(
                PriceTextBox, "Цена");

            int constructionYear = ReadInt(
                ConstructionYearTextBox, "Год постройки");

            int repairYear = 0;

            if (!string.IsNullOrWhiteSpace(
                LastRepairYearTextBox.Text))
            {
                repairYear = ReadInt(
                    LastRepairYearTextBox,
                    "Год капитального ремонта");
            }

            if (rooms < 1)
            {
                throw new ArgumentException(
                    "Количество комнат должно быть больше нуля.");
            }

            if (floor < 1)
            {
                throw new ArgumentException(
                    "Этаж должен быть больше нуля.");
            }

            if (buildingFloors < 1)
            {
                throw new ArgumentException(
                    "Этажность здания должна быть больше нуля.");
            }

            if (category != "Дом" && floor > buildingFloors)
            {
                throw new ArgumentException(
                    "Этаж не может превышать этажность здания.");
            }

            if (area <= 0)
            {
                throw new ArgumentException(
                    "Общая площадь должна быть больше нуля.");
            }

            if (livingArea <= 0)
            {
                throw new ArgumentException(
                    "Жилая площадь должна быть больше нуля.");
            }

            if (livingArea + kitchenArea > area)
            {
                throw new ArgumentException(
                    "Сумма жилой площади и площади кухни " +
                    "не может превышать общую площадь.");
            }

            if (constructionYear < 1700 ||
                constructionYear > DateTime.Today.Year)
            {
                throw new ArgumentException(
                    "Укажите корректный год постройки.");
            }

            if (repairYear != 0 &&
                (repairYear < constructionYear ||
                 repairYear > DateTime.Today.Year))
            {
                throw new ArgumentException(
                    "Год капитального ремонта должен быть " +
                    "не раньше года постройки и не позже текущего года.");
            }

            if (string.IsNullOrWhiteSpace(DistrictTextBox.Text))
            {
                throw new ArgumentException(
                    "Укажите район недвижимости.");
            }

            if (string.IsNullOrWhiteSpace(PropertyAddressTextBox.Text))
            {
                throw new ArgumentException(
                    "Укажите адрес объекта.");
            }

            if (OperationComboBox.SelectedItem is not ComboBoxItem)
            {
                throw new ArgumentException(
                    "Выберите тип операции.");
            }

            if (BuildingMaterialComboBox.SelectedItem is not ComboBoxItem)
            {
                throw new ArgumentException(
                    "Выберите материал здания.");
            }

            Property newProperty = new Property
            {
                OwnerId = owner.Id,

                Category = category,
                OperationType = ComboText(OperationComboBox),

                District = DistrictTextBox.Text.Trim(),
                Address = PropertyAddressTextBox.Text.Trim(),

                Rooms = rooms,
                Floor = floor,

                TotalArea = area,
                LivingArea = livingArea,
                KitchenArea = kitchenArea,

                Price = price,

                BuildingFloors = buildingFloors,
                BuildingMaterial = ComboText(BuildingMaterialComboBox),

                ConstructionYear = constructionYear,
                LastRepairYear = repairYear,

                HasBalcony = BalconyCheckBox.IsChecked == true,
                HasLoggia = LoggiaCheckBox.IsChecked == true,
                HasElevator = ElevatorCheckBox.IsChecked == true,
                HasGarbageChute = GarbageChuteCheckBox.IsChecked == true,
                HasTelephoneCable = TelephoneCableCheckBox.IsChecked == true,

                Conditions = PropertyConditionsTextBox.Text.Trim(),
                IsAvailable = true
            };

            newProperty.Description =
                PropertyDescriptionTextBox.Text.Trim();

            return PropertyFactory.Convert(newProperty);
        }

        private void AddProperty_Click(
            object sender, RoutedEventArgs e)
        {
            try
            {
                Property property = ReadPropertyForm();

                _service.AddProperty(property);


                var matchingRequests =
                    _service.FindRequestsForProperty(property);


                if (matchingRequests.Count > 0)
                {
                    var matchingWindow = new MatchingRequestsWindow(
                        matchingRequests,
                        _service);

                    matchingWindow.Owner = this;
                    matchingWindow.ShowDialog();
                }
                else
                {
                    MessageBox.Show(
                        "Объект добавлен, но подходящих заявок пока нет.",
                        "Автоматический подбор",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }



                ClearPropertyForm();
                RefreshProperties();

                MessageBox.Show(
                    "Объект недвижимости добавлен.");
            }
            catch (Exception ex)
            {
                ShowPropertyError(ex.Message);
            }
        }

        private void UpdateProperty_Click(
            object sender, RoutedEventArgs e)
        {
            if (_selectedPropertyId == null)
            {
                MessageBox.Show(
                    "Выберите объект недвижимости в таблице.");
                return;
            }

            try
            {
                Property updated = ReadPropertyForm();

                updated.Id = _selectedPropertyId.Value;

                Property original =
                    _service.Data.Properties.First(
                        p => p.Id == updated.Id);

                updated.IsAvailable = original.IsAvailable;

                _service.UpdateProperty(updated);

                ClearPropertyForm();
                RefreshProperties();

                MessageBox.Show(
                    "Данные недвижимости обновлены.");
            }
            catch (Exception ex)
            {
                ShowPropertyError(ex.Message);
            }
        }

        private void DeleteProperty_Click(
            object sender, RoutedEventArgs e)
        {
            if (_selectedPropertyId == null)
            {
                MessageBox.Show(
                    "Выберите объект недвижимости в таблице.");
                return;
            }

            MessageBoxResult answer = MessageBox.Show(
                "Удалить выбранный объект недвижимости?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (answer != MessageBoxResult.Yes)
                return;

            try
            {
                _service.DeleteProperty(
                    _selectedPropertyId.Value);

                ClearPropertyForm();
                RefreshProperties();

                MessageBox.Show(
                    "Объект недвижимости удалён.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка");
            }
        }

        private void NewProperty_Click(object sender, RoutedEventArgs e)
        {
            // Снимаем выбор и переходим в режим создания объекта.
            ClearPropertyForm();
            OwnerComboBox.Focus();
        }

        private void ClearProperty_Click(
            object sender, RoutedEventArgs e)
        {
            ClearPropertyForm();

        }


        private void ClearPropertyForm()
        {
            _selectedPropertyId = null;

            PropertiesGrid.SelectedItem = null;

            OwnerComboBox.SelectedIndex = -1;
            CategoryComboBox.SelectedIndex = 0;
            OperationComboBox.SelectedIndex = 0;

            DistrictTextBox.Clear();
            PropertyAddressTextBox.Clear();

            RoomsTextBox.Clear();
            FloorTextBox.Clear();

            AreaTextBox.Clear();
            LivingAreaTextBox.Clear();
            KitchenAreaTextBox.Clear();

            PriceTextBox.Clear();

            BuildingFloorsTextBox.Clear();
            BuildingMaterialComboBox.SelectedIndex = 0;

            ConstructionYearTextBox.Clear();
            LastRepairYearTextBox.Clear();

            BalconyCheckBox.IsChecked = false;
            LoggiaCheckBox.IsChecked = false;
            ElevatorCheckBox.IsChecked = false;
            GarbageChuteCheckBox.IsChecked = false;
            TelephoneCableCheckBox.IsChecked = false;

            PropertyDescriptionTextBox.Clear();
            PropertyConditionsTextBox.Clear();

            ApplyPropertyCategoryRules();
        }

        private void PropertiesGrid_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            if (PropertiesGrid.SelectedItem is not Property property)
                return;

            _selectedPropertyId = property.Id;

            OwnerComboBox.SelectedItem =
                _service.Data.Clients.FirstOrDefault(
                    c => c.Id == property.OwnerId);

            SelectComboValue(
                CategoryComboBox, property.Category);

            SelectComboValue(
                OperationComboBox, property.OperationType);

            DistrictTextBox.Text = property.District;
            PropertyAddressTextBox.Text = property.Address;

            RoomsTextBox.Text = property.Rooms.ToString();

            FloorTextBox.Text = property.Floor.ToString();

            AreaTextBox.Text = property.TotalArea.ToString();
            LivingAreaTextBox.Text = property.LivingArea.ToString();
            KitchenAreaTextBox.Text = property.KitchenArea.ToString();

            PriceTextBox.Text = property.Price.ToString();

            BuildingFloorsTextBox.Text =
                property.BuildingFloors.ToString();

            SelectComboValue(
                BuildingMaterialComboBox,
                property.BuildingMaterial);

            ConstructionYearTextBox.Text =
                property.ConstructionYear.ToString();

            LastRepairYearTextBox.Text =
                property.LastRepairYear == 0
                    ? ""
                    : property.LastRepairYear.ToString();

            BalconyCheckBox.IsChecked = property.HasBalcony;
            LoggiaCheckBox.IsChecked = property.HasLoggia;
            ElevatorCheckBox.IsChecked = property.HasElevator;
            GarbageChuteCheckBox.IsChecked = property.HasGarbageChute;
            TelephoneCableCheckBox.IsChecked =
                property.HasTelephoneCable;

            PropertyDescriptionTextBox.Text =
                property.Description;

            PropertyConditionsTextBox.Text =
                property.Conditions;

            ApplyPropertyCategoryRules();
        }


        // ==========================================
        // ЗАЯВКИ И ПОДБОР НЕДВИЖИМОСТИ
        // ==========================================

        private FrameworkElement? FindRequestErrorField(string message)
        {
            string error = message.ToLowerInvariant();

            // Сначала проверяем конкретные поля
            if (error.Contains("год капитального ремонта") || error.Contains("год ремонта"))
            {
                return RequestRepairYearTextBox;
            }

            if (error.Contains("минимальный год постройки"))
                return RequestConstructionYearTextBox;

            if (error.Contains("минимальная жилая площадь"))
                return RequestLivingAreaTextBox;

            if (error.Contains("минимальная площадь кухни"))
                return RequestKitchenAreaTextBox;

            if (error.Contains("минимальная площадь"))
                return RequestAreaTextBox;

            if (error.Contains("предпочитаемый этаж"))
                return RequestFloorTextBox;

            if (error.Contains("этажность"))
                return RequestBuildingFloorsTextBox;

            if (error.Contains("количество комнат") ||
                error.Contains("минимум комнат"))
                return RequestRoomsTextBox;

            if (error.Contains("максимальная цена") ||
                error.Contains("цен"))
                return RequestPriceTextBox;

            if (error.Contains("клиент"))
                return RequestClientComboBox;

            if (error.Contains("район"))
                return RequestDistrictTextBox;

            if (error.Contains("категори"))
                return RequestCategoryComboBox;

            if (error.Contains("тип операции"))
                return RequestOperationComboBox;

            if (error.Contains("материал"))
                return RequestBuildingMaterialComboBox;

            if (error.Contains("специальн") ||
                error.Contains("индивидуальн") ||
                error.Contains("услови") ||
                error.Contains("пожелани"))
                return RequestConditionsTextBox;

            return null;
        }

        private void ShowRequestError(string message)
        {
            FrameworkElement? field = FindRequestErrorField(message);

            if (field != null)
            {
                Point position = field.TranslatePoint(
                    new Point(0, 0),
                    RequestScrollViewer);

                // Располагаем поле выше центра видимой области,
                // чтобы его не закрывало окно сообщения
                double targetOffset =
                    RequestScrollViewer.VerticalOffset
                    + position.Y
                    - RequestScrollViewer.ViewportHeight / 3
                    + field.ActualHeight / 2;

                RequestScrollViewer.ScrollToVerticalOffset(
                    Math.Max(0, targetOffset));
            }

            Dispatcher.BeginInvoke(
                new Action(() =>
                {
                    MessageBox.Show(
                        this,
                        message,
                        "Ошибка заявки",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }),
                System.Windows.Threading.DispatcherPriority.ContextIdle);
        }

        private void FindProperties_Click(
            object sender, RoutedEventArgs e)
        {
            try
            {
                if (RequestClientComboBox.SelectedItem is not Client client)
                {
                    throw new ArgumentException("Выберите клиента.");
                }

                Request request = new Request
                {
                    ClientId = client.Id,
                    OperationType = ComboText(RequestOperationComboBox),
                    Category = ComboText(RequestCategoryComboBox),
                    District = RequestDistrictTextBox.Text.Trim(),

                    MinimumRooms = ReadInt(
                        RequestRoomsTextBox,
                        "Минимальное количество комнат"),

                    MinimumArea = ReadDouble(
                        RequestAreaTextBox,
                        "Минимальная площадь"),

                    MaximumPrice = ReadDecimal(
                        RequestPriceTextBox,
                        "Максимальная цена"),

                    Conditions = RequestConditionsTextBox.Text.Trim(),
                    IsSpecial = SpecialRequestCheckBox.IsChecked == true
                };

                ReadAdditionalRequestFields(request);

                _service.AddRequest(request);

                _selectedRequestId = request.Id;

                RefreshRequests();

                RequestsGrid.SelectedItem = request;
                RequestsGrid.ScrollIntoView(request);

                ResultsGrid.ItemsSource = null;

                MessageBox.Show(
                    "Заявка успешно зарегистрирована.\n" +
                    "Для поиска объектов откройте подраздел «Подбор недвижимости».");
            }
            catch (Exception ex)
            {
                ShowRequestError(ex.Message);
            }
        }

        private void SearchExistingRequest_Click(
            object sender, RoutedEventArgs e)
        {
            try
            {
                if (RequestsGrid.SelectedItem is not Request request)
                {
                    MessageBox.Show(
                        "Сначала выберите зарегистрированную заявку.");
                    return;
                }

                var results = _service.FindProperties(request);

                ResultsGrid.ItemsSource = results;


                if (request.IsSpecial)
                {
                    MessageBox.Show(
                        "СПЕЦИАЛЬНЫЙ ЗАКАЗ\n\n" +
                        "Заявка обслуживается в приоритетном порядке.\n\n" +
                        $"Индивидуальные условия:\n{request.Conditions}\n\n" +
                        $"Найдено подходящих объектов: {results.Count}",
                        "Приоритетное обслуживание",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show(
                        $"Найдено подходящих объектов: {results.Count}",
                        "Результаты подбора",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка подбора");
            }
        }

        private void AcceptOffer_Click(object sender, RoutedEventArgs e)
        {
            RegisterSelectedOfferDecision(true);
        }

        private void RejectOffer_Click(object sender, RoutedEventArgs e)
        {
            RegisterSelectedOfferDecision(false);
        }


        private void GoToDeal_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (RequestsGrid.SelectedItem is not Request request)
                {
                    MessageBox.Show(
                        "Сначала выберите заявку клиента.");
                    return;
                }

                if (ResultsGrid.SelectedItem is not Property property)
                {
                    MessageBox.Show(
                        "Выберите объект недвижимости.");
                    return;
                }

                // Проверяем, что клиент принял предложение
                bool accepted = _service.Data.OfferDecisions?
                    .Any(d =>
                        d.RequestId == request.Id &&
                        d.PropertyId == property.Id &&
                        d.Status == "Принято") == true;

                if (!accepted)
                {
                    MessageBox.Show(
                        "Сначала зарегистрируйте согласие клиента " +
                        "с помощью кнопки «Принять предложение».",
                        "Предложение не принято",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                // Обновляем списки для оформления сделки
                DealClientComboBox.ItemsSource = null;
                DealClientComboBox.ItemsSource = _service.Data.Clients;

                DealPropertyComboBox.ItemsSource = null;
                DealPropertyComboBox.ItemsSource =
                    _service.Data.Properties
                        .Where(p => p.IsAvailable)
                        .ToList();

                // Выбираем клиента и недвижимость
                DealClientComboBox.SelectedItem =
                    _service.Data.Clients.FirstOrDefault(
                        c => c.Id == request.ClientId);

                DealPropertyComboBox.SelectedItem =
                    _service.Data.Properties.FirstOrDefault(
                        p => p.Id == property.Id);

                // Подставляем стоимость объекта
                DealAmountTextBox.Text = property.Price.ToString();

                // Переходим на вкладку «Сделки»

                MainTabControl.SelectedIndex = 3;

            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        private void RegisterSelectedOfferDecision(bool accepted)
        {
            try
            {
                if (RequestsGrid.SelectedItem is not Request request)
                {
                    MessageBox.Show(
                        "Сначала выберите заявку в списке зарегистрированных заявок.",
                        "Внимание",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                if (ResultsGrid.SelectedItem is not Property property)
                {
                    MessageBox.Show(
                        "Выберите объект недвижимости в таблице предложений.",
                        "Внимание",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                string comment = OfferCommentTextBox.Text.Trim();

                string action = accepted ? "принять" : "отклонить";

                MessageBoxResult confirmation = MessageBox.Show(
                    $"Вы действительно хотите {action} предложение:\n" +
                    $"{property.Address}?",
                    "Подтверждение решения",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (confirmation != MessageBoxResult.Yes)
                    return;

                _service.RegisterOfferDecision(
                    request.Id,
                    property.Id,
                    accepted,
                    comment);

                MessageBox.Show(
                    accepted
                        ? "Предложение принято. Теперь можно перейти к оформлению сделки."
                        : "Отказ клиента сохранён.",
                    "Решение зарегистрировано",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);


                OfferCommentTextBox.Clear();

                // Обновляем предложения для выбранной заявки
                var refreshed = _service.FindProperties(request);

                ResultsGrid.ItemsSource = refreshed;

                // Сохраняем выделение принятого или отклонённого объекта
                ResultsGrid.SelectedItem =
                    refreshed.FirstOrDefault(p => p.Id == property.Id);

            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        private void PrintMatchingProperties_Click(
            object sender, RoutedEventArgs e)
        {
            // Проверяем, выбрана ли заявка
            if (RequestsGrid.SelectedItem is not Request request)
            {
                MessageBox.Show(
                    "Сначала выберите заявку в таблице.");
                return;
            }

            // Находим клиента, которому принадлежит заявка
            Client? client = _service.Data.Clients
                .FirstOrDefault(c => c.Id == request.ClientId);

            if (client == null)
            {
                MessageBox.Show("Клиент заявки не найден.");
                return;
            }

            // Получаем актуальный список подходящих объектов
            var properties = _service.FindProperties(request);

            // Открываем документ
            MatchingPropertiesReportWindow window =
                new MatchingPropertiesReportWindow(
                    client, request, properties);

            window.Owner = this;
            window.ShowDialog();
        }

        private void RequestsGrid_SelectionChanged(
            object sender, SelectionChangedEventArgs e)
        {
            if (RequestsGrid.SelectedItem is not Request request)
                return;
            _selectedRequestId = request.Id;

            RequestClientComboBox.SelectedItem =
                _service.Data.Clients.FirstOrDefault(
                    c => c.Id == request.ClientId);

            SelectComboValue(
                RequestOperationComboBox,
                request.OperationType);

            SelectComboValue(
                RequestCategoryComboBox,
                request.Category);

            RequestDistrictTextBox.Text = request.District;

            RequestRoomsTextBox.Text =
                request.MinimumRooms.ToString();

            RequestAreaTextBox.Text =
                request.MinimumArea.ToString();

            RequestPriceTextBox.Text =
                request.MaximumPrice.ToString();

            RequestConditionsTextBox.Text =
                request.Conditions;

            SpecialRequestCheckBox.IsChecked =
                request.IsSpecial;

            // НОВЫЕ ПОЛЯ — вставить сюда
            RequestFloorTextBox.Text = request.PreferredFloor.ToString();

            RequestBuildingFloorsTextBox.Text =
                request.PreferredBuildingFloors.ToString();

            RequestLivingAreaTextBox.Text =
                request.MinimumLivingArea.ToString();

            RequestKitchenAreaTextBox.Text =
                request.MinimumKitchenArea.ToString();

            RequestConstructionYearTextBox.Text =
                request.MinimumConstructionYear.ToString();

            RequestRepairYearTextBox.Text =
                request.MinimumRepairYear.ToString();

            SelectComboValue(
                RequestBuildingMaterialComboBox,
                string.IsNullOrWhiteSpace(request.PreferredBuildingMaterial)
                    ? "Любой"
                    : request.PreferredBuildingMaterial);

            RequestBalconyCheckBox.IsChecked = request.RequiresBalcony;
            RequestLoggiaCheckBox.IsChecked = request.RequiresLoggia;
            RequestElevatorCheckBox.IsChecked = request.RequiresElevator;
            RequestGarbageChuteCheckBox.IsChecked = request.RequiresGarbageChute;
            RequestTelephoneCableCheckBox.IsChecked =
                request.RequiresTelephoneCable;

            // Подбираем недвижимость для выбранной заявки.

            ResultsGrid.ItemsSource = null;

        }


        private void FindRequestsForSelectedProperty_Click(
            object sender, RoutedEventArgs e)
        {
            // Проверяем, выбран ли объект в таблице
            if (PropertiesGrid.SelectedItem is not Property property)
            {
                MessageBox.Show(
                    "Сначала выберите объект недвижимости в таблице.");
                return;
            }

            // Ищем заявки, которым подходит выбранный объект
            var matchingRequests =
                _service.FindRequestsForProperty(property);

            if (matchingRequests.Count == 0)
            {
                MessageBox.Show(
                    "Для выбранного объекта подходящих заявок пока нет.",
                    "Результаты подбора",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            // Открываем существующее окно с подходящими заявками
            MatchingRequestsWindow window =
                new MatchingRequestsWindow(matchingRequests, _service);

            window.Owner = this;
            window.ShowDialog();
        }

        private void UpdateRequest_Click(
            object sender, RoutedEventArgs e)
        {
            if (_selectedRequestId == null)
            {
                MessageBox.Show("Сначала выберите заявку в таблице.");
                return;
            }

            try
            {
                if (RequestClientComboBox.SelectedItem is not Client client)
                    throw new ArgumentException("Выберите клиента.");

                Request updated = new Request
                {
                    Id = _selectedRequestId.Value,
                    ClientId = client.Id,
                    OperationType = ComboText(RequestOperationComboBox),
                    Category = ComboText(RequestCategoryComboBox),
                    District = RequestDistrictTextBox.Text.Trim(),
                    MinimumRooms = ReadInt(
                        RequestRoomsTextBox, "Минимальное количество комнат"),
                    MinimumArea = ReadDouble(
                        RequestAreaTextBox, "Минимальная площадь"),
                    MaximumPrice = ReadDecimal(
                        RequestPriceTextBox, "Максимальная цена"),
                    Conditions = RequestConditionsTextBox.Text.Trim(),
                    IsSpecial = SpecialRequestCheckBox.IsChecked == true
                };
                ReadAdditionalRequestFields(updated);

                _service.UpdateRequest(updated);
                RefreshRequests();

                // Обновляем отображение, так как Request
                // пока не реализует INotifyPropertyChanged.
                RequestsGrid.Items.Refresh();

                Request saved = _service.Data.Requests.First(
                    r => r.Id == updated.Id);

                ResultsGrid.ItemsSource =
                    _service.FindProperties(saved);

                MessageBox.Show("Изменения заявки сохранены.");
            }
            catch (Exception ex)
            {
                ShowRequestError(ex.Message);
            }
        }

        private void DeleteRequest_Click(
            object sender, RoutedEventArgs e)
        {
            if (_selectedRequestId == null)
            {
                MessageBox.Show("Сначала выберите заявку в таблице.");
                return;
            }

            if (MessageBox.Show(
                "Удалить выбранную заявку?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning) != MessageBoxResult.Yes)
                return;

            try
            {
                _service.DeleteRequest(_selectedRequestId.Value);

                _selectedRequestId = null;
                RefreshRequests();
                ClearRequestForm();
                MessageBox.Show("Заявка удалена.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка");
            }
        }

        private void ClearRequest_Click(
            object sender, RoutedEventArgs e)
        {
            ClearRequestForm();
        }

        private void ClearRequestForm()
        {

            _selectedRequestId = null;

            RequestsGrid.SelectedItem = null;
            RequestClientComboBox.SelectedIndex = -1;
            RequestOperationComboBox.SelectedIndex = 0;
            RequestCategoryComboBox.SelectedIndex = 0;

            RequestDistrictTextBox.Clear();
            RequestRoomsTextBox.Clear();
            RequestAreaTextBox.Clear();
            RequestPriceTextBox.Clear();
            RequestConditionsTextBox.Clear();

            SpecialRequestCheckBox.IsChecked = false;
            ResultsGrid.ItemsSource = null;

            RequestFloorTextBox.Text = "0";
            RequestBuildingFloorsTextBox.Text = "0";
            RequestLivingAreaTextBox.Text = "0";
            RequestKitchenAreaTextBox.Text = "0";

            RequestConstructionYearTextBox.Text = "0";
            RequestRepairYearTextBox.Text = "0";

            RequestBuildingMaterialComboBox.SelectedIndex = 0;

            RequestBalconyCheckBox.IsChecked = null;
            RequestLoggiaCheckBox.IsChecked = null;
            RequestElevatorCheckBox.IsChecked = null;
            RequestGarbageChuteCheckBox.IsChecked = null;
            RequestTelephoneCableCheckBox.IsChecked = null;

        }



        private void AddDeal_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DealClientComboBox.SelectedItem is not Client client)
                {
                    MessageBox.Show("Выберите клиента.");
                    return;
                }

                if (DealPropertyComboBox.SelectedItem is not Property property)
                {
                    MessageBox.Show("Выберите объект недвижимости.");
                    return;
                }

                if (!decimal.TryParse(DealAmountTextBox.Text, out decimal amount)
                    || amount <= 0)
                {
                    MessageBox.Show("Введите корректную сумму сделки.");
                    return;
                }

                // Проверяем, что клиент принял предложение по своей заявке
                bool accepted = _service.Data.OfferDecisions.Any(d =>
                    d.PropertyId == property.Id &&
                    d.Status == "Принято" &&
                    _service.Data.Requests.Any(r =>
                        r.Id == d.RequestId &&
                        r.ClientId == client.Id));

                if (!accepted)
                {
                    MessageBox.Show(
                        "Нельзя оформить сделку без согласия клиента.\n" +
                        "Сначала выберите заявку, найдите подходящий объект " +
                        "и нажмите «Принять предложение».",
                        "Согласие не зарегистрировано",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }

                Deal deal = new Deal
                {
                    ClientId = client.Id,
                    PropertyId = property.Id,
                    Amount = amount
                };

                _service.AddDeal(deal);

                RefreshProperties();

                DealPropertyComboBox.ItemsSource = _service.Data.Properties
                    .Where(p => p.IsAvailable)
                    .ToList();

                RefreshDeals();

                MessageBox.Show(
                    $"Сделка успешно оформлена!\n" +
                    $"Сумма: {amount:N0} руб.\n" +
                    $"Комиссия агентства: {deal.Commission:N0} руб.",
                    "Оформление сделки",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                DealAmountTextBox.Clear();
                DealCommissionTextBlock.Text = "0 руб.";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка");
            }
        }


        private void RefreshDeals()
        {
            DealsGrid.ItemsSource = _service.Data.Deals
                .OrderByDescending(d => d.DealDate)
                .Select(d => new
                {
                    d.DealDate,

                    SellerName = _service.Data.Clients
                        .FirstOrDefault(c => c.Id ==
                            (d.SellerId != Guid.Empty
                                ? d.SellerId
                                : _service.Data.Properties
                                    .FirstOrDefault(p => p.Id == d.PropertyId)?
                                    .OwnerId))?.FullName
                        ?? "Неизвестный продавец",

                    ClientName = _service.Data.Clients
                        .FirstOrDefault(c => c.Id == d.ClientId)?
                        .DisplayName ?? "Неизвестный покупатель",

                    PropertyAddress = _service.Data.Properties
                        .FirstOrDefault(p => p.Id == d.PropertyId)?
                        .Address ?? "Объект не найден",

                    d.Amount,
                    d.Commission
                })
                .ToList();
        }


        private void DealPropertyComboBox_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            if (DealSellerTextBox == null ||
                DealAmountTextBox == null)
                return;

            if (DealPropertyComboBox.SelectedItem is not Property property)
            {
                DealSellerTextBox.Text = "Выберите недвижимость";
                DealAmountTextBox.Clear();
                return;
            }

            Client? seller = _service.Data.Clients
                .FirstOrDefault(c => c.Id == property.OwnerId);

            DealSellerTextBox.Text =
                seller?.DisplayName ?? "Владелец не найден";

            DealAmountTextBox.Text = property.Price.ToString("0.##");
        }

        private void DealAmountTextBox_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            if (DealCommissionTextBlock == null ||
                DealAmountTextBox == null)
                return;

            if (decimal.TryParse(
                    DealAmountTextBox.Text,
                    out decimal amount) && amount > 0)
            {

                decimal commission = Math.Round(
                    amount * 0.02m,
                    2,
                    MidpointRounding.AwayFromZero);


                DealCommissionTextBlock.Text =
                    $"{commission:N2} руб.";
            }
            else
            {
                DealCommissionTextBlock.Text = "0 руб.";
            }
        }


        private void PrintDealContract_Click(
            object sender, RoutedEventArgs e)
        {
            if (DealsGrid.SelectedIndex < 0)
            {
                MessageBox.Show("Выберите сделку в таблице.");
                return;
            }

            var deals = _service.Data.Deals
                .OrderByDescending(d => d.DealDate)
                .ToList();

            int index = DealsGrid.SelectedIndex;

            if (index >= deals.Count)
                return;

            Deal deal = deals[index];

            Client buyer = _service.Data.Clients
                .First(c => c.Id == deal.ClientId);

            Client seller = _service.Data.Clients
                .First(c => c.Id == deal.SellerId);

            Property property = _service.Data.Properties
                .First(p => p.Id == deal.PropertyId);

            DealContractWindow window =
                new DealContractWindow(deal, buyer, seller, property);

            window.Owner = this;
            window.ShowDialog();
        }

        // Единый расчёт доходов за период для обеих вкладок статистики.
        // Доход = комиссия по оформленным сделкам + оплаченные взносы.
        // Суммы сделок не являются доходом агентства.
        private (int DealCount, decimal DealAmount, decimal Commission,
            decimal Fees, decimal TotalIncome) GetIncomeForPeriod(
            DateTime start, DateTime end)
        {
            var deals = _service.Data.Deals
                .Where(d => d.DealDate.Date >= start &&
                            d.DealDate.Date <= end)
                .ToList();

            var paidClients = _service.Data.Clients
                .Where(c => c.IsRegistrationFeePaid &&
                            c.RegistrationFeePaidDate.HasValue &&
                            c.RegistrationFeePaidDate.Value.Date >= start &&
                            c.RegistrationFeePaidDate.Value.Date <= end)
                .ToList();

            decimal commission = deals.Sum(d => d.Commission);
            decimal fees = paidClients.Sum(c => c.RegistrationFee);

            return (
                deals.Count,
                deals.Sum(d => d.Amount),
                commission,
                fees,
                commission + fees);
        }


        // Если в первом периоде значение равно нулю, процент изменения
        // не определён: показываем прочерк, а не делим на ноль.
        private static string FormatPercentChange(
            decimal first, decimal second)
        {
            if (first == 0m)
                return "—";

            decimal percent = (second - first) / first * 100m;
            return $"{percent:+0.0;-0.0;0.0}%";
        }

        private void CalculateProfit_Click(
            object sender, RoutedEventArgs e)
        {
            if (!ReportStart1.SelectedDate.HasValue ||
                !ReportEnd1.SelectedDate.HasValue ||
                !ReportStart2.SelectedDate.HasValue ||
                !ReportEnd2.SelectedDate.HasValue)
            {
                MessageBox.Show("Выберите даты обоих периодов.");
                return;
            }

            DateTime start1 = ReportStart1.SelectedDate.Value.Date;
            DateTime end1 = ReportEnd1.SelectedDate.Value.Date;
            DateTime start2 = ReportStart2.SelectedDate.Value.Date;
            DateTime end2 = ReportEnd2.SelectedDate.Value.Date;

            if (start1 > end1 || start2 > end2)
            {
                MessageBox.Show("Проверьте даты периодов.");
                return;
            }

            var first = GetIncomeForPeriod(start1, end1);
            var second = GetIncomeForPeriod(start2, end2);

            ProfitComparisonGrid.ItemsSource = new[]
            {
                new
                {
                    Indicator = "Количество сделок",
                    Period1 = first.DealCount.ToString(),
                    Period2 = second.DealCount.ToString(),
                    Difference = (second.DealCount - first.DealCount)
                        .ToString("+0;-0;0"),
                    Percent = FormatPercentChange(
                        first.DealCount, second.DealCount)
                },
                new
                {
                    Indicator = "Общая сумма сделок",
                    Period1 = $"{first.DealAmount:N2} руб.",
                    Period2 = $"{second.DealAmount:N2} руб.",
                    Difference = $"{second.DealAmount - first.DealAmount:+#,##0.00;-#,##0.00;0.00} руб.",
                    Percent = FormatPercentChange(
                        first.DealAmount, second.DealAmount)
                },
                new
                {
                    Indicator = "Доход от комиссии (2%)",
                    Period1 = $"{first.Commission:N2} руб.",
                    Period2 = $"{second.Commission:N2} руб.",
                    Difference = $"{second.Commission - first.Commission:+#,##0.00;-#,##0.00;0.00} руб.",
                    Percent = FormatPercentChange(
                        first.Commission, second.Commission)
                },
                new
                {
                    Indicator = "Регистрационные взносы",
                    Period1 = $"{first.Fees:N2} руб.",
                    Period2 = $"{second.Fees:N2} руб.",
                    Difference = $"{second.Fees - first.Fees:+#,##0.00;-#,##0.00;0.00} руб.",
                    Percent = FormatPercentChange(
                        first.Fees, second.Fees)
                },
                new
                {
                    Indicator = "Общий доход агентства",
                    Period1 = $"{first.TotalIncome:N2} руб.",
                    Period2 = $"{second.TotalIncome:N2} руб.",
                    Difference = $"{second.TotalIncome - first.TotalIncome:+#,##0.00;-#,##0.00;0.00} руб.",
                    Percent = FormatPercentChange(
                        first.TotalIncome, second.TotalIncome)
                }
            };
        }

        private void GenerateGeneralReport_Click(
            object sender, RoutedEventArgs e)
        {
            if (!GeneralReportStart.SelectedDate.HasValue ||
                !GeneralReportEnd.SelectedDate.HasValue)
            {
                MessageBox.Show("Выберите даты отчёта.");
                return;
            }

            DateTime start = GeneralReportStart.SelectedDate.Value.Date;
            DateTime end = GeneralReportEnd.SelectedDate.Value.Date;

            if (start > end)
            {
                MessageBox.Show("Начало периода позже окончания.");
                return;
            }

            var result = GetIncomeForPeriod(start, end);

            GeneralReportGrid.ItemsSource = new[]
            {
                new { Indicator = "Количество сделок",
                    Value = result.DealCount.ToString() },
                new { Indicator = "Общая сумма сделок",
                    Value = $"{result.DealAmount:N2} руб." },
                new { Indicator = "Доход от комиссии (2%)",
                    Value = $"{result.Commission:N2} руб." },
                new { Indicator = "Регистрационные взносы",
                    Value = $"{result.Fees:N2} руб." },
                new { Indicator = "Общий доход агентства",
                    Value = $"{result.TotalIncome:N2} руб." }
            };
        }



        private void ShowPopularCategories_Click(
            object sender, RoutedEventArgs e)
        {
            var result = new List<dynamic>();

            string[] operations = { "Аренда", "Продажа" };

            foreach (string operation in operations)
            {
                // Заявки клиентов: покупка соответствует продаже
                string requestOperation = operation == "Продажа"
                    ? "Покупка"
                    : "Аренда";

                var demand = _service.Data.Requests
                    .Where(r => r.OperationType == requestOperation)
                    .Where(r => !string.IsNullOrWhiteSpace(r.Category))
                    .GroupBy(r => r.Category.Trim())
                    .Select(g => new
                    {
                        Category = g.Key,
                        Count = g.Count()
                    })
                    .OrderByDescending(x => x.Count)
                    .ThenBy(x => x.Category)
                    .ToList();

                var offers = _service.Data.Properties
                    .Where(p => p.OperationType == operation)
                    .Where(p => !string.IsNullOrWhiteSpace(p.Category))
                    .GroupBy(p => p.Category.Trim())
                    .Select(g => new
                    {
                        Category = g.Key,
                        Count = g.Count()
                    })
                    .OrderByDescending(x => x.Count)
                    .ThenBy(x => x.Category)
                    .ToList();

                result.Add(new
                {
                    Operation = operation,
                    Type = "Спрос",
                    Category = demand.Count > 0
                        ? demand[0].Category
                        : "Нет данных",
                    Count = demand.Count > 0
                        ? demand[0].Count
                        : 0
                });

                result.Add(new
                {
                    Operation = operation,
                    Type = "Предложения",
                    Category = offers.Count > 0
                        ? offers[0].Category
                        : "Нет данных",
                    Count = offers.Count > 0
                        ? offers[0].Count
                        : 0
                });
            }

            PopularCategoriesGrid.ItemsSource = result;
        }
   
        
    }
}
