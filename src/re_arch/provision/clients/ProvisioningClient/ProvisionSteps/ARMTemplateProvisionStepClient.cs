using Luna.Common.Utils;
using Luna.Gallery.Public.Client;
using Luna.Provision.Data;
using Luna.Publish.Public.Client;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Luna.Provision.Clients
{
    public class ARMTemplateProvisionStepClient : BaseProvisionStepClient, IAsyncProvisionStepClient
    {
        private const string DEPLOY_ARM_BASE_URL_FORMAT = "https://management.azure.com/subscriptions/{0}/resourceGroups/{1}/providers/Microsoft.Resources/deployments/{2}?api-version={3}";
        private const string DEPLOY_ARM_API_VERSION = "2019-05-01";
        private const string DEPLOYMENT_NAME_PARAM_NAME = "luna_arm_deployment_name";

        private ILogger _logger;
        private HttpClient _httpClient;

        public ARMTemplateProvisioningStepProp Properties { get; set; }

        public ARMTemplateProvisionStepClient(ARMTemplateProvisioningStepProp properties, 
            ILogger logger)
        {
            this.Properties = properties;
            this._logger = logger;
            this._httpClient = new HttpClient();
        }

        public async Task<ProvisionStepExecutionResult> CheckExecutionStatusAsync(List<MarketplaceSubscriptionParameter> parameters)
        {
            var response = await GetDeploymentAsync(parameters);
            var content = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                var result = JsonConvert.DeserializeObject<DeploymentExtendedResult>(content);
                var state = result.Properties.ProvisioningState;
                if (state.Equals(ArmProvisioningState.Succeeded))
                {
                    return ProvisionStepExecutionResult.Completed;
                }

                if (state.Equals(ArmProvisioningState.Failed) || state.Equals(ArmProvisioningState.Canceled))
                {
                    return ProvisionStepExecutionResult.Failed;
                }
            }
            else
            {
                // TODO: add retry
                throw new LunaServerException(content);
            }

            return ProvisionStepExecutionResult.Running;
        }

        public async Task<List<MarketplaceSubscriptionParameter>> FinishAsync(List<MarketplaceSubscriptionParameter> parameters)
        {
            var response = await GetDeploymentAsync(parameters);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<DeploymentExtendedResult>(content);
                if (result.Properties.Outputs != null)
                {
                    foreach (var key in result.Properties.Outputs.Keys)
                    {
                        JObject value = JObject.Parse(result.Properties.Outputs[key].ToString());
                        parameters.Add(new MarketplaceSubscriptionParameter
                        {
                            Name = key,
                            Value = value["value"].ToString(),
                            Type = MarketplaceParameterValueType.String.ToString(),
                            IsSystemParameter = true
                        });
                    }
                }
            }
            return parameters;
        }

        public async Task<List<MarketplaceSubscriptionParameter>> StartAsync(List<MarketplaceSubscriptionParameter> parameters)
        {
            string subscriptionId = GetParameterValue(parameters, Properties.AzureSubscriptionIdParameterName);
            string resourceGroup = GetParameterValue(parameters, Properties.ResourceGroupNameParameterName);
            string accessToken = GetParameterValue(parameters, Properties.AccessTokenParameterName);
            string location = GetParameterValue(parameters, Properties.AzureLocationParameterName);

            string deploymentName = Guid.NewGuid().ToString();

            parameters.Add(new MarketplaceSubscriptionParameter()
            {
                Name = DEPLOYMENT_NAME_PARAM_NAME,
                Type = MarketplaceParameterValueType.String.ToString(),
                Value = deploymentName,
                IsSystemParameter = true
            });

            var requestUrl = string.Format(DEPLOY_ARM_BASE_URL_FORMAT,
                subscriptionId,
                resourceGroup,
                deploymentName,
                DEPLOY_ARM_API_VERSION);

            var prop = new DeploymentProperties
            {
                DebugSetting = new DebugSetting() { DetailLevel = "none" },
                Mode = this.Properties.IsRunInCompleteMode ? nameof(DeploymentMode.Complete) : nameof(DeploymentMode.Incremental),
                TemplateLink = new TemplateLink { Uri = this.Properties.TemplateUrl },
                Parameters = GetInputParameters(parameters, Properties.InputParameterNames)
            };

            var body = new DeploymentRequestBody
            {
                Properties = prop
            };

            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(requestUrl),
                Method = HttpMethod.Put
            };

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            request.Content = new StringContent(JsonConvert.SerializeObject(body));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                return parameters;
            }
            else
            {
                // TODO: add retry
                var content = await response.Content.ReadAsStringAsync();
                throw new LunaServerException(content);
            }

        }

        private async Task<HttpResponseMessage> GetDeploymentAsync(List<MarketplaceSubscriptionParameter> parameters)
        {
            string subscriptionId = GetParameterValue(parameters, Properties.AzureSubscriptionIdParameterName);
            string resourceGroup = GetParameterValue(parameters, Properties.ResourceGroupNameParameterName);
            string accessToken = GetParameterValue(parameters, Properties.AccessTokenParameterName);
            string deploymentName = GetParameterValue(parameters, DEPLOYMENT_NAME_PARAM_NAME);

            parameters.Add(new MarketplaceSubscriptionParameter()
            {
                Name = DEPLOYMENT_NAME_PARAM_NAME,
                Type = MarketplaceParameterValueType.String.ToString(),
                Value = deploymentName,
                IsSystemParameter = true
            });

            var requestUrl = string.Format(DEPLOY_ARM_BASE_URL_FORMAT,
                subscriptionId,
                resourceGroup,
                deploymentName,
                DEPLOY_ARM_API_VERSION);

            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(requestUrl),
                Method = HttpMethod.Get
            };

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(request);

            return response;
        }

        private object GetInputParameters(List<MarketplaceSubscriptionParameter> parameters, List<string> inputParameterNames)
        {
            JObject inputParameters = new JObject();
            foreach (var name in inputParameterNames)
            {
                JProperty value = new JProperty("value", GetParameterValue(parameters, name));
                inputParameters.Add(name, new JObject(value));
            }

            return inputParameters;
        }

        private string GetParameterValue(List<MarketplaceSubscriptionParameter> parameters, string name)
        {
            var param = parameters.SingleOrDefault(x => x.Name == name);
            if (param != null)
            {
                return param.Value;
            }

            return string.Empty;
        }
    }
}
