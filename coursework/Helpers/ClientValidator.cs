using System;
using System.Linq;
using coursework.Models;

namespace coursework.Helpers
{
    public static class ClientValidator
    {
        public static void Validate(Client client, AgencyData data)
        {
            if (client == null)
                throw new ArgumentException("Данные клиента отсутствуют.");

            // Проверка личных данных
            if (!ValidationHelper.IsValidName(client.LastName))
                throw new ArgumentException("Фамилия должна содержать от 2 до 50 букв. Допускается дефис.");

            if (!ValidationHelper.IsValidName(client.FirstName))
                throw new ArgumentException("Имя должно содержать от 2 до 50 букв. Допускается дефис.");

            if (!string.IsNullOrWhiteSpace(client.MiddleName) &&
                !ValidationHelper.IsValidName(client.MiddleName))
                throw new ArgumentException("Отчество должно содержать только буквы. Допускается дефис.");

            // Проверка контактных данных
            if (!ValidationHelper.IsValidPhone(client.Phone))
                throw new ArgumentException("Некорректный номер телефона.");

            if (!ValidationHelper.IsValidAddress(client.Address))
                throw new ArgumentException("Некорректный адрес. Укажите название улицы и номер дома.");

            if (!ValidationHelper.IsValidEmail(client.Email))
                throw new ArgumentException("Некорректный адрес электронной почты.");

            if (!ValidationHelper.IsValidRegistrationNumber(client.RegistrationNumber))
                throw new ArgumentException("Регистрационный номер должен содержать от 3 до 30 символов.");

            // Проверка уникальности
            ClientUniquenessValidator.Validate(client, data.Clients);

            // Проверка юридического лица
            if (client is LegalEntityClient legal)
            {
                if (string.IsNullOrWhiteSpace(legal.OrganizationName))
                    throw new ArgumentException("Укажите наименование организации.");

                if (legal.OrganizationName.Trim().Length > 200)
                    throw new ArgumentException("Название организации не должно превышать 200 символов.");
            }

            // Проверка регистрационного взноса
            if (client.RegistrationFee < 0)
                throw new ArgumentException("Регистрационный взнос не может быть отрицательным.");

            if (client.IsRegistrationFeePaid)
            {
                if (client.RegistrationFee <= 0)
                    throw new ArgumentException("Оплаченный регистрационный взнос должен быть больше нуля.");

                if (!client.RegistrationFeePaidDate.HasValue)
                    throw new ArgumentException("Укажите дату оплаты регистрационного взноса.");

                if (client.RegistrationFeePaidDate.Value.Date > DateTime.Today)
                    throw new ArgumentException("Дата оплаты не может быть в будущем.");
            }
            else if (client.RegistrationFeePaidDate.HasValue)
            {
                throw new ArgumentException("У неоплаченного взноса не должно быть даты оплаты.");
            }

            // Проверка роли клиента
            string[] allowedTypes =
            {
                "Покупатель",
                "Продавец",
                "Арендатор",
                "Арендодатель",
                "Покупатель / арендатор",
                "Продавец / арендодатель",
                "Покупатель и продавец"
            };

            if (!allowedTypes.Contains(client.ClientType))
                throw new ArgumentException("Выберите корректный тип клиента.");
        }
    }
}
