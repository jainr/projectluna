using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Gallery.Public.Client.DataContracts
{
    public class LunaApplicationSubscription
    {
        public static string example = JsonConvert.SerializeObject(new LunaApplicationSubscription()
        {
            SubscriptionId = Guid.NewGuid(),
            SubscriptionName = "mysub",
            PrimaryKey = Guid.NewGuid().ToString("N"),
            SecondaryKey = Guid.NewGuid().ToString("N"),
            BaseUrl = "https://luna.azurewebapp.net/api",
            Notes = "this is my subscription",
            CreatedTime = DateTime.UtcNow,
            Owners = new List<LunaApplicationSubscriptionOwner>(
                new LunaApplicationSubscriptionOwner[]
                {
                    new LunaApplicationSubscriptionOwner()
                    {
                        UserName = "FirstName LastName",
                        UserId = Guid.NewGuid().ToString()
                    }
                })
        });

        public LunaApplicationSubscription()
        {
            this.Owners = new List<LunaApplicationSubscriptionOwner>();
        }

        [JsonProperty(PropertyName = "SubscriptionId", Required = Required.Always)]
        public Guid SubscriptionId { get; set; }

        [JsonProperty(PropertyName = "SubscriptionName", Required = Required.Always)]
        public string SubscriptionName { get; set; }

        [JsonProperty(PropertyName = "PrimaryKey", Required = Required.Default)]
        public string PrimaryKey { get; set; }

        [JsonProperty(PropertyName = "SecondaryKey", Required = Required.Default)]
        public string SecondaryKey { get; set; }

        [JsonProperty(PropertyName = "BaseUrl", Required = Required.Default)]
        public string BaseUrl { get; set; }

        [JsonProperty(PropertyName = "Notes", Required = Required.Default)]
        public string Notes { get; set; }

        [JsonProperty(PropertyName = "Owners", Required = Required.Default)]
        public List<LunaApplicationSubscriptionOwner> Owners { get; set; }

        [JsonProperty(PropertyName = "CreatedTime", Required = Required.Default)]
        public DateTime CreatedTime { get; set; }
    }
}
