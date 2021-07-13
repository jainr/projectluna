using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Gallery.Public.Client
{
    public class MarketplaceSubscriptionParameter
    {
        public string Name { get; set; }

        public string Type { get; set; }

        public string Value { get; set; }

        public bool IsSystemParameter { get; set; }

        public MarketplaceSubscriptionParameter Copy(string name)
        {
            return new MarketplaceSubscriptionParameter
            {
                Name = name,
                Type = this.Type,
                Value = this.Value,
                IsSystemParameter = this.IsSystemParameter
            };
        }
    }
}
