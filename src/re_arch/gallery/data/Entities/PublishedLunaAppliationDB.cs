using Luna.Gallery.Public.Client.DataContracts;
using Luna.Publish.Public.Client.DataContract;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Gallery.Data.Entities
{
    public class PublishedLunaAppliationDB
    {

        [JsonIgnore]
        public long Id { get; set; }

        public string UniqueName { get; set; }

        public string DisplayName { get; set; }

        public string Description { get; set; }

        public string LogoImageUrl { get; set; }

        public string DocumentationUrl { get; set; }

        public string Publisher { get; set; }

        public string Details { get; set; }

        public long LastAppliedEventId { get; set; }

        public string Tags { get; set; }

        public DateTime CreatedTime { get; set; }

        public DateTime LastUpdatedTime { get; set; }

        public bool IsEnabled { get; set; }

        public PublishedLunaAppliationDB()
        {

        }

        public PublishedLunaAppliationDB(LunaApplication app, DateTime currentTime, long lastAppliedEventId)
        {
            this.UniqueName = app.Name;
            this.Description = app.Properties.Description;
            this.DisplayName = app.Properties.DisplayName;
            this.DocumentationUrl = app.Properties.DocumentationUrl;
            this.LogoImageUrl = app.Properties.LogoImageUrl;
            this.Publisher = app.Properties.Publisher;
            this.CreatedTime = currentTime;
            this.LastUpdatedTime = currentTime;
            this.IsEnabled = true;
            this.LastAppliedEventId = lastAppliedEventId;

            this.SetDetails(app);
            this.SetTags(app);
        }

        private void SetDetails(LunaApplication app)
        {
            // TODO: this is a temp solution before we introduce the lineage service
            // We will have lineage service extract all the details
            var appDetails = new LunaApplicationDetails();
            foreach(var api in app.APIs)
            {
                var apiDetails = new LunaAPIDetails()
                {
                    Name = api.Name
                };

                foreach(var version in api.Versions)
                {
                    var versionDetails = new LunaAPIVersionDetails()
                    {
                        Name = version.Name
                    };

                    if (version.Properties.GetType() == typeof(AzureMLRealtimeEndpointAPIVersionProp))
                    {
                        apiDetails.Type = "Realtime";
                        foreach (var endpoint in ((AzureMLRealtimeEndpointAPIVersionProp)version.Properties).Endpoints)
                        {
                            versionDetails.Operations.Add(endpoint.OperationName);
                        }
                    }
                    else if (version.Properties.GetType() == typeof(AzureMLPipelineEndpointAPIVersionProp))
                    {
                        apiDetails.Type = "Asynchronized";
                        foreach (var endpoint in ((AzureMLPipelineEndpointAPIVersionProp)version.Properties).Endpoints)
                        {
                            versionDetails.Operations.Add(endpoint.OperationName);
                        }
                    }

                    apiDetails.Versions.Add(versionDetails);
                }

                appDetails.APIs.Add(apiDetails);
            }

            this.Details = JsonConvert.SerializeObject(appDetails);
        }

        private void SetTags(LunaApplication app)
        {
            StringBuilder tagStr = new StringBuilder();
            foreach(var tag in app.Properties.Tags)
            {
                tagStr.Append(tag.Key);
                if (tag.Value != null)
                {
                    tagStr.Append(":");
                    tagStr.Append(tag.Value);
                }
                tagStr.Append(";");
            }
            this.Tags = tagStr.ToString();
        }

        public PublishedLunaApplication ToPublishedLunaApplication()
        {
            var app = new PublishedLunaApplication()
            {
                UniqueName = this.UniqueName,
                DisplayName = this.DisplayName,
                Description = this.Description,
                LogoImageUrl = this.LogoImageUrl,
                DocumentationUrl = this.DocumentationUrl,
                Publisher = this.Publisher,
                Details = JsonConvert.DeserializeObject<LunaApplicationDetails>(this.Details),
            };

            if (this.Tags != null)
            {
                foreach (var keyValue in this.Tags.Split(";", StringSplitOptions.RemoveEmptyEntries))
                {
                    var tag = new LunaPublishedApplicationTag();
                    // Key value pair if contains : and : is not the last character
                    if (keyValue.Contains(":") && keyValue.IndexOf(":") < keyValue.Length - 1)
                    {
                        tag.Name = keyValue.Substring(0, keyValue.IndexOf(":"));
                        tag.Value = keyValue.Substring(keyValue.IndexOf(":") + 1);
                    }
                    else
                    {
                        tag.Name = keyValue;
                        tag.Value = null;
                    }
                    app.Tags.Add(tag);
                }
            }

            return app;
        }

    }
}
