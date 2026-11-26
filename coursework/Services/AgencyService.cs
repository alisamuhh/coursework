using coursework.Helpers;
using coursework.Models;
using coursework.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace coursework.Services
{
    public class AgencyService
    {
        public AgencyData Data { get; private set; } = new();

        private readonly IAgencyDataStorage _storage;
        private readonly IPropertyMatchingService _matchingService;
        private readonly IDealProcessingService _dealProcessingService;
        private readonly IOfferDecisionService _offerDecisionService;
        private readonly IAgencySearchService _searchService;

        public AgencyService()
            : this(
                new AgencyDataStorage(),
                new PropertyMatchingService(),
                new DealProcessingService(),
                new OfferDecisionService(),
                new AgencySearchService())
        {
        }

        public AgencyService(
            IAgencyDataStorage storage,
            IPropertyMatchingService matchingService,
            IDealProcessingService dealProcessingService,
            IOfferDecisionService offerDecisionService,
            IAgencySearchService searchService)
        {
            _storage = storage ??
                throw new ArgumentNullException(nameof(storage));

            _matchingService = matchingService ??
                throw new ArgumentNullException(nameof(matchingService));

            _dealProcessingService = dealProcessingService ??
                throw new ArgumentNullException(nameof(dealProcessingService));

            _offerDecisionService = offerDecisionService ??
                throw new ArgumentNullException(nameof(offerDecisionService));

            _searchService = searchService ??
                throw new ArgumentNullException(nameof(searchService));
        }

        private void ValidateClient(Client client)
        {
            ClientValidator.Validate(client, Data);
        }

        public void AddClient(Client client)
        {
            ValidateClient(client);

            client.LastName = client.LastName.Trim();
            client.FirstName = client.FirstName.Trim();
            client.MiddleName = client.MiddleName?.Trim() ?? "";
            client.Phone = client.Phone.Trim();
            client.Address = client.Address.Trim();
            client.Email = client.Email?.Trim() ?? "";
            client.RegistrationNumber =
                client.RegistrationNumber.Trim();

            if (client is LegalEntityClient legal)
            {
                legal.OrganizationName =
                    legal.OrganizationName.Trim();
            }

            Data.Clients.Add(client);
            Save();
        }

        private void ValidateClientRoleChange(Client updated)
        {
            // Находим существующего клиента
            Client original = Data.Clients
                .FirstOrDefault(c => c.Id == updated.Id)
                ?? throw new ArgumentException("Клиент не найден.");

            // Если роль клиента не изменилась,
            // проверять ограничения не требуется
            if (string.Equals(
                original.ClientType?.Trim(),
                updated.ClientType?.Trim(),
                StringComparison.Ordinal))
            {
                return;
            }

            // Проверяем связанные с клиентом данные
            bool hasRequests = Data.Requests.Any(
                r => r.ClientId == updated.Id);

            bool hasProperties = Data.Properties.Any(
                p => p.OwnerId == updated.Id);

            bool hasBuyerDeals = Data.Deals.Any(
                d => d.ClientId == updated.Id);

            bool hasSellerDeals = Data.Deals.Any(
                d => d.SellerId == updated.Id);

            // Проверяем права новой роли
            bool canBuy = ClientRolePolicy.CanBuyOrRent(updated.ClientType);
            bool canSell = ClientRolePolicy.CanSellOrRentOut(updated.ClientType);

            // Нельзя убрать право покупки при наличии заявок или сделок
            if (!canBuy && (hasRequests || hasBuyerDeals))
            {
                throw new InvalidOperationException(
                    "Нельзя изменить тип клиента: у него уже " +
                    "есть заявки или сделки в роли покупателя.");
            }

            // Нельзя убрать право продажи при наличии недвижимости или сделок
            if (!canSell && (hasProperties || hasSellerDeals))
            {
                throw new InvalidOperationException(
                    "Нельзя изменить тип клиента: у него уже " +
                    "есть недвижимость или сделки в роли продавца.");
            }
        }

        public void UpdateClient(Client updated)
        {
            Client original = Data.Clients
                .FirstOrDefault(c => c.Id == updated.Id)
                ?? throw new ArgumentException("Клиент не найден.");

            ValidateClient(updated);
            ValidateClientRoleChange(updated);

            // Сохраняем первоначальную дату регистрации
            updated.RegistrationDate = original.RegistrationDate;

            updated.LastName = updated.LastName.Trim();
            updated.FirstName = updated.FirstName.Trim();
            updated.MiddleName = updated.MiddleName?.Trim() ?? "";
            updated.Phone = updated.Phone.Trim();
            updated.Email = updated.Email?.Trim() ?? "";
            updated.Address = updated.Address.Trim();
            updated.RegistrationNumber =
                updated.RegistrationNumber.Trim();

            if (updated is LegalEntityClient legal)
            {
                legal.OrganizationName =
                    legal.OrganizationName.Trim();
            }

            // Замена объекта позволяет менять тип лица
            // без потери идентификатора и связей с другими данными
            int index = Data.Clients.IndexOf(original);
            Data.Clients[index] = updated;

            Save();
        }

        public void DeleteClient(Guid clientId)
        {
            bool hasProperties = Data.Properties.Any(
                p => p.OwnerId == clientId);

            bool hasRequests = Data.Requests.Any(
                r => r.ClientId == clientId);


            bool hasDeals = Data.Deals.Any(d =>
                d.ClientId == clientId ||
                d.SellerId == clientId);


            if (hasProperties || hasRequests || hasDeals)
            {
                throw new InvalidOperationException(
                    "Нельзя удалить клиента, связанного с " +
                    "недвижимостью, заявками или сделками.");
            }

            Client client = Data.Clients.FirstOrDefault(
                c => c.Id == clientId)
                ?? throw new ArgumentException("Клиент не найден.");


            DateTime earliestDeletionDate =
                client.RegistrationDate.Date.AddYears(1);

            if (DateTime.Today < earliestDeletionDate)
            {
                throw new InvalidOperationException(
                    "Сведения о клиенте должны храниться не менее одного года. " +
                    $"Удаление возможно не ранее {earliestDeletionDate:dd.MM.yyyy}.");
            }


            Data.Clients.Remove(client);
            Save();
        }

        public List<Client> SearchClients(string searchText)
        {
            return _searchService.SearchClients(Data, searchText);
        }

        private void ValidateProperty(Property property)
        {
            PropertyValidator.Validate(property, Data);
        }

        private void ValidatePropertyOwnerRole(Property property)
        {
            Client owner = Data.Clients.FirstOrDefault(
                c => c.Id == property.OwnerId)
                ?? throw new ArgumentException("Владелец не найден.");

            if (!ClientRolePolicy.CanSellOrRentOut(owner.ClientType))
            {
                throw new InvalidOperationException(
                    "Этот клиент зарегистрирован как покупатель / арендатор. " +
                    "Для размещения недвижимости измените тип клиента " +
                    "на «Покупатель и продавец».");
            }
        }


        public void AddProperty(Property property)
        {
            // Проверяем все характеристики недвижимости
            ValidateProperty(property);

            // Убираем лишние пробелы
            property.Category = property.Category.Trim();
            property.OperationType = property.OperationType.Trim();
            property.District = property.District.Trim();
            property.Address = property.Address.Trim();
            property.BuildingMaterial = property.BuildingMaterial.Trim();
            property.Conditions = property.Conditions?.Trim() ?? "";

            ValidatePropertyOwnerRole(property);
            Data.Properties.Add(property);

            Save();
        }


        public void UpdateProperty(Property updated)
        {

            Property property = Data.Properties.FirstOrDefault(
                p => p.Id == updated.Id)
                ?? throw new ArgumentException(
                    "Объект недвижимости не найден.");

            if (Data.Deals.Any(d => d.PropertyId == property.Id) &&
                updated.OwnerId != property.OwnerId)
            {
                throw new InvalidOperationException(
                    "Нельзя вручную менять владельца объекта с историей сделок.");
            }

            if (!property.IsAvailable &&
                Data.Deals.Any(d => d.PropertyId == property.Id) &&
                updated.OperationType != property.OperationType)
            {
                throw new InvalidOperationException(
                    "Сначала повторно выставьте объект, затем меняйте тип операции.");
            }

            updated.IsAvailable = property.IsAvailable;

            ValidateProperty(updated);

            updated.Category = updated.Category.Trim();
            updated.OperationType = updated.OperationType.Trim();
            updated.District = updated.District.Trim();
            updated.Address = updated.Address.Trim();
            updated.BuildingMaterial = updated.BuildingMaterial.Trim();
            updated.Conditions = updated.Conditions?.Trim() ?? "";

            // Заменяем объект целиком: при смене категории меняется и производный тип.
            ValidatePropertyOwnerRole(updated);
            int index = Data.Properties.IndexOf(property);
            Data.Properties[index] = updated;

            Save();
        }


        public void ReListProperty(Guid propertyId)
        {
            Property property = Data.Properties
                .FirstOrDefault(p => p.Id == propertyId)
                ?? throw new ArgumentException(
                    "Объект недвижимости не найден.");

            if (property.IsAvailable)
                throw new InvalidOperationException(
                    "Объект уже доступен для подбора.");

            ValidatePropertyOwnerRole(property);

            Data.OfferDecisions ??= new List<OfferDecision>();

            Data.OfferDecisions.RemoveAll(
                d => d.PropertyId == propertyId);

            property.IsAvailable = true;

            Save();
        }


        public void DeleteProperty(Guid propertyId)
        {
            Property property = Data.Properties
                .FirstOrDefault(p => p.Id == propertyId)
                ?? throw new ArgumentException(
                    "Объект недвижимости не найден.");

            if (Data.Deals.Any(d => d.PropertyId == propertyId))
                throw new InvalidOperationException(
                    "Нельзя удалить объект, связанный со сделкой.");

            Data.OfferDecisions ??= new List<OfferDecision>();

            Data.OfferDecisions.RemoveAll(
                d => d.PropertyId == propertyId);

            Data.Properties.Remove(property);

            Save();
        }

        public List<Property> SearchProperties(
            string searchText,
            string category,
            string operation)
        {
            return _searchService.SearchProperties(
                Data,
                searchText,
                category,
                operation);
        }

        private void ValidateRequest(Request request)
        {
            RequestValidator.Validate(request, Data);
        }

        private void ValidateRequestClientRole(Request request)
        {
            Client client = Data.Clients.FirstOrDefault(
                c => c.Id == request.ClientId)
                ?? throw new ArgumentException("Клиент не найден.");

            if (!ClientRolePolicy.CanBuyOrRent(client.ClientType))
            {
                throw new InvalidOperationException(
                    "Этот клиент зарегистрирован как продавец / арендодатель. " +
                    "Для оформления заявки на покупку или аренду " +
                    "измените тип клиента на «Покупатель и продавец».");
            }
        }

        public void AddRequest(Request request)
        {
            ValidateRequest(request);
            ValidateRequestClientRole(request);

            request.OperationType = request.OperationType.Trim();
            request.Category = request.Category.Trim();
            request.District = request.District?.Trim() ?? "";
            request.Conditions = request.Conditions?.Trim() ?? "";

            Data.Requests.Add(request);
            Save();
        }

        public void UpdateRequest(Request updated)
        {
            if (updated == null)
                throw new ArgumentException("Данные заявки отсутствуют.");

            Request original = Data.Requests.FirstOrDefault(
                r => r.Id == updated.Id)
                ?? throw new ArgumentException("Заявка не найдена.");

            // Проверяем все поля заявки перед сохранением изменений
            ValidateRequest(updated);
            ValidateRequestClientRole(updated);

            updated.OperationType = updated.OperationType.Trim();
            updated.Category = updated.Category.Trim();
            updated.District = updated.District?.Trim() ?? "";
            updated.Conditions = updated.Conditions?.Trim() ?? "";

            original.ClientId = updated.ClientId;
            original.OperationType = updated.OperationType;
            original.Category = updated.Category;
            original.District = updated.District;
            original.MinimumRooms = updated.MinimumRooms;
            original.MinimumArea = updated.MinimumArea;
            original.MaximumPrice = updated.MaximumPrice;

            original.PreferredFloor = updated.PreferredFloor;
            original.MinimumLivingArea = updated.MinimumLivingArea;
            original.MinimumKitchenArea = updated.MinimumKitchenArea;

            original.RequiresBalcony = updated.RequiresBalcony;
            original.RequiresLoggia = updated.RequiresLoggia;

            original.PreferredBuildingFloors =
                updated.PreferredBuildingFloors;

            original.PreferredBuildingMaterial =
                updated.PreferredBuildingMaterial;

            original.MinimumConstructionYear =
                updated.MinimumConstructionYear;

            original.MinimumRepairYear =
                updated.MinimumRepairYear;

            original.RequiresElevator = updated.RequiresElevator;
            original.RequiresGarbageChute = updated.RequiresGarbageChute;
            original.RequiresTelephoneCable =
                updated.RequiresTelephoneCable;

            original.Conditions = updated.Conditions;
            original.IsSpecial = updated.IsSpecial;

            Save();

        }

        public void DeleteRequest(Guid requestId)
        {
            Request request = Data.Requests
                .FirstOrDefault(r => r.Id == requestId)
                ?? throw new ArgumentException("Заявка не найдена.");

            // Удаляем связанные решения по предложениям
            Data.OfferDecisions ??= new List<OfferDecision>();

            Data.OfferDecisions.RemoveAll(
                d => d.RequestId == requestId);

            Data.Requests.Remove(request);

            Save();
        }

        public void RegisterOfferDecision(
            Guid requestId,
            Guid propertyId,
            bool accepted,
            string comment = "")
        {
            _offerDecisionService.Register(
                Data,
                requestId,
                propertyId,
                accepted,
                comment,
                _matchingService.IsMatch);

            Save();
        }
        private bool IsPropertyMatchingRequest(Property property, Request request)
        {
            return _matchingService.IsMatch(property, request);
        }


        public List<Property> FindProperties(Request request)
        {
            if (request == null)
                throw new ArgumentException("Заявка не указана.");

            Data.OfferDecisions ??= new List<OfferDecision>();

            return Data.Properties
                .Where(p => IsPropertyMatchingRequest(p, request))
                .Where(p => !Data.OfferDecisions.Any(d =>
                    d.RequestId == request.Id &&
                    d.PropertyId == p.Id &&
                    d.Status == "Отклонено"))
                .OrderBy(p => p.Price)
                .ToList();
        }

        public List<Request> FindRequestsForProperty(Property property)
        {
            if (property == null)
                throw new ArgumentException("Объект недвижимости не указан.");

            Data.OfferDecisions ??= new List<OfferDecision>();

            return Data.Requests
                .Where(r => IsPropertyMatchingRequest(property, r))
                .Where(r => !Data.OfferDecisions.Any(d =>
                    d.RequestId == r.Id &&
                    d.PropertyId == property.Id &&
                    d.Status == "Отклонено"))
                .OrderByDescending(r => r.IsSpecial)
                .ThenBy(r => r.CreatedAt)
                .ToList();
        }


        public void AddDeal(Deal deal)
        {
            // Проверяем возможность заключения сделки.
            Property property = DealValidator.Validate(
                deal,
                Data,
                _matchingService.IsMatch);

            // Регистрируем сделку и обновляем недвижимость.
            _dealProcessingService.CompleteDeal(deal, property, Data);

            // Сохраняем изменения в JSON.
            Save();
        }


        // Перевод старых записей Property в реальные производные классы.
        private void NormalizePropertyTypes()
        {
            for (int i = 0; i < Data.Properties.Count; i++)
                Data.Properties[i] = PropertyFactory.Convert(Data.Properties[i]);
        }

        public void Save()
        {
            NormalizePropertyTypes();
            _storage.Save(Data);
        }


        private bool RestoreOwnersAfterPastSales()
        {
            bool changed = false;

            foreach (Property property in Data.Properties)
            {
                if (property.IsAvailable ||
                    property.OperationType != "Продажа")
                    continue;

                Deal? lastSale = Data.Deals
                    .Where(d => d.PropertyId == property.Id)
                    .OrderByDescending(d => d.DealDate)
                    .FirstOrDefault();

                if (lastSale != null &&
                    Data.Clients.Any(c => c.Id == lastSale.ClientId) &&
                    property.OwnerId != lastSale.ClientId)
                {
                    property.OwnerId = lastSale.ClientId;
                    changed = true;
                }
            }

            return changed;
        }

        public void Load()
        {
            if (_storage.Exists())
            {

                string json = _storage.Read();
                Data = _storage.Deserialize(json);


                NormalizePropertyTypes();

                bool fixedOwners = RestoreOwnersAfterPastSales();
                bool updatedDemo = DemoDataService.UpdateDemoClients(Data);

                if (fixedOwners || updatedDemo)
                    Save();

                return;
            }

            // Первоначальное заполнение
            Data = DemoDataService.Create();

            NormalizePropertyTypes();

            Save();
        }


    }
}