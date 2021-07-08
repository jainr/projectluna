using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Provision.Data
{
    public class MarketplacePlanDB
    {
        public long Id { get; set; }

        public string OfferId { get; set; }

        public string PlanId { get; set; }

        public string Parameters { get; set; }

        public string Mode { get; set; }

        public string Properties { get; set; }

        public string ProvisioningStepsSecretName { get; set; }

        public long CreatedByEventId { get; set; }

    }
}