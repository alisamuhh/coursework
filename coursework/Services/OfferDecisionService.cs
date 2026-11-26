
using System;
using System.Linq;
using System.Collections.Generic;
using coursework.Models;
using coursework.Interfaces;

namespace coursework.Services
{
    public class OfferDecisionService : IOfferDecisionService
    {
        public void Register(
            AgencyData data, Guid requestId, Guid propertyId, bool accepted,
            string comment, Func<Property, Request, bool> isMatch)
        {
            // Получаем заявку
            Request request = data.Requests.FirstOrDefault(r => r.Id == requestId)
                ?? throw new ArgumentException("Заявка не найдена.");

            // Получаем предлагаемую недвижимость
            Property property = data.Properties.FirstOrDefault(p => p.Id == propertyId)
                ?? throw new ArgumentException("Недвижимость не найдена.");

            if (!property.IsAvailable)
                throw new ArgumentException("Этот объект недвижимости уже недоступен.");

            // Решение можно принять только по подходящему объекту
            if (!isMatch(property, request))
                throw new ArgumentException("Объект не соответствует требованиям заявки.");

            comment = comment?.Trim() ?? "";

            if (comment.Length > 1000)
                throw new ArgumentException("Комментарий не должен превышать 1000 символов.");

            // При отказе обязательно указывается причина
            if (!accepted && string.IsNullOrWhiteSpace(comment))
                throw new ArgumentException("Укажите причину отказа.");

            data.OfferDecisions ??= new List<OfferDecision>();

            // Проверяем, принималось ли решение ранее
            OfferDecision? existing = data.OfferDecisions.FirstOrDefault(d =>
                d.RequestId == requestId && d.PropertyId == propertyId);

            if (existing != null)
            {
                // Обновляем существующее решение
                existing.Status = accepted ? "Принято" : "Отклонено";
                existing.DecisionDate = DateTime.Now;
                existing.Comment = comment;
            }
            else
            {
                // Создаём новую запись о решении
                data.OfferDecisions.Add(new OfferDecision
                {
                    RequestId = requestId,
                    PropertyId = propertyId,
                    Status = accepted ? "Принято" : "Отклонено",
                    DecisionDate = DateTime.Now,
                    Comment = comment
                });
            }
        }
    }
}
