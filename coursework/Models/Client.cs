using System;
using System.Text.Json.Serialization;

namespace coursework.Models
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "$clientType")]
    [JsonDerivedType(typeof(IndividualClient), "individual")]
    [JsonDerivedType(typeof(LegalEntityClient), "legal")]
    public class Client
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string LastName { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string MiddleName { get; set; } = "";

        public string Phone { get; set; } = "";
        public string Email { get; set; } = "";
        public string Address { get; set; } = "";

        public string ClientType { get; set; } = "";

        public string RegistrationNumber { get; set; } = "";
        public DateTime RegistrationDate { get; set; } = DateTime.Today;

        // Сумма регистрационного взноса
        public decimal RegistrationFee { get; set; } = 0m;

        // Дата фактической оплаты взноса
        public DateTime? RegistrationFeePaidDate { get; set; }

        // Был ли регистрационный взнос оплачен
        public bool IsRegistrationFeePaid { get; set; } = false;

        public string FullName =>
            $"{LastName} {FirstName} {MiddleName}".Trim();

        public Client() { }

        public Client(string lastName, string address,
                      string phone, string registrationNumber)
        {
            LastName = lastName;
            Address = address;
            Phone = phone;
            RegistrationNumber = registrationNumber;
        }

        [JsonIgnore]
        public string DisplayIdentity => GetIdentityDescription();


        [JsonIgnore]
        public string DisplayName => GetDisplayName();

        public virtual string GetDisplayName() => FullName;

        public virtual string GetIdentityDescription() => FullName;

        public override string ToString()
        {
            return DisplayName;
        }
    }
}
