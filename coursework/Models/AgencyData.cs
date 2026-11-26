using coursework.Models;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace coursework.Models
{
    public class AgencyData
    {
        public ObservableCollection<Client> Clients { get; set; } = new();

        public ObservableCollection<Property> Properties { get; set; } = new();

        public ObservableCollection<Request> Requests { get; set; } = new();

        public ObservableCollection<Deal> Deals { get; set; } = new();

        public List<OfferDecision> OfferDecisions { get; set; } = new();
    }
}
