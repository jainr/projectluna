using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.PubSub.Public.Client
{

    public class LunaEventStoreType
    {
        public static bool IsValidEventStoreType(string name)
        {
            return name.Equals(APPLICATION_EVENT_STORE, StringComparison.InvariantCulture) ||
               name.Equals(SUBSCRIPTION_EVENT_STORE, StringComparison.InvariantCulture) ||
               name.Equals(AZURE_MARKETPLACE_EVENT_STORE, StringComparison.InvariantCulture);
        }

        public const string APPLICATION_EVENT_STORE = "ApplicationEvents";
        public const string SUBSCRIPTION_EVENT_STORE = "SubscriptionEvents";
        public const string AZURE_MARKETPLACE_EVENT_STORE = "AzureMarketplaceEvents";
    }

    public class LunaEventType
    {
        public const string PUBLISH_APPLICATION_EVENT = "PUBLISH_APPLICATION_EVENT";
        public const string DELETE_APPLICATION_EVENT = "DELETE_APPLICATION_EVENT";
        public const string REGENERATE_APPLICATION_MASTER_KEY = "REGENERATE_APPLICATION_MASTER_KEY";

        public const string CREATE_SUBSCRIPTION_EVENT = "CREATE_SUBSCRIPTION";
        public const string DELETE_SUBSCRIPTION_EVENT = "DELETE_SUBSCRIPTION";
        public const string REGENERATE_SUBSCRIPTION_KEY_EVENT = "REGENERATE_SUBSCRIPTION_KEY";

        public const string PUBLISH_AZURE_MARKETPLACE_OFFER = "PUBLISH_AZURE_MARKETPLACE_OFFER";
        public const string DELETE_AZURE_MARKETPLACE_OFFER = "DELETE_AZURE_MARKETPLACE_OFFER";

        public const string CREATE_AZURE_MARKETPLACE_SUBSCRIPTION = "CREATE_AZURE_MARKETPLACE_SUBSCRIPTION";
        public const string ACTIVATE_AZURE_MARKETPLACE_SUBSCRIPTION = "ACTIVATE_AZURE_MARKETPLACE_SUBSCRIPTION";
        public const string DELETE_AZURE_MARKETPLACE_SUBSCRIPTION = "DELETE_AZURE_MARKETPLACE_SUBSCRIPTION";
    }
}
