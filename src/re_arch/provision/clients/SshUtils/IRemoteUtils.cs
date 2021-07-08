using Luna.Gallery.Public.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Provision.Clients
{
    public interface IRemoteUtils
    {
        string ReadFileContent(string fileName);

        void DeleteWorkingDirectory(string directoryName);

        string ExecuteCommand(string packageUrl,
            string scriptFileName,
            List<MarketplaceSubscriptionParameter> parameters,
            string logFile,
            string errorLogFile);
    }
}
