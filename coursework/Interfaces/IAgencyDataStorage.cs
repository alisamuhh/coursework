using coursework.Models;

namespace coursework.Interfaces
{
    public interface IAgencyDataStorage
    {
        bool Exists();

        string Read();

        AgencyData Deserialize(string json);

        void Save(AgencyData data);
    }
}
