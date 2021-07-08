using Luna.Gallery.Public.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Luna.Gallery.Data
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

        public LunaApplicationSubscriptionEventContent ToEventContent()
        {
            return new LunaApplicationSubscriptionEventContent()
            {
                SubscriptionId = this.SubscriptionId,
                ApplicationName = this.ApplicationName,
                Status = this.Status,
                Notes = this.Notes,
                PrimaryKeySecretName = this.PrimaryKeySecretName,
                SecondaryKeySecretName = this.SecondaryKeySecretName
            };
        }

        public List<LunaApplicationSubscriptionOwnerDB> Owners { get; set; }
    }
}
