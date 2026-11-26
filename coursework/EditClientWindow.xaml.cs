using coursework.Models;
using coursework.Services;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace coursework
{
    public partial class EditClientWindow : Window
    {
        private readonly AgencyService _service;
        private readonly Client _originalClient;

        public EditClientWindow(AgencyService service, Client client)
        {
            InitializeComponent();

            _service = service;
            _originalClient = client;

            LoadClientData();
        }

        // Загружаем данные выбранного клиента в форму
        private void LoadClientData()
        {
            ClientNameTextBlock.Text =
                _originalClient.GetIdentityDescription();

            LastNameTextBox.Text = _originalClient.LastName;
            FirstNameTextBox.Text = _originalClient.FirstName;
            MiddleNameTextBox.Text = _originalClient.MiddleName;

            PhoneTextBox.Text = _originalClient.Phone;
            EmailTextBox.Text = _originalClient.Email;
            AddressTextBox.Text = _originalClient.Address;

            RegistrationTextBox.Text =
                _originalClient.RegistrationNumber;

            RegistrationFeeTextBox.Text =
                _originalClient.RegistrationFee.ToString("0.##");

            RegistrationFeePaidCheckBox.IsChecked =
                _originalClient.IsRegistrationFeePaid;


            // Уже оплаченный взнос менять нельзя
            bool paymentCompleted =
                _originalClient.IsRegistrationFeePaid;

            RegistrationFeeTextBox.IsReadOnly = paymentCompleted;
            RegistrationFeePaidCheckBox.IsEnabled = !paymentCompleted;

            if (paymentCompleted)
            {
                RegistrationFeeTextBox.Background =
                    System.Windows.Media.Brushes.Gainsboro;
            }


            RegistrationDateTextBox.Text =
                _originalClient.RegistrationDate.ToString("dd.MM.yyyy");

            PaymentDateTextBox.Text =
                _originalClient.RegistrationFeePaidDate?
                    .ToString("dd.MM.yyyy") ?? "Не оплачено";

            ClientKindComboBox.SelectedIndex =
                _originalClient is LegalEntityClient ? 1 : 0;

            if (_originalClient is LegalEntityClient legal)
            {
                OrganizationNameTextBox.Text =
                    legal.OrganizationName;
            }

            foreach (ComboBoxItem item in ClientTypeComboBox.Items)
            {
                if (item.Content?.ToString() ==
                    _originalClient.ClientType)
                {
                    ClientTypeComboBox.SelectedItem = item;
                    break;
                }
            }
        }

        // Показываем организацию для юридического лица
        private void ClientKindComboBox_SelectionChanged(
            object sender, SelectionChangedEventArgs e)
        {
            if (OrganizationPanel == null)
                return;

            OrganizationPanel.Visibility =
                ClientKindComboBox.SelectedIndex == 1
                    ? Visibility.Visible
                    : Visibility.Collapsed;
        }

        // Сохраняем изменённые данные
        private void Save_Click(
            object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(LastNameTextBox.Text) ||
                    string.IsNullOrWhiteSpace(FirstNameTextBox.Text) ||
                    string.IsNullOrWhiteSpace(PhoneTextBox.Text) ||
                    string.IsNullOrWhiteSpace(AddressTextBox.Text) ||
                    string.IsNullOrWhiteSpace(RegistrationTextBox.Text))
                {
                    throw new ArgumentException(
                        "Заполните все обязательные поля.");
                }

                if (ClientKindComboBox.SelectedIndex < 0)
                {
                    throw new ArgumentException(
                        "Выберите тип лица.");
                }

                bool isLegal =
                    ClientKindComboBox.SelectedIndex == 1;

                if (isLegal &&
                    string.IsNullOrWhiteSpace(
                        OrganizationNameTextBox.Text))
                {
                    throw new ArgumentException(
                        "Укажите наименование организации.");
                }

               
                // Создаём объект нужного типа
                Client updated = isLegal
                    ? new LegalEntityClient
                    {
                        OrganizationName =
                            OrganizationNameTextBox.Text.Trim()
                    }
                    : new IndividualClient();

                // Сохраняем идентификатор и дату регистрации
                updated.Id = _originalClient.Id;
                updated.RegistrationDate =
                    _originalClient.RegistrationDate;

                updated.LastName = LastNameTextBox.Text.Trim();
                updated.FirstName = FirstNameTextBox.Text.Trim();
                updated.MiddleName = MiddleNameTextBox.Text.Trim();

                updated.Phone = PhoneTextBox.Text.Trim();
                updated.Email = EmailTextBox.Text.Trim();
                updated.Address = AddressTextBox.Text.Trim();

                updated.RegistrationNumber =
                    RegistrationTextBox.Text.Trim();

                // Сохраняем роль клиента
                string selectedRole = (ClientTypeComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString()?? "";

                updated.ClientType =
                    string.IsNullOrWhiteSpace(selectedRole)
                        ? _originalClient.ClientType
                        : selectedRole;

                // Не теряем дату ранее зарегистрированной оплаты


                if (_originalClient.IsRegistrationFeePaid)
                {
                    // Уже оплаченный взнос не изменяем
                    updated.RegistrationFee =
                        _originalClient.RegistrationFee;

                    updated.IsRegistrationFeePaid = true;

                    updated.RegistrationFeePaidDate =
                        _originalClient.RegistrationFeePaidDate;
                }
                else
                {
                    // Считываем сумму для ещё не оплаченного взноса
                    if (!decimal.TryParse(
                        RegistrationFeeTextBox.Text,
                        NumberStyles.Number,
                        CultureInfo.CurrentCulture,
                        out decimal fee) || fee < 0)
                    {
                        throw new ArgumentException(
                            "Введите корректную сумму регистрационного взноса.");
                    }

                    bool isPaid =
                        RegistrationFeePaidCheckBox.IsChecked == true;

                    if (isPaid && fee <= 0)
                    {
                        throw new ArgumentException(
                            "Для оплаты укажите сумму больше нуля.");
                    }

                    updated.RegistrationFee = fee;
                    updated.IsRegistrationFeePaid = isPaid;

                    updated.RegistrationFeePaidDate =
                        isPaid ? DateTime.Today : null;
                }



                _service.UpdateClient(updated);

                MessageBox.Show(
                    "Изменения успешно сохранены.",
                    "Редактирование клиента",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Ошибка сохранения",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        // Закрываем окно без сохранения
        private void Cancel_Click(
            object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
