using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Gallery.Public.Client.DataContracts
{
    public class PublishedLunaApplication
    {
        public PublishedLunaApplication()
        {
            Tags = new List<LunaPublishedApplicationTag>();
        }

        public string UniqueName { get; set; }

        public string DisplayName { get; set; }

        public string Description { get; set; }

        public string LogoImageUrl { get; set; }

        public string DocumentationUrl { get; set; }

        public string Publisher { get; set; }

        public List<LunaPublishedApplicationTag> Tags { get; set; }

        public LunaApplicationDetails Details { get; set; }
    }
}
