using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace agent
{
    public static class AgentFunction
    {
        [FunctionName("HttpExample")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            Process proc = new Process();
            proc.StartInfo.FileName = "kubectl";
            proc.StartInfo.Arguments = "version";
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.Start();
            string result = "";
            while (!proc.StandardOutput.EndOfStream)
            {
                result = result + proc.StandardOutput.ReadLine();
            }

            string responseMessage = string.IsNullOrEmpty(name)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : result;

            return new OkObjectResult(responseMessage);
        }
        
        [FunctionName("runScript")]
        public static async Task<IActionResult> RunScript(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "lunadmeo/runscript")] HttpRequest req,
            ILogger log)
        {
            var content = await new StreamReader(req.Body).ReadToEndAsync();
            RunScriptConfig config = JsonConvert.DeserializeObject<RunScriptConfig>(content);
            string tmpPath = Path.GetTempPath();
            string scriptFileName = Path.Combine(tmpPath, "script.sh");
            string logFileName = Path.Combine(tmpPath, "std.log");
            string erroLogFileName = Path.Combine(tmpPath, "error.log");
            using (var client = new WebClient())
            {
                client.DownloadFile(config.ScriptFileUrl, scriptFileName);
            }

            StringBuilder sb = new StringBuilder();
            sb.Append($"-c {scriptFileName}");

            foreach(var arg in config.InputArguments)
            {
                sb.Append($" -{arg.Option}");
                sb.Append($" {arg.Value}");
            }

            sb.Append($" 1>{logFileName} 2>{erroLogFileName} &");

            Process proc = new Process();
            proc.StartInfo.FileName = "chmod";
            proc.StartInfo.Arguments = $" u+r+x {scriptFileName}";
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardError = true;
            proc.StartInfo.CreateNoWindow = true;
            proc.Start();

            proc.WaitForExit();
            log.LogInformation($"Exit code 1: {proc.ExitCode}");
            proc.Dispose();

            proc = new Process();
            proc.StartInfo.FileName = "bash";
            proc.StartInfo.Arguments = sb.ToString();
            proc.StartInfo.UseShellExecute = false;
            //proc.StartInfo.RedirectStandardOutput = true;
            //proc.StartInfo.RedirectStandardError = true;
            proc.StartInfo.CreateNoWindow = false;
            proc.Start();
            

            proc.WaitForExit();
            //log.LogWarning(proc.StandardError.ReadToEnd());
            //log.LogInformation(proc.StandardOutput.ReadToEnd());

            log.LogInformation($"Exit code 2: {proc.ExitCode}");

            return new OkObjectResult(proc.StartInfo.Arguments);
        }
    }
}
