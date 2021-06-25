using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Provision.Data
{
    public class MarketplaceSubProvisionJobDB
    {
        public MarketplaceSubProvisionJobDB()
        {

        }

        public long Id { get; set; }

        public Guid SubscriptionId { get; set; }

        public string OfferId { get; set; }

        public string PlanId { get; set; }

        public long PlanCreatedByEventId { get; set; }

        public string Status { get; set; }

        public string Mode { get; set; }

        public string EventType { get; set; }

        public int ProvisioningStepIndex { get; set; }

        public bool IsSynchronizedStep { get; set; }

        public string ProvisioningStepStatus { get; set; }

        public string ParametersSecretName { get; set; }

        public string ProvisionStepsSecretName { get; set; }

        public bool IsActive { get; set; }

        public int RetryCount { get; set; }

        public long CreatedByEventId { get; set; }

        public string LastErrorMessage { get; set; }

        public DateTime CreatedTime { get; set; }

        public DateTime LastUpdatedTime { get; set; }

        public DateTime? CompletedTime { get; set; }

    }
}
