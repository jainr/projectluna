using Luna.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Luna.Data.DataContracts
{
    public class OfferLayout
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public OfferLayout(string id, string displayName)
        {
            this.Id = id;
            this.DisplayName = displayName;
        }
    }
    public class PlanLayout
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }

        public PlanLayout(string id, string displayName)
        {
            this.Id = id;
            this.DisplayName = displayName;
        }
    }

    public class OfferParameterLayout
    {
        public string DisplayName { get; set; }
        public string Id { get; set; }
        public string Type { get; set; }
        public List<string> Values { get; set; }

        public OfferParameterLayout(OfferParameter param)
        {
            this.DisplayName = param.DisplayName;
            this.Id = param.ParameterName;
            this.Type = param.ValueType;
            if (this.Type.Equals("string", StringComparison.InvariantCultureIgnoreCase) && param.FromList)
            {
                this.Type = "list";
                this.Values = param.ValueList.Split(';').ToList<string>();
            }
        }
    }

    public class SubscriptionLayout
    {
        public Guid SubscriptionId { get; set; }

        public string SubscriptionName { get; set; }

        public OfferLayout Offer { get; set; }

        public List<PlanLayout> Plans { get; set; }

        public List<string> HostTypes { get; set; }

        public List<OfferParameterLayout> Parameters { get; set; }

        public SubscriptionLayout(Guid subscriptionId, string subscriptionName,
            OfferLayout offer, List<PlanLayout> plans, List<string> hostTypes, List<OfferParameter> offerParameters=null)
        {
            this.SubscriptionId = subscriptionId;
            this.SubscriptionName = subscriptionName;
            this.Offer = offer;
            this.Plans = plans;
            this.HostTypes = hostTypes;
            this.Parameters = new List<OfferParameterLayout>();
            if (offerParameters != null)
            {
                foreach (var param in offerParameters)
                {
                    this.Parameters.Add(new OfferParameterLayout(param));
                }
            }
        }

    }
}
