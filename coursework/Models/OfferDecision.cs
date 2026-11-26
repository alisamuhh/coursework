using System;

namespace coursework.Models
{
    public class OfferDecision
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        // Заявка, по которой предложили недвижимость
        public Guid RequestId { get; set; }

        // Предложенный объект
        public Guid PropertyId { get; set; }

        // "Принято" или "Отклонено"
        public string Status { get; set; } = "";

        // Дата решения клиента
        public DateTime DecisionDate { get; set; } = DateTime.Now;

        // Причина отказа или комментарий
        public string Comment { get; set; } = "";
    }
}
