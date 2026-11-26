namespace coursework.Models
{
    public class IndividualClient : Client
    {
        public override string GetIdentityDescription() =>
            $"Физическое лицо: {FullName}";
    }
}