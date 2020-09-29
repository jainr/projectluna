using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Luna.Clients.Controller.Auth;
using Luna.Clients.Exceptions;
using Luna.Clients.Models.Controller;
using Luna.Data.DataContracts.Luna.AI;
using Luna.Data.Entities;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;

namespace Luna.Clients.Controller
{
    public static class ControllerHelper
    {
        private static HttpClient HttpClient = new HttpClient();

        public static string GetLunaGeneratedUuid()
        {
            return "a" + Guid.NewGuid().ToString("N").Substring(1);
        }

        public static async Task<string> GetRegion(AMLWorkspace workspace)
        {
            var requestUri = new Uri("https://management.azure.com" + workspace.ResourceId + "?api-version=2019-05-01");
            var request = new HttpRequestMessage { RequestUri = requestUri, Method = HttpMethod.Get };

            try
            {
                var token = await ControllerAuthHelper.GetToken(workspace.AADTenantId.ToString(), workspace.AADApplicationId.ToString(), workspace.AADApplicationSecrets);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            catch (AdalServiceException)
            {
                throw new LunaBadRequestUserException($"Cannot find the AML workspace. Invalid client secret is provided", UserErrorCode.InvalidParameter);
            }

            var response = await HttpClient.SendAsync(request);
            string responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new LunaBadRequestUserException($"Cannot find the AML workspace {workspace.WorkspaceName}", UserErrorCode.InvalidParameter);
            }

            IDictionary<string, object> workspaceDetails = (IDictionary<string, object>)System.Text.Json.JsonSerializer.Deserialize(responseContent, typeof(IDictionary<string, object>));
            return workspaceDetails["location"].ToString();
        }

        public static async Task<List<AMLPipeline>> GetAllPipelines(AMLWorkspace workspace)
        {
            var requestUrl = $"https://{workspace.Region}.api.azureml.ms/pipelines/v1.0" + workspace.ResourceId + $"/pipelines";
            var requestUri = new Uri(requestUrl);
            var request = new HttpRequestMessage { RequestUri = requestUri, Method = HttpMethod.Get };

            var token = await ControllerAuthHelper.GetToken(workspace.AADTenantId.ToString(), workspace.AADApplicationId.ToString(), workspace.AADApplicationSecrets);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await HttpClient.SendAsync(request);

            string responseContent = await response.Content.ReadAsStringAsync();

            List<Dictionary<string, object>> rawPipelineList = (List<Dictionary<string, object>>)System.Text.Json.JsonSerializer.Deserialize(responseContent, typeof(List<Dictionary<string, object>>));
            List<AMLPipeline> pipelineList = new List<AMLPipeline>();
            foreach (var item in rawPipelineList)
            {
                string displayName = item.ContainsKey("Name") && item["Name"] != null ? item["Name"].ToString() : "noName";
                string id = item.ContainsKey("Id") && item["Id"] != null ? item["Id"].ToString() : "noId";
                string description = item.ContainsKey("Description") && item["Description"] != null ? item["Description"].ToString() : "noDescription";
                string createdDate = item.ContainsKey("CreatedDate") && item["CreatedDate"] != null ? item["CreatedDate"].ToString() : "noCreatedDate";
                pipelineList.Add(new AMLPipeline()
                {
                    DisplayName = displayName,
                    Id = id,
                    Description = description,
                    CreatedDate = createdDate
                });
            }
            if (!response.IsSuccessStatusCode)
            {
                throw new LunaServerException($"Query failed with response {responseContent}");
            }

            return pipelineList;
        }

    }
}
