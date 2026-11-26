using System;
using System.Linq;
using System.Collections.Generic;

namespace coursework.Models
{
    // Создание объектов недвижимости нужного типа
    public static class PropertyFactory
    {

        private static readonly Dictionary<string, Func<Property>> _factories =
            new(StringComparer.Ordinal)
            {
                ["Квартира"] = () => new ApartmentProperty(),
                ["Дом"] = () => new HouseProperty(),
                ["Комната"] = () => new RoomProperty()
            };

        public static void Register(
            string category,
            Func<Property> factory)
        {
            if (string.IsNullOrWhiteSpace(category))
                throw new ArgumentException(
                    "Категория недвижимости не указана.");

            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            _factories.Add(category.Trim(), factory);
        }

        public static Property Create(string category)
        {
            string key = category?.Trim() ?? "";

            if (_factories.TryGetValue(key, out var factory))
                return factory();

            throw new ArgumentException(
                "Неизвестная категория недвижимости.");
        }


        // Преобразование старых объектов Property
        // в объекты соответствующих производных классов
        public static Property Convert(Property source)
        {

            if (source.GetType() == Create(source.Category).GetType())
                return source;

            Property result = Create(source.Category);

            foreach (var property in typeof(Property).GetProperties()
                .Where(x => x.CanRead && x.CanWrite &&
                    x.Name != nameof(Property.DisplayDescription)))
            {
                property.SetValue(result, property.GetValue(source));
            }

            return result;
        }
    }
}