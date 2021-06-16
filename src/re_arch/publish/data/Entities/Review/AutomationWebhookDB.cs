using Luna.Publish.Public.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Publish.Data
{
    public class AutomationWebhookDB
    {
        public AutomationWebhookDB()
        {

        }

        public AutomationWebhookDB(AutomationWebhook webhook)
        {
            this.Name = webhook.Name;
            this.Description = webhook.Description;
            this.WebhookUrl = webhook.WebhookUrl;
            this.IsEnabled = webhook.IsEnabled;
            this.CreatedTime = DateTime.UtcNow;
            this.LastUpdatedTime = this.CreatedTime;
        }

        public void Update(AutomationWebhook webhook)
        {
            this.Description = webhook.Description;
            this.WebhookUrl = webhook.WebhookUrl;
            this.IsEnabled = webhook.IsEnabled;
            this.LastUpdatedTime = DateTime.UtcNow;
        }

        public AutomationWebhook ToAutomationWebhook()
        {
            return new AutomationWebhook()
            {
                Name = this.Name,
                Description = this.Description,
                WebhookUrl = this.WebhookUrl,
                IsEnabled = this.IsEnabled,
                CreatedTime = this.CreatedTime,
                LastUpdatedTime = this.LastUpdatedTime
            };
        }

        public long Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string WebhookUrl { get; set; }

        public bool IsEnabled { get; set; }

        public DateTime CreatedTime { get; set; }

        public DateTime LastUpdatedTime { get; set; }
    }
}
