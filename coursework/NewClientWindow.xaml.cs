using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using coursework.Models;
using coursework.Services;

namespace coursework
{
    public partial class NewClientWindow : Window
    {
        private readonly AgencyService _service;

        private readonly Client? _editingClient;

        public NewClientWindow(AgencyService service)
        {
            InitializeComponent();
            _service = service;
        }


        public NewClientWindow(AgencyService service, Client client)
            : this(service)
        {
            _editingClient = client;

            Title = "Редактирование клиента";

            LastNameTextBox.Text = client.LastName;
            FirstNameTextBox.Text = client.FirstName;
            MiddleNameTextBox.Text = client.MiddleName;
            PhoneTextBox.Text = client.Phone;
            EmailTextBox.Text = client.Email;
            AddressTextBox.Text = client.Address;
            RegistrationTextBox.Text = client.RegistrationNumber;

            RegistrationFeeTextBox.Text =
                client.RegistrationFee.ToString("0.##");

            RegistrationFeePaidCheckBox.IsChecked =
                client.IsRegistrationFeePaid;

            ClientKindComboBox.SelectedIndex =
                client is LegalEntityClient ? 1 : 0;

            if (client is LegalEntityClient legal)
                OrganizationNameTextBox.Text = legal.OrganizationName;

            foreach (ComboBoxItem item in ClientTypeComboBox.Items)
            {
                if (item.Content?.ToString() == client.ClientType)
                {
                    ClientTypeComboBox.SelectedItem = item;
                    break;
                }
            }
        }


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

        private void Register_Click(
            object sender, RoutedEventArgs e)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(LastNameTextBox.Text) && !coursework.Helpers.ValidationHelper.IsValidName(LastNameTextBox.Text))
                {
                    throw new ArgumentException("Фамилия должна содержать от 2 до 50 букв. " + "Цифры и специальные символы запрещены. Допусается дефис");
                }

                if (!string.IsNullOrWhiteSpace(FirstNameTextBox.Text) && !coursework.Helpers.ValidationHelper.IsValidName(FirstNameTextBox.Text))
                {
                    throw new ArgumentException("Имя должно содержать от 2 до 50 букв. " + "Цифры и специальные символы запрещены. Допусается дефис");
                }

                if (!string.IsNullOrWhiteSpace(MiddleNameTextBox.Text) && !coursework.Helpers.ValidationHelper.IsValidName(MiddleNameTextBox.Text))
                {
                    throw new ArgumentException("Отчетство должно содержать от 2 до 50 букв. " + "Цифры и специальные символы запрещены. Допусается дефис");
                }

                if (string.IsNullOrWhiteSpace(LastNameTextBox.Text) ||
                    string.IsNullOrWhiteSpace(FirstNameTextBox.Text) ||
                    string.IsNullOrWhiteSpace(PhoneTextBox.Text) ||
                    string.IsNullOrWhiteSpace(AddressTextBox.Text) ||
                    string.IsNullOrWhiteSpace(RegistrationTextBox.Text))
                {
                    throw new ArgumentException(
                        "Заполните все обязательные поля.");
                }

                if (ClientTypeComboBox.SelectedItem is not ComboBoxItem role)
                {
                    throw new ArgumentException(
                        "Выберите роль клиента.");
                }

                bool isLegal = ClientKindComboBox.SelectedIndex == 1;

                if (isLegal &&
                    string.IsNullOrWhiteSpace(OrganizationNameTextBox.Text))
                {
                    throw new ArgumentException(
                        "Укажите наименование организации.");
                }

                if (!decimal.TryParse(
                    RegistrationFeeTextBox.Text,
                    NumberStyles.Number,
                    CultureInfo.CurrentCulture,
                    out decimal fee) || fee < 0)
                {
                    throw new ArgumentException(
                        "Введите корректный регистрационный взнос.");
                }

                if (RegistrationFeePaidCheckBox.IsChecked == true && fee <= 0)
                {
                    throw new ArgumentException(
                        "Для подтверждения оплаты укажите сумму больше нуля.");
                }

                Client client = isLegal
                    ? new LegalEntityClient
                    {
                        OrganizationName =
                            OrganizationNameTextBox.Text.Trim()
                    }
                    : new IndividualClient();

                client.LastName = LastNameTextBox.Text.Trim();
                client.FirstName = FirstNameTextBox.Text.Trim();
                client.MiddleName = MiddleNameTextBox.Text.Trim();
                client.Phone = PhoneTextBox.Text.Trim();
                client.Email = EmailTextBox.Text.Trim();
                client.Address = AddressTextBox.Text.Trim();
                client.RegistrationNumber =
                    RegistrationTextBox.Text.Trim();

                client.ClientType =
                    role.Content.ToString() ?? "";

                client.RegistrationFee = fee;

                client.IsRegistrationFeePaid =
                    RegistrationFeePaidCheckBox.IsChecked == true;

                client.RegistrationFeePaidDate =
                    client.IsRegistrationFeePaid
                        ? DateTime.Now
                        : null;


                if (_editingClient == null)
                {
                    _service.AddClient(client);
                }
                else
                {
                    client.Id = _editingClient.Id;
                    client.RegistrationDate = _editingClient.RegistrationDate;

                    if (client.IsRegistrationFeePaid)
                    {
                        client.RegistrationFeePaidDate =
                            _editingClient.IsRegistrationFeePaid &&
                            _editingClient.RegistrationFeePaidDate.HasValue
                                ? _editingClient.RegistrationFeePaidDate
                                : DateTime.Today;
                    }

                    _service.UpdateClient(client);
                }


                MessageBox.Show(
    _editingClient == null
        ? "Клиент успешно зарегистрирован."
        : "Данные клиента обновлены.",
    "Клиенты",
    MessageBoxButton.OK,
    MessageBoxImage.Information);

                DialogResult = true;
            }
            catch (Exception ex)
            {

                MessageBox.Show(
                    ex.Message,
                    "Ошибка регистрации",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        private void Cancel_Click(
            object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
