
using System;
using System.IO;
using System.Linq;
using coursework.Models;

namespace coursework.Services
{
    public static class DemoDataService
    {
        // Исходные демонстрационные данные хранятся отдельно от рабочего файла
        public static AgencyData Create()
        {
            var assembly = typeof(DemoDataService).Assembly;

            string resourceName = assembly.GetManifestResourceNames()
                .FirstOrDefault(name => name.EndsWith(".demo_data.json", StringComparison.OrdinalIgnoreCase))
                ?? throw new FileNotFoundException(
                    "Не найден встроенный ресурс demo_data.json. " +
                    "Установите для файла действие при сборке «Внедренный ресурс».");

            using Stream stream = assembly.GetManifestResourceStream(resourceName)
                ?? throw new FileNotFoundException("Не удалось открыть ресурс " + resourceName);

            using StreamReader reader = new StreamReader(stream);
            string json = reader.ReadToEnd();

            // Загружаем данные с восстановлением производных типов
            return new AgencyDataStorage().Deserialize(json);
        }

        // Сохранена совместимость со старыми демонстрационными клиентами
        // Уже заполненные сведения из рабочего JSON не перезаписываются
        public static bool UpdateDemoClients(AgencyData data)
        {
            AgencyData reference = Create();
            bool changed = false;

            for (int i = 0; i < 8 && i < reference.Clients.Count; i++)
            {
                string oldNumber = $"ДЕМО-{i + 1:000}";
                string newNumber = $"РН-2026-{i + 1:0000}";

                Client? client = data.Clients.FirstOrDefault(c =>
                    c.RegistrationNumber == oldNumber || c.RegistrationNumber == newNumber);

                if (client == null)
                    continue;

                Client source = reference.Clients[i];

                if (client.RegistrationNumber == oldNumber)
                {
                    client.RegistrationNumber = newNumber;
                    changed = true;
                }

                if (string.IsNullOrWhiteSpace(client.FirstName))
                {
                    client.FirstName = source.FirstName;
                    client.MiddleName = source.MiddleName;
                    client.Email = source.Email;
                    client.ClientType = source.ClientType;
                    client.Address = source.Address;
                    client.Phone = source.Phone;
                    changed = true;
                }
            }

            return changed;
        }
    }
}
