namespace coursework.Models
{
    public class ApartmentProperty : Property
    {
        public ApartmentProperty()
        {
            Category = "Квартира";
        }

        public override string GetDescription() =>
            $"Квартира: {Rooms} комн., этаж {Floor}/{BuildingFloors}, {TotalArea:0.##} м²";
    }
}