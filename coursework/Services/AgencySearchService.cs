
using System;
using System.Collections.Generic;
using System.Linq;
using coursework.Interfaces;
using coursework.Models;

namespace coursework.Services
{
    public class AgencySearchService : IAgencySearchService
    {
        public List<Client> SearchClients(AgencyData data, string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return data.Clients.ToList();

            searchText = searchText.Trim();

            return data.Clients
                .Where(c =>
                    c.FullName.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                    c.Phone.Contains(searchText) ||
                    c.RegistrationNumber.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                    (c is LegalEntityClient legal &&
                     legal.OrganizationName.Contains(searchText, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }

        public List<Property> SearchProperties(
            AgencyData data, string searchText, string category, string operation)
        {
            IEnumerable<Property> query = data.Properties;

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                searchText = searchText.Trim();

                query = query.Where(p =>
                    p.Address.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                    p.District.Contains(searchText, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(category) && category != "Все категории")
                query = query.Where(p => p.Category == category);

            if (!string.IsNullOrWhiteSpace(operation) && operation != "Все операции")
                query = query.Where(p => p.OperationType == operation);

            return query.OrderBy(p => p.District)
                .ThenBy(p => p.Address)
                .ToList();
        }
    }
}
