using System;
using System.Linq;
using coursework.Models;

namespace coursework.Helpers
{
    public static class RequestValidator
    {
        public static void Validate(Request request, AgencyData data)
        {
            if (request == null)
                throw new ArgumentException("Данные заявки отсутствуют.");

            // Проверка клиента
            if (!data.Clients.Any(c => c.Id == request.ClientId))
                throw new ArgumentException("Выберите существующего клиента.");

            // Проверка типа операции
            if (request.OperationType != "Покупка" && request.OperationType != "Аренда")
                throw new ArgumentException("Тип заявки должен быть «Покупка» или «Аренда».");

            // Проверка категории
            if (string.IsNullOrWhiteSpace(request.Category))
                throw new ArgumentException("Выберите категорию недвижимости.");

            // Район может быть пустым — поиск по всем районам
            if (!string.IsNullOrWhiteSpace(request.District) &&
                !ValidationHelper.IsValidDistrict(request.District))
                throw new ArgumentException("Некорректный район. Используйте буквы и дефисы.");

            // Ноль означает отсутствие ограничения по количеству комнат
            if (request.MinimumRooms < 0 || request.MinimumRooms > 100)
                throw new ArgumentException("Минимальное количество комнат должно быть от 0 до 100.");

            // Проверка минимальной площади
            if (double.IsNaN(request.MinimumArea) ||
                double.IsInfinity(request.MinimumArea) ||
                request.MinimumArea < 0 || request.MinimumArea > 100000)
                throw new ArgumentException("Минимальная площадь должна быть от 0 до 100000 м².");

            if (request.MinimumArea > 0 &&
                request.MinimumLivingArea > 0 &&
                request.MinimumKitchenArea > 0 &&
                request.MinimumLivingArea + request.MinimumKitchenArea > request.MinimumArea)
            {
                throw new ArgumentException("Сумма минимальной жилой площади и площади кухни " +
                    "не может превышать минимальную общую площадь.");
            }

            // Проверка максимальной цены
            if (request.MaximumPrice <= 0 || request.MaximumPrice > 1000000000000m)
                throw new ArgumentException("Укажите положительную максимальную цену.");

            if (request.MaximumPrice < 3000)
                throw new ArgumentException("Максимальная цена в заявке должна быть не менее 3000 рублей.");

            // Дополнительные пожелания
            if (request.Conditions != null && request.Conditions.Length > 2000)
                throw new ArgumentException("Дополнительные условия не должны превышать 2000 символов.");

            if (request.IsSpecial && string.IsNullOrWhiteSpace(request.Conditions))
                throw new ArgumentException("Для специального заказа укажите индивидуальные условия.");

            if (request.PreferredFloor < 0 || request.PreferredFloor > 200)
                throw new ArgumentException("Предпочитаемый этаж должен быть от 0 до 200.");

            if (request.PreferredBuildingFloors < 0 || request.PreferredBuildingFloors > 200)
                throw new ArgumentException("Этажность дома должна быть от 0 до 200.");

            if (request.PreferredFloor > 0 &&
                request.PreferredBuildingFloors > 0 &&
                request.PreferredFloor > request.PreferredBuildingFloors)
                throw new ArgumentException("Предпочитаемый этаж не может превышать этажность дома.");

            if (double.IsNaN(request.MinimumLivingArea) ||
                double.IsInfinity(request.MinimumLivingArea) ||
                request.MinimumLivingArea < 0 || request.MinimumLivingArea > 100000)
                throw new ArgumentException("Некорректная минимальная жилая площадь.");

            if (double.IsNaN(request.MinimumKitchenArea) ||
                double.IsInfinity(request.MinimumKitchenArea) ||
                request.MinimumKitchenArea < 0 || request.MinimumKitchenArea > 100000)
                throw new ArgumentException("Некорректная минимальная площадь кухни.");

            if (request.MinimumLivingArea + request.MinimumKitchenArea > 100000)
                throw new ArgumentException("Сумма минимальных площадей слишком велика.");

            if (request.MinimumConstructionYear != 0 &&
                (request.MinimumConstructionYear < 1700 ||
                 request.MinimumConstructionYear > DateTime.Today.Year))
                throw new ArgumentException("Некорректный минимальный год постройки.");

            if (request.MinimumRepairYear != 0 &&
                (request.MinimumRepairYear < 1700 ||
                 request.MinimumRepairYear > DateTime.Today.Year))
                throw new ArgumentException("Некорректный минимальный год капитального ремонта.");

            if (request.MinimumConstructionYear > 0 &&
                request.MinimumRepairYear > 0 &&
                request.MinimumRepairYear < request.MinimumConstructionYear)
            {
                throw new ArgumentException("Минимальный год капитального ремонта " +
                    "не может быть раньше минимального года постройки.");
            }

            if (request.PreferredBuildingMaterial != null &&
                request.PreferredBuildingMaterial.Length > 100)
                throw new ArgumentException("Слишком длинное название материала здания.");
        }
    }
}
