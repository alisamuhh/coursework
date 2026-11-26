using System;
using System.Linq;
using System.Text.RegularExpressions;
namespace coursework.Helpers
{ 

public static class ValidationHelper
{
    // Проверка имени, фамилии и отчества
    public static bool IsValidName(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        value = value.Trim();

        // От 2 до 50 символов, только буквы и дефисы
        if (value.Length < 2 || value.Length > 50)
            return false;

        return Regex.IsMatch(
            value,
            @"^\p{L}+(?:-\p{L}+)*$");
    }

    // Проверка номера телефона
    public static bool IsValidPhone(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        // Запрещаем буквы и посторонние символы
        if (!Regex.IsMatch(value,
            @"^\+?[\d\s()-]+$"))
            return false;

        string digits = new string(
            value.Where(char.IsDigit).ToArray());

        return digits.Length >= 11 &&
               digits.Length <= 12;
    }

        // Проверка адреса

        public static bool IsValidAddress(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            value = value.Trim();

            // Ограничиваем слишком короткие и длинные адреса
            if (value.Length < 8 || value.Length > 200)
                return false;

            // Только допустимые символы адреса
            if (!Regex.IsMatch(value,
                @"^[\p{L}\p{N}\s.,/\-№()]+$"))
                return false;

            // Проверяем наличие номера дома
            if (!Regex.IsMatch(value, @"\d+[\p{L}]?(?:/\d+)?"))
                return false;

            // Адрес должен содержать обозначение улицы
            // или название перед номером дома
            bool hasStreetType = Regex.IsMatch(value,
                @"(?i)(?:ул\.?|улица|проспект|пр-т|проезд|" +
                @"переулок|пер\.?|шоссе|бульвар|площадь|" +
                @"наб\.?|набережная|тракт|линия)\s+[\p{L}]");

            bool hasStreetAndHouse = Regex.IsMatch(value,
                @"\p{L}[\p{L}\s.-]*,\s*(?:д\.?\s*)?\d+");

            if (!hasStreetType && !hasStreetAndHouse)
                return false;

            return true;
        }

        public static bool IsValidDistrict(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            value = value.Trim();

            if (value.Length < 2 || value.Length > 100)
                return false;

            return Regex.IsMatch(
                value,
                @"^[\p{L}][\p{L}\s\-]*$");
        }

        // Проверка электронной почты
        public static bool IsValidEmail(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return true; // Почта необязательна

        return Regex.IsMatch(
            value.Trim(),
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
    }

    // Проверка регистрационного номера
    public static bool IsValidRegistrationNumber(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        return Regex.IsMatch(
            value.Trim(),
            @"^[\p{L}\d-]{3,30}$");
    }
}

}

