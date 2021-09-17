using Luna.Common.Utils;
using Luna.Marketplace.Public.Client;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Luna.Provision.Clients.ProvisioningClient.ProvisionSteps
{
    public class WebhookProvisionStepClient : BaseProvisionStepClient, ISyncProvisionStepClient
    {
        private readonly ILogger _logger;
        private readonly HttpClient _httpClient;

        public WebhookProvisioningStepProp Properties { get; set; }

        public WebhookProvisionStepClient(WebhookProvisioningStepProp properties, ILogger logger)
        {
            this.Properties = properties;
            this._httpClient = new HttpClient();
            _logger = logger;
        }

        public async Task<List<MarketplaceSubscriptionParameter>> RunAsync(List<MarketplaceSubscriptionParameter> parameters)
        {
            var requestUri = new Uri(Properties.WebhookUrl);
            var request = new HttpRequestMessage { RequestUri = requestUri, Method = HttpMethod.Post };

            if (this.Properties.WebhookAuthType.Equals(WebhookAuthType.ApiKey.ToString()))
            {
                request.Headers.Add(this.Properties.WebhookAuthKey, this.Properties.WebhookAuthValue);
            }
            else if (this.Properties.WebhookAuthType.Equals(WebhookAuthType.BearerToken.ToString()))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", this.Properties.WebhookAuthValue);
            }
            else if (this.Properties.WebhookAuthType.Equals(WebhookAuthType.QueryParameter.ToString()))
            {
                request.RequestUri = new Uri(string.Format("{0}?{1}={2}", this.Properties.WebhookUrl, this.Properties.WebhookAuthKey, this.Properties.WebhookAuthValue));
            }

            Dictionary<string, object> requestBody = new Dictionary<string, object>();

            foreach (var param in parameters)
            {
                if (this.Properties.InputParameterNames.Contains(param.Name))
                {
                    requestBody.Add(param.Name, param.Value);
                }
            }

            request.Content = new StringContent(JsonConvert.SerializeObject(requestBody));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var response = await this._httpClient.SendAsync(request);

            var content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var output = JsonConvert.DeserializeObject<Dictionary<string, object>>(content);
                foreach (var param in output.Keys)
                {
                    if (this.Properties.OutputParameterNames.Contains(param))
                    {
                        parameters.Add(new MarketplaceSubscriptionParameter
                        {
                            Name = param,
                            Value = output[param].ToString(),
                            Type = MarketplaceParameterValueType.String.ToString(),
                            IsSystemParameter = false,
                        });
                    }
                }
                return parameters;
            }
            else
            {
                throw new LunaServerException(content);
            }

        }
    }
}
