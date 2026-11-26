using coursework.Models;

namespace coursework.Interfaces
{
    public interface IPropertyMatchingService
    {
        bool IsMatch(Property property, Request request);
    }
}
