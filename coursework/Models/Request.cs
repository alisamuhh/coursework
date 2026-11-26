using System;

namespace coursework.Models
{
    public class Request
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid ClientId { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public string ClientName { get; set; } = "";

        public string OperationType { get; set; } = "Покупка";

        public string Category { get; set; } = "Квартира";
        public string District { get; set; } = "";

        public int MinimumRooms { get; set; }
        public double MinimumArea { get; set; }

        public decimal MaximumPrice { get; set; }


        // Дополнительные требования к недвижимости

        // 0 — этаж не имеет значения
        public int PreferredFloor { get; set; } = 0;

        // 0 — минимальная жилая площадь не задана
        public double MinimumLivingArea { get; set; } = 0;

        // 0 — минимальная площадь кухни не задана
        public double MinimumKitchenArea { get; set; } = 0;

        // null — наличие балкона не имеет значения
        public bool? RequiresBalcony { get; set; } = null;

        // null — наличие лоджии не имеет значения
        public bool? RequiresLoggia { get; set; } = null;

        // 0 — этажность дома не имеет значения
        public int PreferredBuildingFloors { get; set; } = 0;

        // Пустая строка — любой материал дома
        public string PreferredBuildingMaterial { get; set; } = "";

        // 0 — год постройки не имеет значения
        public int MinimumConstructionYear { get; set; } = 0;

        // 0 — год ремонта не имеет значения
        public int MinimumRepairYear { get; set; } = 0;

        // null — наличие лифта не имеет значения
        public bool? RequiresElevator { get; set; } = null;

        // null — наличие мусоропровода не имеет значения
        public bool? RequiresGarbageChute { get; set; } = null;

        // null — наличие телефонного кабеля не имеет значения
        public bool? RequiresTelephoneCable { get; set; } = null;

        public bool IsSpecial { get; set; }

        public string Conditions { get; set; } = "";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public Request()
        {
        }

        public override string ToString()
        {
            return $"{OperationType}: {Category}, {District}";
        }
    }
}
