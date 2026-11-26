namespace coursework.Models
{
    public class RoomProperty : Property
    {
        public RoomProperty()
        {
            Category = "Комната";
        }

        public override string GetDescription() =>
            $"Комната: этаж {Floor}/{BuildingFloors}, {TotalArea:0.##} м²";
    }
}