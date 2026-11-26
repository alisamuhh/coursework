
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using coursework.Models;

namespace coursework
{
    public partial class MatchingPropertiesReportWindow : Window
    {
        public MatchingPropertiesReportWindow(
            Client client,
            Request request,
            List<Property> properties)
        {
            InitializeComponent();

            FlowDocument document = new FlowDocument
            {
                FontFamily = new FontFamily("Arial"),
                FontSize = 12,
                PagePadding = new Thickness(35),
                ColumnWidth = 1000
            };

            // Заголовок документа
            Paragraph title = new Paragraph(
                new Run("СПИСОК ПОДХОДЯЩИХ ПРЕДЛОЖЕНИЙ"))
            {
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 15)
            };
            document.Blocks.Add(title);

            AddText(document,
                $"Дата формирования: {DateTime.Today:dd.MM.yyyy}");

            AddText(document,
                $"Клиент: {client.FullName}");

            AddText(document,
                $"Регистрационный номер: {client.RegistrationNumber}");

            AddText(document,
                $"Контактный телефон: {client.Phone}");

            AddHeading(document, "Пожелания клиента");

            AddText(document,
                $"Тип заявки: {request.OperationType}");

            AddText(document,
                $"Категория недвижимости: {request.Category}");

            AddText(document,
                $"Район: {(string.IsNullOrWhiteSpace(request.District) ? "Любой" : request.District)}");

            AddText(document,
                $"Минимальное количество комнат: {request.MinimumRooms}");

            AddText(document,
                $"Минимальная площадь: {request.MinimumArea:N1} м²");

            AddText(document,
                $"Максимальная цена: {request.MaximumPrice:N2} руб.");

            if (!string.IsNullOrWhiteSpace(request.Conditions))
            {
                AddText(document,
                    $"Дополнительные пожелания: {request.Conditions}");
            }

            AddHeading(document,
                $"Подходящие предложения — {properties.Count}");

            if (properties.Count == 0)
            {
                AddText(document,
                    "На дату формирования документа подходящих объектов не найдено.");
            }
            else
            {
                Table table = new Table
                {
                    CellSpacing = 0
                };

                table.Columns.Add(new TableColumn
                {
                    Width = new GridLength(35)
                });
                table.Columns.Add(new TableColumn
                {
                    Width = new GridLength(85)
                });
                table.Columns.Add(new TableColumn
                {
                    Width = new GridLength(175)
                });
                table.Columns.Add(new TableColumn
                {
                    Width = new GridLength(55)
                });
                table.Columns.Add(new TableColumn
                {
                    Width = new GridLength(75)
                });
                table.Columns.Add(new TableColumn
                {
                    Width = new GridLength(110)
                });

                TableRowGroup group = new TableRowGroup();
                table.RowGroups.Add(group);

                AddRow(group, true,
                    "№", "Категория", "Адрес",
                    "Комнат", "Площадь", "Цена, руб.");

                for (int i = 0; i < properties.Count; i++)
                {
                    Property property = properties[i];

                    AddRow(group, false,
                        (i + 1).ToString(),
                        property.Category,
                        property.Address,
                        property.Rooms.ToString(),
                        $"{property.TotalArea:N1} м²",
                        $"{property.Price:N2}");
                }

                document.Blocks.Add(table);
            }

            Paragraph footer = new Paragraph(
                new Run("Информация актуальна на дату формирования документа."))
            {
                FontSize = 10,
                Foreground = Brushes.Gray,
                Margin = new Thickness(0, 20, 0, 0)
            };

            document.Blocks.Add(footer);

            DocumentViewer.Document = document;
        }

        private static void AddText(
            FlowDocument document, string value)
        {
            document.Blocks.Add(
                new Paragraph(new Run(value))
                {
                    Margin = new Thickness(0, 2, 0, 2)
                });
        }

        private static void AddHeading(
            FlowDocument document, string value)
        {
            document.Blocks.Add(
                new Paragraph(new Run(value))
                {
                    FontSize = 15,
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(0, 18, 0, 8),
                });
        }

        private static void AddRow(
            TableRowGroup group,
            bool isHeader,
            params string[] values)
        {
            TableRow row = new TableRow();

            if (isHeader)
                row.Background = new SolidColorBrush(
                    Color.FromRgb(226, 232, 240));

            foreach (string value in values)
            {
                TableCell cell = new TableCell(
                    new Paragraph(new Run(value))
                    {
                        Margin = new Thickness(0)
                    })
                {
                    Padding = new Thickness(5),
                    BorderBrush = Brushes.LightGray,
                    BorderThickness = new Thickness(0.5),
                    FontWeight = isHeader
                        ? FontWeights.Bold
                        : FontWeights.Normal
                };

                row.Cells.Add(cell);
            }

            group.Rows.Add(row);
        }

        private void Print_Click(
            object sender, RoutedEventArgs e)
        {
            PrintDialog dialog = new PrintDialog();

            if (dialog.ShowDialog() == true)
            {
                FlowDocument document = DocumentViewer.Document;

                document.PageWidth =
                    dialog.PrintableAreaWidth;

                document.PageHeight =
                    dialog.PrintableAreaHeight;

                document.ColumnWidth =
                    dialog.PrintableAreaWidth;

                dialog.PrintDocument(
                    ((IDocumentPaginatorSource)document).DocumentPaginator,
                    "Список подходящих предложений");
            }
        }

        private void Close_Click(
            object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
