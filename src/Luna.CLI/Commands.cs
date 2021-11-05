using Azure.Core;
using Azure.Identity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Luna.CLI
{

    public class CLIConfig
    {
        public Guid TenantId { get; set; }
        public Guid ClientId { get; set; }
        public string BaseUrl { get; set; }
    }

    public class DeviceCodeResponse
    {
        public string user_code { get; set; }

        public string device_code { get; set; }

        public string verification_uri { get; set; }

        public string message { get; set; }
    }

    public class AccessToken
    {
        public string Token { get; set; }

        public DateTimeOffset ExpiresOn { get; set; }
    }

    public class Commands
    {
        private const string CONFIG_FILE = "cli.config";
        private const string TOKEN_CACHE_FILE = "token.cache";

        static private List<string> _validResources = new List<string>(
            new string[] { "app", "api", "api-version", "config", "login" });

        static private Dictionary<string, List<string>> _validOperations = new Dictionary<string, List<string>>(); 

        static private Dictionary<string, List<string>> _requiredArguments = new Dictionary<string, List<string>>();

        static private Dictionary<string, string> _requestUrls = new Dictionary<string, string>();

        static private Dictionary<string, HttpMethod> _httpMethods = new Dictionary<string, HttpMethod>();

        static private CLIConfig _config;

        AccessToken _token;

        private static HttpClient _httpClient = new HttpClient();

        static private void LoadConstants()
        {
            _validOperations.Add("config", new List<string>(new string[] { "show", "set" }));
            _validOperations.Add("app", new List<string> (new string[] { "create", "update", "get", "list", "delete"}));
            _validOperations.Add("api", new List<string>(new string[] { "create", "update", "get", "list", "delete" }));
            _validOperations.Add("api-version", new List<string>(new string[] { "create", "update", "get", "list", "delete" }));

            _requiredArguments.Add("config-show", new List<string>(new string[] { }));
            _requiredArguments.Add("config-set", new List<string>(new string[] { "tenantId", "clientId", "baseUrl" }));

            _requiredArguments.Add("app-create", new List<string>(new string[] { "applicationName", "displayName", "owner", "description" }));
            _requiredArguments.Add("app-update", new List<string>(new string[] { "applicationName" }));
            _requiredArguments.Add("app-get", new List<string>(new string[] { "applicationName" }));
            _requiredArguments.Add("app-list", new List<string>(new string[] { }));
            _requiredArguments.Add("app-delete", new List<string>(new string[] { "applicationName" }));

            _requiredArguments.Add("api-create", new List<string>(new string[] { "applicationName", "apiName", "apiDisplayName", "apiType", "description" }));
            _requiredArguments.Add("api-update", new List<string>(new string[] { "applicationName", "apiName" }));
            _requiredArguments.Add("api-get", new List<string>(new string[] { "applicationName", "apiName" }));
            _requiredArguments.Add("api-list", new List<string>(new string[] { "applicationName" }));
            _requiredArguments.Add("api-delete", new List<string>(new string[] { "applicationName", "apiName" }));

            _requiredArguments.Add("api-version-create", new List<string>(new string[] { "applicationName", "apiName", "versionName" }));
            _requiredArguments.Add("api-version-update", new List<string>(new string[] { "applicationName", "apiName", "versionName" }));
            _requiredArguments.Add("api-version-get", new List<string>(new string[] { "applicationName", "apiName", "versionName" }));
            _requiredArguments.Add("api-version-list", new List<string>(new string[] { "applicationName", "apiName" }));
            _requiredArguments.Add("api-version-delete", new List<string>(new string[] { "applicationName", "apiName", "versionName" }));

            _requestUrls.Add("app", "{0}/api/applications/{1}");
            _requestUrls.Add("api", "{0}/api/applications{1}/apis/{2}");
            _requestUrls.Add("api-version", "{0}/api/applications/{1}/apis/{2}/versions/{3}");

            _httpMethods.Add("create", HttpMethod.Put);
            _httpMethods.Add("update", HttpMethod.Put);
            _httpMethods.Add("get", HttpMethod.Get);
            _httpMethods.Add("list", HttpMethod.Get);
            _httpMethods.Add("delete", HttpMethod.Delete);
        }

        static private Uri GetUri(string format, Dictionary<string, string> arguments)
        {
            var url = string.Format(format,
                _config.BaseUrl,
                arguments.ContainsKey("applicationName") ? arguments["applicationName"] : "",
                arguments.ContainsKey("apiName") ? arguments["apiName"] : "",
                arguments.ContainsKey("versionName") ? arguments["versionName"] : ""
                );

            return new Uri(url);
        }

        static private async Task RefreshTokenAsync()
        {
            Func<DeviceCodeInfo, CancellationToken, Task> PrintDeviceCode = (code, cancellationToken) =>
            {
                Console.WriteLine(code.Message);

                return Task.CompletedTask;
            };

            DeviceCodeCredentialOptions options = new DeviceCodeCredentialOptions();
            options.TenantId = _config.TenantId.ToString();
            options.ClientId = _config.ClientId.ToString();
            options.DeviceCodeCallback = PrintDeviceCode;
            DeviceCodeCredential credential = new DeviceCodeCredential(options);
            TokenRequestContext tokenRequestContext = new TokenRequestContext(new string []{ $"api://{_config.ClientId}/user_impersonation" });
            var token = credential.GetToken(tokenRequestContext);

            AccessToken accessToken = new AccessToken
            {
                Token = token.Token,
                ExpiresOn = token.ExpiresOn
            };

            using (StreamWriter sw = new StreamWriter(TOKEN_CACHE_FILE))
            {
                sw.Write(JsonConvert.SerializeObject(accessToken));
            }
            Console.WriteLine("Access token is obtained.");
        }

        static private string GetToken()
        {
            try
            {
                using (StreamReader sr = new StreamReader(TOKEN_CACHE_FILE))
                {
                    var content = sr.ReadToEnd();
                    var token = JsonConvert.DeserializeObject<AccessToken>(content);
                    // Give 5 min buffer
                    if (token.ExpiresOn.AddMinutes(-5) > DateTime.UtcNow)
                    {
                        return token.Token;
                    }

                    throw new Exception("Token expired");
                }
            }
            catch(Exception)
            {
                PrintError("Access token is invalid or expired. Please run 'luna login' to obtain access token.");
                return null;
            }
        }

        static private void PrintUsage()
        {
            Console.WriteLine("Usage");
        }

        static private void PrintError(string errorMessage)
        {
            Console.WriteLine($"[Error]: {errorMessage}");
        }

        static private void ShowConfig()
        {
            Console.WriteLine($"Tenant ID: {_config.TenantId}");
            Console.WriteLine($"Client ID: {_config.ClientId}");
            Console.WriteLine($"Base URL: {_config.BaseUrl}");
        }

        static private bool SetConfig(Dictionary<string, string> args)
        {
            try
            {
                var content = JsonConvert.SerializeObject(new CLIConfig
                {
                    TenantId = Guid.Parse(args["tenantId"]),
                    ClientId = Guid.Parse(args["clientId"]),
                    BaseUrl = args["baseUrl"]
                });

                using (StreamWriter sw = new StreamWriter(CONFIG_FILE))
                {
                    sw.Write(content);
                }
            }
            catch (Exception)
            {
                PrintError("The CLI configuration is invalid.");
                return false;
            }
            return true;
        }

        static private bool ReadConfig()
        {
            try
            {
                using (StreamReader sr = new StreamReader(CONFIG_FILE))
                {
                    var content = sr.ReadToEnd();
                    _config = JsonConvert.DeserializeObject<CLIConfig>(content);
                }

                return _config.TenantId != null && _config.ClientId != null && _config.BaseUrl != null;
            }
            catch(Exception)
            {
                PrintError("The CLI configuration is invalid. Please run 'luna config set' to setup the configuration.");
                return false;
            }
        }

        static async Task Main(string[] args)
        {

            if (args.Length < 2)
            {
                if (args.Length == 1 && args[0].Equals("login"))
                {
                    if (!ReadConfig())
                    {
                        return;
                    }
                    await RefreshTokenAsync();
                    return;
                }

                PrintUsage();
                return;
            }
            LoadConstants();
            if (!_validResources.Contains(args[0].ToLower()))
            {
                PrintUsage();
                return;
            }

            var resource = args[0];

            if (!_validOperations[resource].Contains(args[1].ToLower()))
            {
                PrintUsage();
                return;
            }

            var operation = args[1];

            if (!(resource.Equals("config") && operation.Equals("set")) && !ReadConfig())
            {
                return;
            }

            Dictionary<string, string> arguments = new Dictionary<string, string>();

            for (int i=2;i<args.Length;i+=2)
            {
                if (args.Length <= 2 || !args[i].StartsWith("--"))
                {
                    PrintError($"Unknown argument name {args[i]}. An argument name should start with '--'.");
                    return;
                }

                if (i+1 >= args.Length)
                {
                    PrintError($"Argument {args[i]} has no value specified.");
                    return;
                }

                arguments.Add(args[i].Substring(2, args[i].Length - 2), args[i + 1]);
            }

            foreach(var arg in _requiredArguments[$"{resource}-{operation}"])
            {
                if (!arguments.ContainsKey(arg))
                {
                    PrintError($"Required argument --{arg} is not provided.");
                    return;
                }
            }

            if (resource.Equals("config"))
            {
                if (operation.Equals("set"))
                {
                    SetConfig(arguments);
                }
                else if (operation.Equals("show"))
                {
                    ShowConfig();
                }

                return;
            }

            var uri = GetUri(_requestUrls[resource], arguments);
            var token = GetToken();
            if (token == null)
            {
                return;
            }
            HttpMethod method = _httpMethods[operation];
            var content = JsonConvert.SerializeObject(arguments);

            var request = new HttpRequestMessage { RequestUri = uri, Method = method };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            if (method == HttpMethod.Put || method == HttpMethod.Post)
            {
                request.Content = new StringContent(content);
            }

            var response = await _httpClient.SendAsync(request);

            string responseContent = await response.Content.ReadAsStringAsync();

            Console.WriteLine(responseContent);
        }
    }
}
