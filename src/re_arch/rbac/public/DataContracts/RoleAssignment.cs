using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.RBAC.Public.Client.DataContracts
{
    public class RoleAssignment
    {
        public static string example = JsonConvert.SerializeObject(new RoleAssignment()
        {
            Uid = Guid.NewGuid().ToString(),
            UserName = "FirstName LastName",
            Role = "Admin"
        });

        [JsonProperty(PropertyName ="Uid", Required = Required.Always)]
        public string Uid { get; set; }

        [JsonProperty(PropertyName = "UserName", Required = Required.Always)]
        public string UserName { get; set; }

        [JsonProperty(PropertyName = "Role", Required = Required.Always)]
        public string Role { get; set; }

        [JsonProperty(PropertyName = "CreatedTime", Required = Required.Default)]
        public DateTime CreatedTime { get; set; }
    }
}
