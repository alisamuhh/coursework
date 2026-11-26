using System;

namespace coursework.Models
{
    public class Deal
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        // Покупатель или арендатор
        public Guid ClientId { get; set; }

        // Продавец или собственник
        public Guid SellerId { get; set; }

        // Объект недвижимости
        public Guid PropertyId { get; set; }

        public string OperationType { get; set; } = "";
        public string PropertyCategory { get; set; } = "";
        public string PropertyAddress { get; set; } = "";
        public string PropertyDistrict { get; set; } = "";
        public double? PropertyTotalArea { get; set; }
        public int? PropertyRooms { get; set; }
        public string? PropertyConditions { get; set; }


        // Дата оформления сделки
        public DateTime DealDate { get; set; } = DateTime.Today;

        // Сумма сделки
        public decimal Amount { get; set; }

        // Комиссия агентства — 2%

        public decimal Commission
        {
            get
            {
                return Math.Round(Amount * 0.02m, 2,
                    MidpointRounding.AwayFromZero);
            }
        }


        public Deal()
        {
        }

        public Deal(Guid clientId, Guid propertyId, decimal amount)
        {
            ClientId = clientId;
            PropertyId = propertyId;
            Amount = amount;
        }
    }
}
