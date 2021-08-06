using Luna.Common.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Luna.RBAC.Public.Client
{
    public class RoleAssignmentRequest
    {
        public static string example = JsonConvert.SerializeObject(new RoleAssignmentRequest()
        {
            Uid = "b46324b3-6a92-4e35-84be-fa1b2919af69",
            UserName = "FirstName LastName",
            Role = RBACRole.SystemAdmin.ToString()
        });

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            ValidationUtils.ValidateObjectId(Uid, nameof(Uid));
            ValidationUtils.ValidateStringValueLength(UserName, ValidationUtils.OBJECT_NAME_STRING_MAX_LENGTH, nameof(UserName));
            ValidationUtils.ValidateEnum(Role, typeof(RBACRole), nameof(Role));
        }

        [JsonProperty(PropertyName = "Uid", Required = Required.Always)]
        public string Uid { get; set; }

        [JsonProperty(PropertyName = "UserName", Required = Required.Always)]
        public string UserName { get; set; }

        [JsonProperty(PropertyName = "Role", Required = Required.Always)]
        public string Role { get; set; }

        [JsonProperty(PropertyName = "CreatedTime", Required = Required.Default)]
        public DateTime CreatedTime { get; set; }
    }
}
