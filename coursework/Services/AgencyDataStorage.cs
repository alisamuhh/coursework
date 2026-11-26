
using coursework.Models;
using coursework.Interfaces;
using System;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;



namespace coursework.Services
{
    public class AgencyDataStorage : IAgencyDataStorage
    {
        private readonly string _filePath =
            Path.Combine(AppContext.BaseDirectory, "agency_data.json");

        public void Save(AgencyData data)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            string json = JsonSerializer.Serialize(data, options);

            File.WriteAllText(_filePath, json, Encoding.UTF8);
        }

        public bool Exists()
        {
            return File.Exists(_filePath);
        }

        public string Read()
        {
            return File.ReadAllText(_filePath, Encoding.UTF8);
        }

        public AgencyData Deserialize(string json)
        {
            json = AddDiscriminatorsToLegacyJson(json);

            AgencyData data = JsonSerializer.Deserialize<AgencyData>(json)
                ?? throw new InvalidDataException(
                    "Не удалось прочитать данные агентства.");

            data.Clients ??= new();
            data.Properties ??= new();
            data.Requests ??= new();
            data.Deals ??= new();
            data.OfferDecisions ??= new();

            return data;
        }

        private static string AddDiscriminatorsToLegacyJson(string json)
        {
            JsonNode? root = JsonNode.Parse(json);

            if (root is not JsonObject document)
                return json;

            // Старые клиенты становятся физическими лицами
            if (document["Clients"] is JsonArray clients)
            {
                foreach (JsonNode? node in clients)
                {
                    if (node is not JsonObject client)
                        continue;

                    if (!client.ContainsKey("$clientType"))
                    {
                        client.Insert(0, "$clientType", "individual");
                    }
                }
            }

            // Старую недвижимость переводим в дочерние классы
            if (document["Properties"] is JsonArray properties)
            {
                foreach (JsonNode? node in properties)
                {
                    if (node is not JsonObject property)
                        continue;

                    if (property.ContainsKey("$type"))
                        continue;

                    string? category =
                        property["Category"]?.GetValue<string>();

                    string type = category switch
                    {
                        "Квартира" => "apartment",
                        "Дом" => "house",
                        "Комната" => "room",
                        _ => throw new InvalidDataException(
                            "Неизвестная категория недвижимости: " +
                            category)
                    };

                    property.Insert(0, "$type", type);
                }
            }

            return document.ToJsonString();
        }
    }

}
