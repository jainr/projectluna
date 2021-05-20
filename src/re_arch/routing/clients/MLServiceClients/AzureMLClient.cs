using Luna.Common.Utils.RestClients;
using Luna.Common.Utils.LoggingUtils.Exceptions;
using Luna.Partner.PublicClient.DataContract.PartnerServices;
using Luna.Publish.Public.Client.DataContract;
using Luna.Routing.Clients.MLServiceClients.Interfaces;
using Luna.Routing.Data.DataContracts;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Luna.Common.LoggingUtils;
using Newtonsoft.Json.Linq;

namespace Luna.Routing.Clients.MLServiceClients
{
    public class AzureMLClient : IRealtimeEndpointClient, IPipelineEndpointClient
    {
        private const string AUTHORIZATION_HEADER = "Authorization";
        private const string BEARER_TOKEN_FORMAT = "Bearer {0}";
        private const string TOKEN_AUTHENTICATION_ENDPOINT = "https://login.microsoftonline.com/";
        private const string AUTHENTICATION_RESOURCE_ID = "https://management.core.windows.net";

        private readonly AzureMLWorkspaceConfiguration _config;
        private readonly AzureMLCache _cache;
        private readonly HttpClient _httpClient;
        private string _accessToken;

        public AzureMLClient(HttpClient httpClient, AzureMLWorkspaceConfiguration config)
        {
            this._httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            this._config = config;
            this._cache = new AzureMLCache();
            Task task = this.RefreshAccessToken();
            task.Wait();
        }

        /// <summary>
        /// Call the realtime endpoint
        /// </summary>
        /// <param name="operationName">The operation name</param>
        /// <param name="input">The input in JSON format</param>
        /// <param name="headers">The headers</param>
        /// <returns>The response</returns>
        public async Task<HttpResponseMessage> CallRealtimeEndpoint(
            string operationName,
            string input,
            BaseAPIVersionProp versionProperties,
            LunaRequestHeaders headers)
        {
            return await CallRealtimeEndpointInternal(operationName, input, versionProperties, headers);
        }

        /// <summary>
        /// Call the realtime endpoint
        /// </summary>
        /// <param name="operationName">The operation name</param>
        /// <param name="input">The input in JSON format</param>
        /// <param name="headers">The headers</param>
        /// <returns>The response</returns>
        private async Task<HttpResponseMessage> CallRealtimeEndpointInternal(
            string operationName,
            string input,
            BaseAPIVersionProp versionProperties,
            LunaRequestHeaders headers,
            bool shouldRefreshCacheAndRetry = true)
        {
            AzureMLRealtimeEndpointAPIVersionProp prop = (AzureMLRealtimeEndpointAPIVersionProp)versionProperties;
            var result = prop.Endpoints.Where(x => x.OperationName == operationName).SingleOrDefault();
            if (result == null)
            {
                throw new LunaNotFoundUserException(string.Format(ErrorMessages.OPERATION_DOES_NOT_EXIST, operationName));
            }

            var endpointName = result.EndpointName;

            var endpoint = await GetRealtimeEndpoint(endpointName);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, endpoint.Url);
            request.Content = new StringContent(input);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            if (endpoint.AuthEnabled && !endpoint.AadAuthEnabled)
            {
                request.Headers.Add(AUTHORIZATION_HEADER, string.Format(BEARER_TOKEN_FORMAT, endpoint.Key));
            }
            else if (endpoint.AadAuthEnabled)
            {
                request.Headers.Add(AUTHORIZATION_HEADER, string.Format(BEARER_TOKEN_FORMAT, this._accessToken));
            }

            headers.AddToHttpRequestHeaders(request.Headers);

            var response = await this._httpClient.SendAsync(request);

            if (shouldRefreshCacheAndRetry && 
                (response.StatusCode == HttpStatusCode.NotFound || response.StatusCode == HttpStatusCode.Unauthorized))
            {
                // TODO: refresh cache
                return await CallRealtimeEndpointInternal(operationName, input, versionProperties, headers, false);
            }

            return response;
        }

