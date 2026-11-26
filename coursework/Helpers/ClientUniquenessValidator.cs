
using coursework.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace coursework.Helpers
{
    public static class ClientUniquenessValidator
    {
        public static void Validate(Client client, IEnumerable<Client> clients)
        {
            if (clients.Any(c => c.Id != client.Id &&
                string.Equals(c.RegistrationNumber?.Trim(), client.RegistrationNumber?.Trim(),
                    StringComparison.OrdinalIgnoreCase)))
            {
                throw new ArgumentException("Клиент с таким регистрационным номером уже существует.");
            }

            string phone = NormalizePhone(client.Phone);

            if (clients.Any(c => c.Id != client.Id && NormalizePhone(c.Phone) == phone))
            {
                throw new ArgumentException("Клиент с таким номером телефона уже зарегистрирован.");
            }

            if (!string.IsNullOrWhiteSpace(client.Email) &&
                clients.Any(c => c.Id != client.Id &&
                    string.Equals(c.Email?.Trim(), client.Email.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                throw new ArgumentException("Клиент с такой электронной почтой уже зарегистрирован.");
            }
        }

        private static string NormalizePhone(string? phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return "";

            string digits = new string(phone.Where(char.IsDigit).ToArray());

            if (digits.Length == 11 && digits[0] == '8')
                digits = "7" + digits.Substring(1);

            return digits;
        }
    }
}
