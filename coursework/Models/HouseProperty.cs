namespace coursework.Models
{
    public class HouseProperty : Property
    {
        public HouseProperty()
        {
            Category = "Дом";
        }

        public override string GetDescription() =>
            $"Дом: {Rooms} комн., {BuildingFloors} эт., {TotalArea:0.##} м²";
    }
}