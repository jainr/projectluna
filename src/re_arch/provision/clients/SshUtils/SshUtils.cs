using Luna.Gallery.Public.Client;
using Luna.Publish.Public.Client;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Luna.Provision.Clients
{
    public class SshUtils : IRemoteUtils
    {
        private string _host;
        private string _userName;
        private string _privateKey;
        private string _passPhrase;

        public SshUtils(string host, string userName, string privateKey, string passPhrase)
        {
            this._host = host;
            this._userName = userName;
            this._privateKey = privateKey;
            this._passPhrase = passPhrase;
        }


        public string ReadFileContent(string fileName)
        {
            var keyFilePath = CreateSSHKeyFile();
            var file = new PrivateKeyFile(keyFilePath, this._passPhrase);
            var content = "";
            using (var client = new SshClient(this._host, this._userName, file))
            {
                var command = client.RunCommand($"cat {fileName}");
                content = command.Result;
            }

            try
            {
                // try to delete the key file
                File.Delete(keyFilePath);
            }
            catch
            {
            }

            return content;
        }

        public void DeleteWorkingDirectory(string directoryName)
        {
            var keyFilePath = CreateSSHKeyFile();
            var file = new PrivateKeyFile(keyFilePath, this._passPhrase);

            using (var client = new SshClient(this._host, this._userName, file))
            {
                client.RunCommand($"rm -rf {directoryName}");
            }

            try
            {
                // try to delete the key file
                File.Delete(keyFilePath);
            }
            catch
            {
            }
        }

        public string ExecuteCommand(string packageUrl, 
            string scriptFileName, 
            List<MarketplaceSubscriptionParameter> parameters,
            string logFile,
            string errorLogFile)
        {
            var keyFilePath = CreateSSHKeyFile();

            var file = new PrivateKeyFile(keyFilePath, this._passPhrase);
            var workingDir = Guid.NewGuid().ToString("N");
            using (var client = new SshClient(this._host, this._userName, file))
            {
                client.Connect();
                CreateWorkingDirectory(client, workingDir);
                DowloadAndUnzipPackage(client, packageUrl);
                ExecuteCommandInternal(client, scriptFileName, parameters, logFile, errorLogFile);
                client.Disconnect();
            }

            try
            {
                // try to delete the key file
                File.Delete(keyFilePath);
            }
            catch
            {
            }

            return workingDir;
        }

        private string CreateSSHKeyFile()
        {
            string filePath = Path.GetTempPath() + Guid.NewGuid().ToString("N");
            File.WriteAllText(filePath, this._privateKey);
            return filePath;
        }

        private void CreateWorkingDirectory(SshClient client, string directoryName)
        {
            client.RunCommand($"mkdir -p {directoryName}");
            client.RunCommand($"cd {directoryName}");
        }

        private void DowloadAndUnzipPackage(SshClient client, string packageUrl)
        {
            var localPackageName = Guid.NewGuid().ToString("N") + ".zip";
            client.RunCommand($"wget {packageUrl} -O {localPackageName}");
            client.RunCommand($"unzip -o {localPackageName}");
        }

        private void ExecuteCommandInternal(SshClient client, 
            string scriptFileName,
            List<MarketplaceSubscriptionParameter> parameters,
            string logFile,
            string errorLogFile)
        {
            StringBuilder result = new StringBuilder();
            result.Append($"{scriptFileName} ");
            foreach(var param in parameters)
            {
                if (param.Type.Equals(MarketplaceParameterValueType.String.ToString()))
                {
                    result.Append($"-{param.Name} \"{param.Value}\" ");
                }
                else
                {
                    result.Append($"-{param.Name} {param.Value} ");
                }
            }

            result.Append($"1>{logFile} 2>{errorLogFile} &");

            client.RunCommand(result.ToString());
        }
    }
}
