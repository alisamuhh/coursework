using coursework.Models;

namespace coursework.Interfaces
{
    public interface IDealProcessingService
    {
        void CompleteDeal(
            Deal deal,
            Property property,
            AgencyData data);
    }
}
