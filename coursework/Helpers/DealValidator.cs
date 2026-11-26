using System;
using System.Linq;
using coursework.Models;

namespace coursework.Helpers
{
    public static class DealValidator
    {
        public static Property Validate(Deal deal, AgencyData data, Func<Property, Request, bool> isMatch)
        {
            if (deal == null)
                throw new ArgumentException("Данные сделки отсутствуют.");

            if (!data.Clients.Any(c => c.Id == deal.ClientId))
                throw new ArgumentException("Покупатель или арендатор не найден.");

            Property property = data.Properties.FirstOrDefault(p => p.Id == deal.PropertyId)
                ?? throw new ArgumentException("Объект недвижимости не найден.");

            if (!property.IsAvailable)
                throw new InvalidOperationException("Объект недвижимости уже недоступен.");

            if (!data.Clients.Any(c => c.Id == property.OwnerId))
                throw new ArgumentException("Владелец недвижимости не найден.");

            if (property.OwnerId == deal.ClientId)
                throw new ArgumentException("Нельзя оформить сделку с самим собой.");

            if (deal.Amount <= 0 || deal.Amount > 1000000000000m)
                throw new ArgumentException("Сумма сделки должна быть больше нуля " +
                    "и не превышать 1 трлн рублей.");

            bool accepted = data.OfferDecisions != null &&
                data.OfferDecisions.Any(d =>
                    d.PropertyId == property.Id &&
                    d.Status == "Принято" &&
                    data.Requests.Any(r =>
                        r.Id == d.RequestId &&
                        r.ClientId == deal.ClientId &&
                        isMatch(property, r)));

            if (!accepted)
                throw new InvalidOperationException("Нельзя оформить сделку без согласия клиента. " +
                    "Сначала необходимо принять предложение по подходящей заявке.");

            return property;
        }
    }
}
