using Luna.Provision.Clients;
using Luna.Publish.Public.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Luna.Common.Test
{
    public class MockSwaggerClient : ISwaggerClient
    {
        public async Task<string> GenerateSwaggerAsync(LunaApplication app)
        {
            return "{}";
        }
    }
}
