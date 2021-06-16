using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Luna.Gallery.Data
{
    public class LunaApplicationSubscriptionOwnerDB
    {

        [JsonIgnore]
        [Key]
        public long Id { get; set; }

        public Guid SubscriptionId { get; set; }

        public string UserId { get; set; }

        public string UserName { get; set; }

        public DateTime CreatedTime { get; set; }

        [JsonIgnore]
        public LunaApplicationSubscriptionDB Subscription { get; set; }

    }
}
