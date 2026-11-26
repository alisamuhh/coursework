using System;
using System.Text.Json.Serialization;

namespace coursework.Models
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
    [JsonDerivedType(typeof(ApartmentProperty), "apartment")]
    [JsonDerivedType(typeof(HouseProperty), "house")]
    [JsonDerivedType(typeof(RoomProperty), "room")]
    public class Property
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid OwnerId { get; set; }

        public string Category { get; set; } = "Квартира";
        public string OperationType { get; set; } = "Продажа";

        public string District { get; set; } = "";
        public string Address { get; set; } = "";

        public int Floor { get; set; }
        public int Rooms { get; set; }

        public double TotalArea { get; set; }
        public double LivingArea { get; set; }
        public double KitchenArea { get; set; }

        public bool HasBalcony { get; set; }
        public bool HasLoggia { get; set; }

        public decimal Price { get; set; }

        public int BuildingFloors { get; set; }
        public string BuildingMaterial { get; set; } = "";

        public int ConstructionYear { get; set; }
        public int LastRepairYear { get; set; }

        public bool HasElevator { get; set; }
        public bool HasGarbageChute { get; set; }
        public bool HasTelephoneCable { get; set; }

        public string Description { get; set; } = "";

        public string Conditions { get; set; } = "";

        public bool IsAvailable { get; set; } = true;

        [JsonIgnore]
        public string Status => OperationType switch
        {
            "Продажа" => IsAvailable ? "В продаже" : "Продано",
            "Аренда" => IsAvailable ? "Сдаётся в аренду" : "Сдано в аренду",
            _ => IsAvailable ? "Доступен" : "Недоступен"
        };

        public Property()
        {
        }

        [JsonIgnore]
        public string DisplayDescription => GetDescription();

        public virtual string GetDescription()
        {
            return $"{Category}: {Address}, {TotalArea:0.##} м²";
        }

        public override string ToString()
        {
            return $"{Category}, {District}, {Rooms} комн., {Price:N0} руб.";
        }

        [JsonIgnore]
        public string OwnerName { get; set; } = "";
    }
}