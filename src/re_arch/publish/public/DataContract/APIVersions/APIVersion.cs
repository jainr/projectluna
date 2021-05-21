using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Publish.Public.Client.DataContract
{
    public class APIVersion
    {
        public APIVersion(string name, BaseAPIVersionProp properties)
        {
            this.Name = name;
            this.Properties = properties;
        }

        public string Name { get; set; }

        public BaseAPIVersionProp Properties { get; set; }
    }
}
