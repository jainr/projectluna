using Luna.Common.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Luna.Gallery.Public.Client
{
    public class GalleryServiceClientConfiguration : RestClientConfiguration
    {
        public string ServiceBaseUrl { get; set; }
        public string AuthenticationKey { get; set; }
    }
}
