using Luna.Gallery.Public.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Gallery.Data
{
    public class ApplicationPublisherDB
    {
        public ApplicationPublisherDB()
        {

        }

        public ApplicationPublisherDB(ApplicationPublisher publisher)
        {
            this.Name = publisher.Name;
            this.Type = publisher.Type;
            this.DisplayName = publisher.DisplayName;
            this.Description = publisher.Description;
            this.EndpointUrl = publisher.EndpointUrl;
            this.WebsiteUrl = publisher.WebsiteUrl;
            this.IsEnabled = publisher.IsEnabled;
            this.CreatedTime = DateTime.UtcNow;
            this.LastUpdatedTime = this.CreatedTime;
        }

        public void Update(ApplicationPublisher publisher)
        {
            this.DisplayName = publisher.DisplayName;
            this.Description = publisher.Description;
            this.EndpointUrl = publisher.EndpointUrl;
            this.WebsiteUrl = publisher.WebsiteUrl;
            this.IsEnabled = publisher.IsEnabled;
            this.LastUpdatedTime = DateTime.UtcNow;
        }

        public ApplicationPublisher ToApplicationPublisher(string publisherKey)
        {
            return new ApplicationPublisher()
            {
                Name = this.Name,
                DisplayName = this.DisplayName,
                Type = this.Type,
                Description = this.Description,
                EndpointUrl = this.EndpointUrl,
                WebsiteUrl = this.WebsiteUrl,
                IsEnabled = this.IsEnabled,
                PublisherKey = publisherKey,
                CreatedTime = this.CreatedTime,
                LastUpdatedTime = this.LastUpdatedTime
            };
        }

        public long Id { get; set; }

        public string Name { get; set; }

        public string Type { get; set; }

        public string DisplayName { get; set; }

        public string Description { get; set; }

        public string EndpointUrl { get; set; }

        public string WebsiteUrl { get; set; }

        public bool IsEnabled { get; set; }

        public string PublisherKeySecretName { get; set; }

        public DateTime CreatedTime { get; set; }

        public DateTime LastUpdatedTime { get; set; }
    }
}
