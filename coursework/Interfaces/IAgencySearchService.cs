using System.Collections.Generic;
using coursework.Models;

namespace coursework.Interfaces
{
    public interface IAgencySearchService
    {
        List<Client> SearchClients(
            AgencyData data,
            string searchText);

        List<Property> SearchProperties(
            AgencyData data,
            string searchText,
            string category,
            string operation);
    }
}
