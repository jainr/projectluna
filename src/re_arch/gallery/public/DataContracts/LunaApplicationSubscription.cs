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
            SubscriptionId = new Guid("0eb1e9dd-df4e-4b03-9e8f-6d8ac0eab782"),
            SubscriptionName = "mysub",
            PrimaryKey = "60158cd36c464531bf40a6c7efc629cd",
            SecondaryKey = "fc7cfaab23b0494aa53f178f7fbc720c",
            BaseUrl = "https://luna.azurewebapp.net/api",
            Notes = "this is my subscription",
            CreatedTime = new DateTime(637589448301320267),
            Owners = new List<LunaApplicationSubscriptionOwner>(
                new LunaApplicationSubscriptionOwner[]
                {
                    new LunaApplicationSubscriptionOwner()
                    {
                        UserName = "FirstName LastName",
                        UserId = "c4627f84-c3a8-45b3-8709-565bd2e50b97"
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
