using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Luna.Data.Entities
{
    /// <summary>
    /// Entity class that maps to the offers table in the database.
    /// </summary>
    public partial class Offer
    {
        /// <summary>
        /// Constructs the EF Core collection navigation properties.
        /// </summary>
        public Offer()
        {
            this.AadSecretTmps = new HashSet<AadSecretTmp>();
            this.ArmTemplateParameters = new HashSet<ArmTemplateParameter>();
            this.ArmTemplates = new HashSet<ArmTemplate>();
            this.IpConfigs = new HashSet<IpConfig>();
            this.OfferParameters = new HashSet<OfferParameter>();
            this.Plans = new HashSet<Plan>();
            this.Subscriptions = new HashSet<Subscription>();
            this.ManualActivation = false;
            this.ManualCompleteOperation = false;
            this.IsAzureMarketplaceOffer = true;
            this.IsInternalApplication = true;
        }

        /// <summary>
        /// Copies all non-EF Core values.
        /// </summary>
        /// <param name="offer">The object to be copied.</param>
        public void Copy(Offer offer)
        {
            this.OfferVersion = offer.OfferVersion;
            this.DisplayName = offer.DisplayName;
            this.Owners = offer.Owners;
            this.HostSubscription = offer.HostSubscription;
            this.IsInternalApplication = this.IsInternalApplication;
            this.IsAzureMarketplaceOffer = offer.IsAzureMarketplaceOffer;
            this.LogoImageUrl = offer.LogoImageUrl;
            this.DocumentationUrl = offer.DocumentationUrl;
            this.Description = offer.Description;
            this.Tags = offer.Tags;
        }

        public string GetTagByKey(string key)
        {
            if (IsTagKeyExist(key))
            {
                var result = this.Tags.Substring(Tags.IndexOf(key) + key.Length + 1);
                result = result.Contains(";") ? result.Substring(0, result.IndexOf(";")) : result;
                return result;
            }

            return null;
        }

        public bool IsTagKeyExist(string key)
        {
            // case sensitive
            if (this.Tags.StartsWith(key + "=") || this.Tags.Contains(";" + key + "="))
            {
                return true;
            }
            return false;
        }

        [JsonIgnore]
        public long Id { get; set; }
        
        public string OfferName { get; set; }
        
        public string DisplayName { get; set; }
        
        public string OfferVersion { get; set; }
        
        public string Owners { get; set; }
        
        public Guid HostSubscription { get; set; }

        public string Description { get; set; }
        public string LogoImageUrl { get; set; }

        public string DocumentationUrl { get; set; }

        public string Tags { get; set; }

        public string Status { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime LastUpdatedTime { get; set; }
        [JsonIgnore]
        public DateTime? DeletedTime { get; set; }
        [JsonIgnore]
        public Guid ContainerName { get; set; }

        public bool ManualActivation { get; set; }

        public bool ManualCompleteOperation { get; set; }

        public bool IsAzureMarketplaceOffer { get; set; }
        public bool IsInternalApplication { get; set; }

        [JsonIgnore]
        public virtual ICollection<AadSecretTmp> AadSecretTmps { get; set; }
        [JsonIgnore]
        public virtual ICollection<ArmTemplateParameter> ArmTemplateParameters { get; set; }
        [JsonIgnore]
        public virtual ICollection<ArmTemplate> ArmTemplates { get; set; }
        [JsonIgnore]
        public virtual ICollection<IpConfig> IpConfigs { get; set; }
        [JsonIgnore]
        public virtual ICollection<OfferParameter> OfferParameters { get; set; }
        [JsonIgnore]
        public virtual ICollection<Plan> Plans { get; set; }
        [JsonIgnore]
        public virtual ICollection<Subscription> Subscriptions { get; set; }
    }
}