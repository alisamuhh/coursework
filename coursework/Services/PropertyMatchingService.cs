
using System;
using coursework.Models;
using coursework.Interfaces;

namespace coursework.Services
{
    public class PropertyMatchingService : IPropertyMatchingService
    {
        public bool IsMatch(Property property, Request request)
        {
            if (property == null || request == null)
                return false;

            // Объект должен быть доступен
            if (!property.IsAvailable)
                return false;

            // Покупка соответствует продаже
            string requiredOperation = request.OperationType == "Покупка" ? "Продажа" : "Аренда";

            if (property.OperationType != requiredOperation)
                return false;

            // Клиент не должен получать собственный объект
            if (property.OwnerId == request.ClientId)
                return false;

            // Категория
            if (property.Category != request.Category)
                return false;

            // Район
            if (!string.IsNullOrWhiteSpace(request.District) &&
                !string.Equals(property.District?.Trim(), request.District.Trim(),
                    StringComparison.OrdinalIgnoreCase))
                return false;

            // Количество комнат
            if (property.Rooms < request.MinimumRooms)
                return false;

            // Общая площадь
            if (property.TotalArea < request.MinimumArea)
                return false;

            // Максимальная цена
            if (property.Price > request.MaximumPrice)
                return false;

            // Предпочитаемый этаж
            if (request.PreferredFloor > 0 && property.Floor != request.PreferredFloor)
                return false;

            // Этажность дома
            if (request.PreferredBuildingFloors > 0 &&
                property.BuildingFloors != request.PreferredBuildingFloors)
                return false;

            // Жилая площадь
            if (property.LivingArea < request.MinimumLivingArea)
                return false;

            // Площадь кухни
            if (property.KitchenArea < request.MinimumKitchenArea)
                return false;

            // Материал здания
            if (!string.IsNullOrWhiteSpace(request.PreferredBuildingMaterial) &&
                !string.Equals(property.BuildingMaterial?.Trim(),
                    request.PreferredBuildingMaterial.Trim(), StringComparison.OrdinalIgnoreCase))
                return false;

            // Год постройки
            if (request.MinimumConstructionYear > 0 &&
                property.ConstructionYear < request.MinimumConstructionYear)
                return false;

            // Год ремонта
            if (request.MinimumRepairYear > 0 &&
                property.LastRepairYear < request.MinimumRepairYear)
                return false;

            // Удобства
            if (request.RequiresBalcony.HasValue &&
                property.HasBalcony != request.RequiresBalcony.Value)
                return false;

            if (request.RequiresLoggia.HasValue &&
                property.HasLoggia != request.RequiresLoggia.Value)
                return false;

            if (request.RequiresElevator.HasValue &&
                property.HasElevator != request.RequiresElevator.Value)
                return false;

            if (request.RequiresGarbageChute.HasValue &&
                property.HasGarbageChute != request.RequiresGarbageChute.Value)
                return false;

            if (request.RequiresTelephoneCable.HasValue &&
                property.HasTelephoneCable != request.RequiresTelephoneCable.Value)
                return false;

            return true;
        }
    }
}
