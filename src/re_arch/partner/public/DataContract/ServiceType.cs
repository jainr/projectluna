using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Partner.PublicClient.DataContract
{
    public class ServiceType
    {
        public static string example = JsonConvert.SerializeObject(
            new ServiceType(PartnerServiceType.AzureML.ToString(), "Azure Machine Learning workspace"));

        public ServiceType(string id, string displayName)
        {
            this.Id = id;
            this.DisplayName = displayName;
        }

        [JsonProperty(PropertyName = "Id", Required = Required.Always)]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "DisplayName", Required = Required.Always)]
        public string DisplayName { get; set; }
    }
}
