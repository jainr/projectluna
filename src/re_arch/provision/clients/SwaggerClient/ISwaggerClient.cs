using Luna.Publish.Public.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Luna.Provision.Clients
{
    public interface ISwaggerClient
    {
        /// <summary>
        /// Generate swagger for a Luna application
        /// </summary>
        /// <param name="app">The luna application</param>
        /// <returns>The swagger document</returns>
        Task<string> GenerateSwaggerAsync(LunaApplication app);
    }
}
