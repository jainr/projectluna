using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace agent
{
    public class RunScriptConfig
    {
        public string ScriptFileUrl {get;set;}

        public int TimeoutInSeconds {get;set;}

        public List<ScriptArgument> InputArguments {get;set;}
    }

    public class ScriptArgument
    {
        public string Option {get;set;}
        public string Value {get;set;}
    }
}