        /// <summary>
        /// Execute pipeline by calling the pipeline endpoint
        /// </summary>
        /// <param name="appName">the application name</param>
        /// <param name="apiName">the API name</param>
        /// <param name="versionName">the version name</param>
        /// <param name="operationName">the operation name</param>
        /// <param name="operationId">the operation id</param>
        /// <param name="input">the input in JSON format</param>
        /// <param name="headers">The headers</param>
        /// <param name="predecessorOperationId">The predecessor operation id if specified</param>
        /// <returns>The operation id</returns>
        public async Task<OperationStatus> ExecutePipeline(string appName, 
            string apiName, 
            string versionName,
            string operationName,
            string operationId,
            string input,
            BaseAPIVersionProp versionProperties,
            LunaRequestHeaders headers,
            string predecessorOperationId = null)
        {
            AzureMLPipelineEndpointAPIVersionProp prop = (AzureMLPipelineEndpointAPIVersionProp)versionProperties;
            var result = prop.Endpoints.Where(x => x.OperationName == operationName).SingleOrDefault();
            if (result == null)
            {
                throw new LunaNotFoundUserException(string.Format(ErrorMessages.OPERATION_DOES_NOT_EXIST, operationName));
            }

            var endpointId = result.EndpointId;

            var content = new PipelineRunRequestBody(
                appName,
                apiName,
                versionName,
                operationId,
                operationName,
                input,
                versionProperties,
                headers,
                predecessorOperationId);

            var url = string.Format(@"https://{0}.api.azureml.ms/pipelines/v1.0{1}/PipelineRuns/PipelineSubmit/{2}",
               this._config.Region,
               this._config.ResourceId,
               endpointId);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Content = new StringContent(JsonConvert.SerializeObject(content));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            headers.AddToHttpRequestHeaders(request.Headers);
            var response = await SendRequestWithRetryAfterTokenRefresh(request);

            if (!response.IsSuccessStatusCode)
            {
                throw new LunaServerException(await response.Content.ReadAsStringAsync());
            }
            else
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var responseBody = JsonConvert.DeserializeObject<PipelineRunResponseBody>(responseContent);
                return responseBody.GetOperationStatus();
            }

        }

        /// <summary>
        /// List operations
        /// </summary>
        /// <param name="versionProperties">The version properties</param>
        /// <param name="headers">The headers</param>
        /// <param name="filterString">The filter string</param>
        /// <returns>The operations</returns>
        public async Task<List<OperationStatus>> ListOperations(BaseAPIVersionProp versionProperties,
            LunaRequestHeaders headers,
            string filterString = null)
        {
            var result = await ListOperationsInternal(
                versionProperties, 
                headers, 
                filterString ?? $"Tags/SubscriptionId eq {headers.SubscriptionId}");

            return result.Value.Select(x => x.GetOperationStatus()).ToList();
        }

        private async Task<PipelineRunListResponseBody> ListOperationsInternal(BaseAPIVersionProp versionProperties,
            LunaRequestHeaders headers,
            string filterString)
        {
            var url = string.Format(@"https://{0}.api.azureml.ms/history/v1.0{1}/experiments/{2}/runs:query",
               this._config.Region,
               this._config.ResourceId,
               headers.SubscriptionId);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);

            var filter = new PipelineRunFilter()
            {
                Filter = filterString
            };

            request.Content = new StringContent(JsonConvert.SerializeObject(filter));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            headers.AddToHttpRequestHeaders(request.Headers);
            var response = await SendRequestWithRetryAfterTokenRefresh(request);

            if (!response.IsSuccessStatusCode)
            {
                throw new LunaServerException(await response.Content.ReadAsStringAsync());
            }
            else
            {
                var content = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<PipelineRunListResponseBody>(content);

            }
        }

        /// <summary>
        /// Cancel an operation
        /// </summary>
        /// <param name="operationId">The operation id</param>
        /// <param name="versionProperties">The version properties</param>
        /// <param name="headers">The headers</param>
        /// <returns></returns>
        public async Task CancelOperation(string operationId,
        BaseAPIVersionProp versionProperties,
        LunaRequestHeaders headers)
        {
            var queryResult = await ListOperationsInternal(versionProperties,
                headers,
                filterString: $"Tags/SubscriptionId eq {headers.SubscriptionId} and Tags/OperationId eq {operationId}");

            if (queryResult.Value.Count == 0)
            {
                throw new LunaNotFoundUserException(string.Format(ErrorMessages.OPERATION_ID_DOES_NOT_EXIST, operationId));
            }

            var run = queryResult.Value[0];

            if (!ExecutionStatus.IsAMLPipelineRunCancellable(run.Status))
            {
                throw new LunaConflictUserException(
                    string.Format(ErrorMessages.CAN_NOT_CANCEL_EXECUTION, 
                        operationId, 
                        ExecutionStatus.FromAzureMLPipelineRunStatusDetail(run.Status)));
            }

            var url = string.Format(@"https://{0}.api.azureml.ms/pipelines/v1.0{1}/PipelineRuns/{2}/Cancel",
               this._config.Region,
               this._config.ResourceId,
               run.RunId);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);

            headers.AddToHttpRequestHeaders(request.Headers);
            var response = await SendRequestWithRetryAfterTokenRefresh(request);

            if (!response.IsSuccessStatusCode)
            {
                throw new LunaServerException(await response.Content.ReadAsStringAsync());
            }
        }

        /// <summary>
        /// Get the pipeline execution status
        /// </summary>
        /// <param name="operationId">The operation id</param>
        /// <param name="versionProperties">The version properties</param>
        /// <param name="headers">The headers</param>
        /// <returns>The operation status</returns>
        public async Task<OperationStatus> GetPipelineExecutionStatus(string operationId,
            BaseAPIVersionProp versionProperties, 
            LunaRequestHeaders headers)
        {
            var queryResult = await ListOperationsInternal(versionProperties, 
                headers, 
                filterString: $"Tags/SubscriptionId eq {headers.SubscriptionId} and Tags/OperationId eq {operationId}");

            var executionList = queryResult.Value.Select(x => x.GetOperationStatus()).ToList();

            if (executionList.Count == 0)
            {
                throw new LunaNotFoundUserException(string.Format(ErrorMessages.OPERATION_ID_DOES_NOT_EXIST, operationId));
            }

            return executionList[0];
        }

        /// <summary>
        /// Get the pipeline execution output in Json format
        /// </summary>
        /// <param name="operationId">The operation id</param>
        /// <param name="versionProperties">The version properties</param>
        /// <param name="headers">The headers</param>
        /// <returns>The execution output in Json format</returns>
        public async Task<object> GetPipelineExecutionJsonOutput(string operationId,
            BaseAPIVersionProp versionProperties,
            LunaRequestHeaders headers)
        {
            var queryResult = await ListOperationsInternal(versionProperties,
                headers,
                filterString: $"Tags/SubscriptionId eq {headers.SubscriptionId} and Tags/OperationId eq {operationId}");

            if (queryResult.Value.Count == 0)
            {
                throw new LunaNotFoundUserException(string.Format(ErrorMessages.OPERATION_ID_DOES_NOT_EXIST, operationId));
            }

            var run = queryResult.Value[0];

            //if (!ExecutionStatus.IsAMLCompletedStatus(run.Status))
            //{
            //    throw new LunaConflictUserException(
            //        string.Format(ErrorMessages.CAN_NOT_GET_OUTPUT,
            //            operationId,
            //            ExecutionStatus.FromAzureMLPipelineRunStatusDetail(run.Status)));
            //}

            var url = string.Format(@"https://{0}.api.azureml.ms/history/v1.0{1}/runs/{2}/children",
               this._config.Region,
               this._config.ResourceId,
               run.RunId);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);

            headers.AddToHttpRequestHeaders(request.Headers);
            var response = await SendRequestWithRetryAfterTokenRefresh(request);

            if (!response.IsSuccessStatusCode)
            {
                throw new LunaServerException(await response.Content.ReadAsStringAsync());
            }
            else
            {
                var content = await response.Content.ReadAsStringAsync();
                var childrenRuns = JsonConvert.DeserializeObject<PipelineRunListResponseBody>(content);

                if (childrenRuns.Value.Count == 0)
                {
                    throw new LunaServerException($"Can not find child run for run with id {run.RunId}");
                }

                var childRunId = childrenRuns.Value[0].RunId;

                url = string.Format(@"https://{0}.api.azureml.ms/history/v1.0{1}/experiments/{2}/runs/{3}/artifacts/contentinfo?path=logs/azureml/executionlogs.txt",
                    this._config.Region,
                    this._config.ResourceId,
                    headers.SubscriptionId,
                    childRunId);

                request = new HttpRequestMessage(HttpMethod.Get, url);

                headers.AddToHttpRequestHeaders(request.Headers);
                response = await SendRequestWithRetryAfterTokenRefresh(request);

                if (!response.IsSuccessStatusCode)
                {
                    throw new LunaServerException(await response.Content.ReadAsStringAsync());
                }

                content = await response.Content.ReadAsStringAsync();

                var artifact = JsonConvert.DeserializeObject<PipelineRunArtifactContentResponseBody>(content);

                response = await this._httpClient.GetAsync(artifact.ContentUri);

                if (!response.IsSuccessStatusCode)
                {
                    throw new LunaServerException(await response.Content.ReadAsStringAsync());
                }

                content = await response.Content.ReadAsStringAsync();

                try
                {
                    var parsedContent = JObject.Parse(content);
                    return JsonConvert.DeserializeObject(content);
                }
                catch(JsonReaderException)
                {
                    return new { Result = content };
                }

            }
        }

        private async Task RefreshAccessToken()
        {
            var key = this._config.ClientSecret;
            var credential = new ClientCredential(this._config.ClientId, key);
            var authContext = new AuthenticationContext(TOKEN_AUTHENTICATION_ENDPOINT + this._config.TenantId, false);
            var token = await authContext.AcquireTokenAsync(AUTHENTICATION_RESOURCE_ID, credential);
            this._accessToken = token.AccessToken;
        }

        private async Task<AzureMLPipelineEndpointCache> GetPipelineEndpoint(string endpointName)
        {
            if (!this._cache.PipelineEndpoints.ContainsKey(endpointName))
            {
            }

            return this._cache.PipelineEndpoints[endpointName];
        }

        private async Task<AzureMLRealtimeEndpointCache> GetRealtimeEndpoint(string endpointName)
        {
            if (!this._cache.RealTimeEndpoints.ContainsKey(endpointName))
            {
                var endpointInfo = await GetRealtimeEndpointInfo(endpointName);

                var endpoint = new AzureMLRealtimeEndpointCache()
                {
                    Url = endpointInfo.ScoringUri,
                    AuthEnabled = endpointInfo.AuthEnabled,
                    AadAuthEnabled = endpointInfo.AadAuthEnabled
                };

                if (endpointInfo.AuthEnabled && !endpointInfo.AadAuthEnabled)
                {
                    endpoint.Key = await GetRealtimeEndpointKey(endpointName);
                }

                _cache.RealTimeEndpoints.Add(endpointName, endpoint);
            }

            return this._cache.RealTimeEndpoints[endpointName];

        }

        private async Task<RealtimeEndpointResponse> GetRealtimeEndpointInfo(string endpointName, bool tokenRefreshed = false)
        {
            var url = string.Format(@"https://{0}.api.azureml.ms/modelmanagement/v1.0{1}/services/{2}",
                this._config.Region,
                this._config.ResourceId,
                endpointName);

            var request = new HttpRequestMessage(HttpMethod.Get, new Uri(url));
            var response = await SendRequestWithRetryAfterTokenRefresh(request);

            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                var endpoint = JsonConvert.DeserializeObject<RealtimeEndpointResponse>(responseContent);
                return endpoint;
            }

            throw new LunaServerException(
                string.Format("Failed to get AML realtime endpoint. Status code: {0}. Error: {1}",
                response.StatusCode,
                await response.Content.ReadAsStringAsync()));
        }

        private async Task<string> GetRealtimeEndpointKey(string endpointName, bool tokenRefreshed = false)
        {
            var url = string.Format(@"https://{0}.api.azureml.ms/modelmanagement/v1.0{1}/services/{2}/listkeys",
                this._config.Region,
                this._config.ResourceId,
                endpointName);

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(url));
            var response = await SendRequestWithRetryAfterTokenRefresh(request);

            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                var endpoint = JsonConvert.DeserializeObject<RealtimeEndpointKeysResponse>(responseContent);
                return endpoint.PrimaryKey;
            }
            else
            {
                throw new LunaServerException(
                    string.Format("Failed to get AML realtime keys. Status code: {0}. Error: {1}",
                    response.StatusCode,
                    await response.Content.ReadAsStringAsync()));
            }
        }

        private async Task<HttpResponseMessage> SendRequestWithRetryAfterTokenRefresh(HttpRequestMessage request, bool tokenRefreshed = false)
        {
            request.Headers.Add("Authorization", $"Bearer {this._accessToken}");
            var response = await this._httpClient.SendAsync(request);
            
            if (response.StatusCode == HttpStatusCode.Unauthorized && !tokenRefreshed)
            {
                await RefreshAccessToken();
                return await SendRequestWithRetryAfterTokenRefresh(request, true);
            }

            return response;
        }

        private Task<string> GetRealtimeEndpointScoringUrl(string endpointName, bool v)
        {
            throw new NotImplementedException();
        }
    }


    public class RealtimeEndpointResponse
    {
        public string Name { get; set; }
        public string ScoringUri { get; set; }

        public bool AuthEnabled { get; set; }

        public bool AadAuthEnabled { get; set; }
    }

    public class PipelineEndpointResponse
    {

    }

    public class RealtimeEndpointKeysResponse

    {
        public string PrimaryKey { get; set; }

        public string SecondaryKey { get; set; }
    }

    public class PipelineRunTags
    {
        public string UserId { get; set; }

        public string SubscriptionId { get; set; }

        public string ApplicationName { get; set; }

        public string APIName { get; set; }

        public string APIVersion { get; set; }

        public string OperationName { get; set; }

        public string OperationId { get; set; }

        public string PredecessorOperationId { get; set; }
    }

    public class PipelineRunRequestBody
    {
        public PipelineRunRequestBody(
            string appName,
            string apiName,
            string versionName,
            string operationId, 
            string operationName,
            string input,
            BaseAPIVersionProp versionProperties,
            LunaRequestHeaders headers,
            string predecessorOperationId)
        {
            this.ExperimentName = headers.SubscriptionId;
            this.ParameterAssignments = JsonConvert.DeserializeObject<JObject>(input);
            Tags = new PipelineRunTags()
            {
                UserId = headers.UserId,
                SubscriptionId = headers.SubscriptionId,
                ApplicationName = appName,
                APIName = apiName,
                APIVersion = versionName,
                OperationName = operationName,
                OperationId = operationId,
                PredecessorOperationId = predecessorOperationId
            };
        }

        public string ExperimentName { get; set; }

        public int RunType { get { return 0; } }

        public string RunSource { get { return "Luna"; } }

        public JObject ParameterAssignments { get; set; }

        public PipelineRunTags Tags { get; set; }
    }

    public class PipelineRunResponseBody
    {
        public string Id { get; set; }

        public PipelineRunStatus Status { get; set; }

        public PipelineRunTags Tags { get; set; }

        public OperationStatus GetOperationStatus()
        {
            return new OperationStatus()
            {
                OperationId = Tags.OperationId,
                OperationName = Tags.OperationName,
                StartTime = Status.CreationTime,
                EndTime = Status.EndTime,
                Status = ExecutionStatus.FromAzureMLPipelineRunStatusCode(Status.StatusCode)
            };
        }
    }

    public class PipelineRunStatus
    {
        public int StatusCode { get; set; }

        public string StatusDetail { get; set; }

        public DateTime CreationTime { get; set; }

        public DateTime? EndTime { get; set; }

    }

    public class PipelineRunListResponseEntity
    {

        public string RunId { get; set; }
        public string Id { get; set; }

        public string Status { get; set; }

        public DateTime? StartTimeUtc { get; set; }

        public DateTime? EndTimeUtc { get; set; }

        public PipelineRunTags Tags { get; set; }

        public OperationStatus GetOperationStatus()
        {
            return new OperationStatus()
            {
                OperationId = Tags.OperationId,
                OperationName = Tags.OperationName,
                StartTime = StartTimeUtc,
                EndTime = EndTimeUtc,
                Status = ExecutionStatus.FromAzureMLPipelineRunStatusDetail(Status)
            };
        }
    }

    public class PipelineRunListResponseBody
    {
        public List<PipelineRunListResponseEntity> Value { get; set; }
    }

    public class PipelineRunArtifactContentResponseBody
    {
        public string ContentUri { get; set; }
    }

    public class PipelineRunFilter
    {
        public string Filter { get; set; }
    }
}
