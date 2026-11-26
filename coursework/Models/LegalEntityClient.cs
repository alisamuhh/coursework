namespace coursework.Models
{
    public class LegalEntityClient : Client
    {
        public string OrganizationName { get; set; } = "";

        public override string GetIdentityDescription() =>
            $"Организация: {OrganizationName}; представитель: {FullName}";

        public override string GetDisplayName() =>
            OrganizationName;
    }
}