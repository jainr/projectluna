using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Gallery.Data
{
    public class LunaApplicationSwaggerDBView
    {
        [JsonIgnore]
        public long Id { get; set; }

        public string ApplicationName { get; set; }

        public string SwaggerContent { get; set; }

        public long SwaggerEventId { get; set; }

    }
}
