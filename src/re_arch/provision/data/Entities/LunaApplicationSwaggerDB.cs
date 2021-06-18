using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Provision.Data
{
    public class LunaApplicationSwaggerDB
    {

        [JsonIgnore]
        public long Id { get; set; }

        public string ApplicationName { get; set; }

        public string SwaggerContent { get; set; }

        public long SwaggerEventId { get; set; }

        public long LastAppliedEventId { get; set; }

        public bool IsEnabled { get; set; }

        public DateTime CreatedTime { get; set; }

    }
}
