using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Luna.Routing.Data.Entities
{
    public class SubscriptionsDBView
    {
        [Key]
        public Guid SubscriptionId { get; set; }

        public string PrimaryKeySecretName { get; set; }

        public string SecondaryKeySecretName { get; set; }

    }
}
