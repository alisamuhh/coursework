using coursework.Models;
using coursework.Services;
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

namespace coursework
{
    /// <summary>
    /// Логика взаимодействия для MatchingRequestsWindow.xaml
    /// </summary>
    public partial class MatchingRequestsWindow : Window
    {
        public MatchingRequestsWindow()
        {
            InitializeComponent();
        }


        public MatchingRequestsWindow(
            List<Request> requests,
            AgencyService service) : this()
        {
            var rows = requests.Select(request =>
            {
                Client? client = service.Data.Clients
                    .FirstOrDefault(c => c.Id == request.ClientId);

                return new
                {
                    ClientName = client?.DisplayName ?? "Клиент не найден",
                    Phone = client?.Phone ?? "Не указан",
                    request.OperationType,
                    request.District,
                    request.MinimumRooms,
                    request.MaximumPrice,
                    request.IsSpecial,
                    request.Conditions
                };
            }).ToList();

            MatchingRequestsGrid.ItemsSource = rows;

            int specialCount = requests.Count(r => r.IsSpecial);

            SummaryTextBlock.Text =
                $"Найдено подходящих заявок: {requests.Count}. " +
                $"Специальных заказов: {specialCount}.";
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

    }
}
