
using coursework.Models;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace coursework
{
    public partial class DealContractWindow : Window
    {
        public DealContractWindow(
            Deal deal,
            Client buyer,
            Client seller,
            Property property)
        {
            InitializeComponent();

            bool isRent = property.OperationType == "Аренда";

            string documentName = isRent
                ? "СВЕДЕНИЯ ДЛЯ ДОГОВОРА АРЕНДЫ"
                : "СВЕДЕНИЯ ДЛЯ ДОГОВОРА КУПЛИ-ПРОДАЖИ";

            FlowDocument document = new FlowDocument
            {
                PagePadding = new Thickness(45),
                FontFamily = new System.Windows.Media.FontFamily("Times New Roman"),
                FontSize = 14
            };

            Paragraph title = new Paragraph(new Run(documentName))
            {
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center
            };

            document.Blocks.Add(title);

            AddText(document, $"Номер сделки: {deal.Id}");
            AddText(document, $"Дата оформления: {deal.DealDate:dd.MM.yyyy}");

            AddText(document, "1. СТОРОНЫ", true);
            AddText(document,
                $"Продавец / арендодатель: {seller.DisplayName}");
            AddText(document,
                $"Покупатель / арендатор: {buyer.DisplayName}");

            AddText(document, "2. ОБЪЕКТ НЕДВИЖИМОСТИ", true);
            AddText(document, $"Категория: {property.Category}");
            AddText(document, $"Адрес: {property.Address}");
            AddText(document, $"Район: {property.District}");
            AddText(document, $"Общая площадь: {property.TotalArea:N2} м²");
            AddText(document, $"Количество комнат: {property.Rooms}");

            AddText(document, "3. ФИНАНСОВЫЕ УСЛОВИЯ", true);
            AddText(document,
                $"Сумма сделки: {deal.Amount:N2} руб.");
            AddText(document,
                $"Вознаграждение агентства (2%): {deal.Commission:N2} руб.");

            AddText(document, "4. ДОПОЛНИТЕЛЬНЫЕ УСЛОВИЯ", true);
            AddText(document,
                string.IsNullOrWhiteSpace(property.Conditions)
                    ? "Дополнительные условия не указаны."
                    : property.Conditions);

            AddText(document, "5. ПОДПИСИ", true);
            AddText(document, "Продавец / арендодатель: __________________");
            AddText(document, "Покупатель / арендатор: __________________");
            AddText(document, "Представитель агентства: __________________");

            AddText(document,
                "Документ сформирован информационной системой агентства. " +
                "Для подписания полноценного договора необходимо согласовать " +
                "его правовые условия и реквизиты сторон.");

            DocumentViewer.Document = document;
        }

        private static void AddText(
            FlowDocument document,
            string text,
            bool isHeader = false)
        {
            Paragraph paragraph = new Paragraph(new Run(text))
            {
                Margin = new Thickness(0, isHeader ? 18 : 5, 0, 5),
                FontWeight = isHeader
                    ? FontWeights.Bold
                    : FontWeights.Normal
            };

            document.Blocks.Add(paragraph);
        }

        private void Print_Click(object sender, RoutedEventArgs e)
        {
            PrintDialog dialog = new PrintDialog();

            if (dialog.ShowDialog() != true)
                return;

            FlowDocument document = DocumentViewer.Document;

            dialog.PrintDocument(
                ((IDocumentPaginatorSource)document).DocumentPaginator,
                "Документ по сделке");
        }
    }
}
