using Luna.Gallery.Public.Client.DataContracts;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Luna.Gallery.Data.Entities
{
    public class LunaApplicationSubscriptionDB
    {
        [Key]
        public Guid SubscriptionId { get; set; }

        public string SubscriptionName { get; set; }

        public string ApplicationName { get; set; }

        public string Status { get; set; }

        public string Notes { get; set; }

        public string PrimaryKeySecretName { get; set; }

        public string SecondaryKeySecretName { get; set; }

        public DateTime CreatedTime { get; set; }

        public DateTime LastUpdatedTime { get; set; }

        public DateTime UnsubscribedTime { get; set; }

        public LunaApplicationSubscription ToLunaApplicationSubscription()
        {
            var sub = new LunaApplicationSubscription()
            {
                SubscriptionId = this.SubscriptionId,
                SubscriptionName = this.SubscriptionName,
                Notes = this.Notes,
            };

            foreach (var owner in this.Owners)
            {
                sub.Owners.Add(new LunaApplicationSubscriptionOwner()
                {
                    UserId = owner.UserId,
                    UserName = owner.UserName
                });
            }

            return sub;
        }

        public List<LunaApplicationSubscriptionOwnerDB> Owners { get; set; }
    }
}
