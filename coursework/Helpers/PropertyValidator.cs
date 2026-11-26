using System;
using System.Linq;
using coursework.Models;
using coursework.Services;

namespace coursework.Helpers
{
    public static class PropertyValidator
    {
        public static void Validate(Property property, AgencyData data)
        {
            if (property == null)
                throw new ArgumentException("Данные недвижимости отсутствуют.");

            // Проверка владельца
            if (!data.Clients.Any(c => c.Id == property.OwnerId))
                throw new ArgumentException("Выберите существующего владельца.");

            // Проверка категории и операции
            if (string.IsNullOrWhiteSpace(property.Category))
                throw new ArgumentException("Выберите категорию недвижимости.");

            if (property.OperationType != "Продажа" && property.OperationType != "Аренда")
                throw new ArgumentException("Некорректный тип операции.");

            if (property.GetType() != PropertyFactory.Create(property.Category).GetType())
                throw new ArgumentException("Тип объекта не соответствует категории недвижимости.");

            // Проверка района
            if (!ValidationHelper.IsValidDistrict(property.District))
                throw new ArgumentException("Район должен содержать только буквы, пробелы и дефисы.");

            // Проверка адреса
            if (!ValidationHelper.IsValidAddress(property.Address))
                throw new ArgumentException("Некорректный адрес недвижимости.");

            // Проверка количества комнат
            if (property.Rooms < 1 || property.Rooms > 100)
                throw new ArgumentException("Количество комнат должно быть от 1 до 100.");

            // Проверка этажности
            if (property.BuildingFloors < 1 || property.BuildingFloors > 200)
                throw new ArgumentException("Этажность здания должна быть от 1 до 200.");

            if (property.Floor < 1 || property.Floor > property.BuildingFloors)
                throw new ArgumentException("Этаж должен быть от 1 до этажности здания.");

            // Проверка площадей
            if (double.IsNaN(property.TotalArea) ||
                double.IsInfinity(property.TotalArea) ||
                property.TotalArea <= 0 || property.TotalArea > 100000)
                throw new ArgumentException("Некорректная общая площадь.");

            if (double.IsNaN(property.LivingArea) ||
                double.IsInfinity(property.LivingArea) ||
                property.LivingArea < 0 || property.LivingArea > property.TotalArea)
                throw new ArgumentException("Некорректная жилая площадь.");

            if (double.IsNaN(property.KitchenArea) ||
                double.IsInfinity(property.KitchenArea) ||
                property.KitchenArea < 0 || property.KitchenArea > property.TotalArea)
                throw new ArgumentException("Некорректная площадь кухни.");

            if (property.LivingArea + property.KitchenArea > property.TotalArea)
                throw new ArgumentException("Сумма жилой площади и площади кухни " +
                    "не может превышать общую площадь.");

            // Проверка цены
            if (property.Price <= 0 || property.Price > 1000000000000m)
                throw new ArgumentException("Цена должна быть положительной и не превышать 1 трлн рублей.");

            // Проверка материала здания
            if (string.IsNullOrWhiteSpace(property.BuildingMaterial) ||
                !property.BuildingMaterial.Any(char.IsLetter) ||
                property.BuildingMaterial.Length > 100)
                throw new ArgumentException("Укажите корректный материал здания.");

            // Проверка годов
            int currentYear = DateTime.Today.Year;

            if (property.ConstructionYear < 1700 || property.ConstructionYear > currentYear)
                throw new ArgumentException($"Год постройки должен быть от 1700 до {currentYear}.");

            // Ноль означает, что капитальный ремонт не проводился
            if (property.LastRepairYear != 0 &&
                (property.LastRepairYear < property.ConstructionYear ||
                 property.LastRepairYear > currentYear))
                throw new ArgumentException("Год капитального ремонта не может быть раньше " +
                    "года постройки или позже текущего года.");

            // Проверка дополнительных условий
            if (property.Conditions != null && property.Conditions.Length > 2000)
                throw new ArgumentException("Описание условий слишком длинное (максимум 2000 символов).");
        }
    }
}
