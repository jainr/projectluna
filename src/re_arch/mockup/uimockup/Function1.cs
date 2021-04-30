using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace uimockup
{
    public class Metric
    {
        public string Name { get; set; }
        public string Status { get; set; }
        public bool IsWarning { get; set; }

        public string TargetReportName { get; set; }
    }

    public class SupportCase
    {
        public string Title { get; set; }

        public string Url { get; set; }

        public string CreatedBy { get; set; }

        public string Status { get; set; }

        public DateTime CreatedTime { get; set; }

        public DateTime LastUpdatedTime { get; set; }

        public DateTime? ResolvedTime { get; set; }

        public string IcMTicketId { get; set; }

        public string IcMTicketUrl { get; set; }
    }

    public class User
    {
        public string UserName { get; set; }

        public string UserId { get; set; }

        public string Role { get; set; }

        public DateTime CreatedTime { get; set; }
    }

    public class Function1
    {
        [FunctionName("GetHighlightedMetrics")]
        public async Task<IActionResult> GetHighlightedMetrics(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "highlightedmetrics")] HttpRequest req,
            ILogger log)
        {
            var result = new List<Metric>();

            result.Add(new Metric()
            {
                Name = "Active Installation",
                Status = "15",
                IsWarning = false,
                TargetReportName = "LunaApp"
            });

            result.Add(new Metric()
            {
                Name = "New installation in past 30 days",
                Status = "2",
                IsWarning = false,
                TargetReportName = "LunaApp"
            });

            result.Add(new Metric()
            {
                Name = "Customer churn in past 30 days",
                Status = "1",
                IsWarning = true,
                TargetReportName = "LunaApp"
            });

            result.Add(new Metric()
            {
                Name = "Total number of requests",
                Status = "1590",
                IsWarning = false,
                TargetReportName = "Usage"
            });

            result.Add(new Metric()
            {
                Name = "Active support tickets",
                Status = "2",
                IsWarning = false,
                TargetReportName = "Usage"
            });

            return new OkObjectResult(result);
        }


        [FunctionName("GetSupportCases")]
        public async Task<IActionResult> GetOutStandingSupportCases(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "supportcases")] HttpRequest req,
            ILogger log)
        {
            var result = new List<SupportCase>();

            var type = "";

            if (req.Query.ContainsKey("type"))
            {
                type = req.Query["type"].ToString();
            }

            if (type.Equals("outstanding") || type.Equals("active"))
            {
                result.Add(new SupportCase()
                {
                    Title = "Unable to delete an application.",
                    Url = "https://aka.ms/lunaai",
                    CreatedBy = "xiwu",
                    Status = "active",
                    CreatedTime = DateTime.UtcNow.AddDays(-3),
                    LastUpdatedTime = DateTime.UtcNow.AddDays(-1),
                    IcMTicketId = "12345",
                    IcMTicketUrl = "https://www.microsoft.com"
                });

                result.Add(new SupportCase()
                {
                    Title = "Create subscription timeout.",
                    Url = "https://aka.ms/lunaai",
                    CreatedBy = "v-zacba",
                    Status = "active",
                    CreatedTime = DateTime.UtcNow.AddDays(-4),
                    LastUpdatedTime = DateTime.UtcNow.AddHours(-1),
                    IcMTicketId = "54321",
                    IcMTicketUrl = "https://www.microsoft.com"
                });
            }

            if (type.Equals("active"))
            {
                result.Add(new SupportCase()
                {
                    Title = "Typo in the publisher portal.",
                    Url = "https://aka.ms/lunaai",
                    CreatedBy = "scottgu",
                    Status = "active",
                    CreatedTime = DateTime.UtcNow.AddDays(-20),
                    LastUpdatedTime = DateTime.UtcNow.AddHours(-15),
                    IcMTicketId = "67890",
                    IcMTicketUrl = "https://www.microsoft.com"
                });
            }

            if (type.Equals("resolved"))
            {
                result.Add(new SupportCase()
                {
                    Title = "Get applciations return 400.",
                    Url = "https://aka.ms/lunaai",
                    CreatedBy = "someone",
                    Status = "resolved",
                    CreatedTime = DateTime.UtcNow.AddDays(-20),
                    LastUpdatedTime = DateTime.UtcNow.AddHours(-1),
                    ResolvedTime = DateTime.UtcNow.AddHours(-1),
                    IcMTicketId = "98765",
                    IcMTicketUrl = "https://www.microsoft.com"
                });
            }

            return new OkObjectResult(result);
        }

        [FunctionName("GetUsers")]
        public async Task<IActionResult> GetUsers(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "users")] HttpRequest req,
            ILogger log)
        {
            var result = new List<User>();

            result.Add(new User()
            {
                UserName = "Xiaochen Wu",
                UserId = "65CCC6D8-EBD0-45DC-AA02-6C19343FAA64",
                Role = "Admin",
                CreatedTime = DateTime.UtcNow.AddDays(-23)
            });

            result.Add(new User()
            {
                UserName = "Zach Bates",
                UserId = "4911FA0F-6FB6-4C0F-BF76-63A0180D4894",
                Role = "Admin",
                CreatedTime = DateTime.UtcNow.AddDays(-16)
            });

            result.Add(new User()
            {
                UserName = "Satya Nadella",
                UserId = "821F944F-55A0-4451-9EFE-772F29082841",
                Role = "Supporter",
                CreatedTime = DateTime.UtcNow.AddDays(-16)
            });

            return new OkObjectResult(result);
        }


        [FunctionName("AddUser")]
        public async Task<IActionResult> AddUser(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "users/add")] HttpRequest req,
            ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var user = JsonConvert.DeserializeObject<User>(requestBody);
            
            if (user != null)
            {
                return new OkObjectResult(user);
            }
            else
            {
                return new BadRequestObjectResult(new Exception("Format is wrong!"));
            }

        }

        [FunctionName("RemoveUser")]
        public async Task<IActionResult> RemoveUser(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "users/remove")] HttpRequest req,
            ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var user = JsonConvert.DeserializeObject<User>(requestBody);

            if (user != null)
            {
                return new OkObjectResult(user);
            }
            else
            {
                return new BadRequestObjectResult(new Exception("Format is wrong!"));
            }

        }
    }
}
