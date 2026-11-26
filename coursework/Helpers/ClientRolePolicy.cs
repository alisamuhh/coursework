namespace coursework.Helpers
{
    public static class ClientRolePolicy
    {
        // Может ли клиент быть покупателем или арендатором?
        public static bool CanBuyOrRent(string clientType)
        {
            return clientType == "Покупатель" ||
                   clientType == "Арендатор" ||
                   clientType == "Покупатель / арендатор" ||
                   clientType == "Покупатель и продавец";
        }

        // Может ли клиент быть продавцом или арендодателем?
        public static bool CanSellOrRentOut(string clientType)
        {
            return clientType == "Продавец" ||
                   clientType == "Арендодатель" ||
                   clientType == "Продавец / арендодатель" ||
                   clientType == "Покупатель и продавец";
        }
    }
}
