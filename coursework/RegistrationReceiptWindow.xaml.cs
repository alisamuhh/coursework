using System;
using System.Windows;
using System.Windows.Controls;
using coursework.Models;

namespace coursework
{
    public partial class RegistrationReceiptWindow : Window
    {
        public RegistrationReceiptWindow(Client client)
        {
            InitializeComponent();

            // Заполняем квитанцию данными клиента
            NumberText.Text =
                $"Квитанция № {client.RegistrationNumber}";

            DateText.Text =
                $"Дата оплаты: {client.RegistrationFeePaidDate:dd.MM.yyyy}";

            ClientText.Text =
                $"Принято от: {client.DisplayName}";

            AddressText.Text =
                $"Адрес: {client.Address}";

            AmountText.Text =
                $"Сумма: {client.RegistrationFee:N2} руб.";
        }

        // Печать квитанции
        private void Print_Click(object sender, RoutedEventArgs e)
        {
            PrintDialog printDialog = new PrintDialog();

            if (printDialog.ShowDialog() == true)
            {
                printDialog.PrintVisual(
                    ReceiptContent,
                    "Квитанция об уплате регистрационного взноса");
            }
        }

        // Закрытие окна
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
