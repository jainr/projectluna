using Luna.Publish.Public.Client;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Luna.Provision.Clients
{
    public class SwaggerClient : ISwaggerClient
    {
        private const string ROUTING_SERVICE_BASE_URL_CONFIG_NAME = "ROUTING_SERVICE_BASE_URL";
        private const string REALTIME_API_PATH_FORMAT = "/api/{0}/{1}/{2}";
        private const string OPERATION_ID_FORMAT = "{0}-{1}-{2}";

        public SwaggerClient()
        {

        }

        /// <summary>
        /// Generate swagger for a Luna application
        /// </summary>
        /// <param name="app">The luna application</param>
        /// <returns>The swagger document</returns>
        public async Task<string> GenerateSwaggerAsync(LunaApplication app)
        {
            var routingServiceUri = new Uri(Environment.GetEnvironmentVariable(ROUTING_SERVICE_BASE_URL_CONFIG_NAME, EnvironmentVariableTarget.Process));

            var swagger = new OpenApiDocument
            {
                Info = new OpenApiInfo()
                {
                    Title = app.Properties.DisplayName,
                    Version = "V1"
                },
                Servers = new List<OpenApiServer>
                {
                    new OpenApiServer { Url = routingServiceUri.GetLeftPart(UriPartial.Authority) }
                },
                Components = new OpenApiComponents()
            };

            swagger.Components.SecuritySchemes.Add("api-key", new OpenApiSecurityScheme()
            {
                Description = "Subscription key",
                Name = "api-key",
                Type = SecuritySchemeType.ApiKey,
                In = ParameterLocation.Header
            });

            swagger.Paths = new OpenApiPaths();

            foreach (var api in app.APIs)
            {
                foreach(var version in api.Versions)
                {
                    if (version.Properties.GetType() == typeof(AzureMLRealtimeEndpointAPIVersionProp))
                    {
                        foreach (var endpoint in ((AzureMLRealtimeEndpointAPIVersionProp)version.Properties).Endpoints)
                        {
                            var path = string.Format(REALTIME_API_PATH_FORMAT, app.Name, api.Name, endpoint.OperationName);
                            if (!swagger.Paths.ContainsKey(path))
                            {
                                var pathItem = new OpenApiPathItem();
                                swagger.Paths.Add(path, pathItem);
                                pathItem.Description = endpoint.Description;
                                
                                var operation = new OpenApiOperation();
                                pathItem.AddOperation(OperationType.Post, operation);
                                operation.Tags.Add(new OpenApiTag()
                                {
                                    Name = api.Name
                                });

                                operation.Summary = endpoint.Description;
                                operation.OperationId = string.Format(OPERATION_ID_FORMAT, app.Name, api.Name, endpoint.OperationName);
                                operation.Parameters.Add(CreateAPIVersionQueryParameter());
                                operation.RequestBody = CreateRequestBody();
                                operation.Responses.Add("200", CreateSuccessResponse());

                                operation.Security.Add(CreateKeyBasedSecurityReq());
                            }
                        }
                    }
                }
            }

            var content = swagger.SerializeAsJson(OpenApiSpecVersion.OpenApi2_0);
            return content;
        }

        private OpenApiSecurityRequirement CreateKeyBasedSecurityReq()
        {
            var req = new OpenApiSecurityRequirement();
            var scheme = new OpenApiSecurityScheme()
            {
                //Type = SecuritySchemeType.ApiKey,
                //In = ParameterLocation.Header,
                Reference = new OpenApiReference()
                {
                    Id = "api-key",
                    Type = ReferenceType.SecurityScheme
                }
            };

            req.Add(scheme, new List<string>());

            return req;
        }

        private OpenApiResponse CreateSuccessResponse()
        {
            var response = new OpenApiResponse();
            response.Description = "The prediction result";
            return response;
        }

        private OpenApiRequestBody CreateRequestBody()
        {
            var body = new OpenApiRequestBody();
            body.Description = "User Input";
            body.Required = true;
            return body;
        }

        private OpenApiParameter CreateAPIVersionQueryParameter()
        {
            var param = new OpenApiParameter();
            param.Name = "api-version";
            param.In = ParameterLocation.Query;
            param.Description = "API version";
            param.Required = true;
            param.Schema = new OpenApiSchema()
            {
                Type = "string"
            };
            return param;
        }
    }
}
