using System;
using coursework.Models;

namespace coursework.Interfaces
{
    public interface IOfferDecisionService
    {
        void Register(
            AgencyData data,
            Guid requestId,
            Guid propertyId,
            bool accepted,
            string comment,
            Func<Property, Request, bool> isMatch);
    }
}
