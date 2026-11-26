
using System;
using coursework.Models;
using coursework.Interfaces;

namespace coursework.Services
{
    public class DealProcessingService : IDealProcessingService
    {
        public void CompleteDeal(
            Deal deal,
            Property property,
            AgencyData data)
        {
            // Фиксируем участников и характеристики недвижимости
            // на момент оформления сделки.
            deal.SellerId = property.OwnerId;
            deal.OperationType = property.OperationType;
            deal.PropertyCategory = property.Category;
            deal.PropertyAddress = property.Address;
            deal.PropertyDistrict = property.District;
            deal.PropertyTotalArea = property.TotalArea;
            deal.PropertyRooms = property.Rooms;
            deal.PropertyConditions = property.Conditions;

            deal.DealDate = DateTime.Now;

            data.Deals.Add(deal);

            // При продаже собственником становится покупатель.
            // При аренде собственник остаётся прежним.
            if (property.OperationType == "Продажа")
                property.OwnerId = deal.ClientId;

            // Исключаем объект из дальнейшего подбора.
            property.IsAvailable = false;
        }
    }
}
