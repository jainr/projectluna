using Luna.Marketplace.Public.Client;
using Luna.Publish.Public.Client;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
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
                client.Connect();
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
                client.Connect();
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
            List<ScriptArgument> inputArguments,
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
                DowloadAndUnzipPackage(client, packageUrl, workingDir);
                ExecuteCommandInternal(client, workingDir, scriptFileName, parameters, inputArguments, logFile, errorLogFile);
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
            var result = client.RunCommand($"mkdir -p {directoryName}");
        }

        private void DowloadAndUnzipPackage(SshClient client, string packageUrl, string workingDir)
        {
            var localPackageName = Guid.NewGuid().ToString("N") + ".zip";
            var result = client.RunCommand($"cd {workingDir}; wget \"{packageUrl}\" -O {localPackageName}");
            result = client.RunCommand("sudo apt-get install -y unzip");
            result = client.RunCommand($"cd {workingDir}; unzip -o {localPackageName}");
        }

        private void ExecuteCommandInternal(SshClient client, 
            string workingDir,
            string scriptFileName,
            List<MarketplaceSubscriptionParameter> parameters,
            List<ScriptArgument> inputArguments,
            string logFile,
            string errorLogFile)
        {
            StringBuilder result = new StringBuilder();
            var newFileName = Guid.NewGuid().ToString() + ".sh";
            // 1. go to working directory
            // 2. remove '/r' from script file (in case the file is composed in Windows)
            // 3. assign permission
            // 4. run script file with arguments
            result.Append($"cd {workingDir}; sed $'s/\r$//' ./{scriptFileName} > ./{newFileName}; chmod u+r+x ./{newFileName}; sudo ./{newFileName} ");

            foreach (var argument in inputArguments)
            {
                var param = parameters.SingleOrDefault(x => x.Name == argument.Name);
                if (param != null)
                {
                    if (param.Type.Equals(MarketplaceParameterValueType.String.ToString()))
                    {
                        result.Append($"-{argument.Option} \"{param.Value}\" ");
                    }
                    else
                    {
                        result.Append($"-{argument.Option} {param.Value} ");
                    }
                }
            }

            result.Append($"1>{logFile} 2>{errorLogFile} &");

            var execResult = client.RunCommand(result.ToString());

        }

        #region static methods

        public static SSHKeyPair GetSSHKeyPair(int keySize = 4096)
        {
            var csp = new RSACryptoServiceProvider(4096);
            SSHKeyPair keyPair = new SSHKeyPair();
            keyPair.PrivateKey = GetPrivateKey(csp);
            keyPair.PublicKey = GetPublicKey(csp);
            return keyPair;
        }

        private static string GetPrivateKey(RSACryptoServiceProvider csp)
        {
            if (csp.PublicOnly) throw new ArgumentException("CSP does not contain a private key", nameof(csp));
            var parameters = csp.ExportParameters(true);
            using (var stream = new MemoryStream())
            {
                var writer = new BinaryWriter(stream);
                writer.Write((byte)0x30);
                using (var innerStream = new MemoryStream())
                {
                    var innerWriter = new BinaryWriter(innerStream);
                    EncodeIntegerBigEndian(innerWriter, new byte[] { 0x00 });
                    EncodeIntegerBigEndian(innerWriter, parameters.Modulus);
                    EncodeIntegerBigEndian(innerWriter, parameters.Exponent);
                    EncodeIntegerBigEndian(innerWriter, parameters.D);
                    EncodeIntegerBigEndian(innerWriter, parameters.P);
                    EncodeIntegerBigEndian(innerWriter, parameters.Q);
                    EncodeIntegerBigEndian(innerWriter, parameters.DP);
                    EncodeIntegerBigEndian(innerWriter, parameters.DQ);
                    EncodeIntegerBigEndian(innerWriter, parameters.InverseQ);
                    var length = (int)innerStream.Length;
                    EncodeLength(writer, length);
                    writer.Write(innerStream.GetBuffer(), 0, length);
                }

                var base64 = Convert.ToBase64String(stream.GetBuffer(), 0, (int)stream.Length).ToCharArray();
                using (StringWriter outputStream = new StringWriter())
                {
                    outputStream.WriteLine("-----BEGIN RSA PRIVATE KEY-----");
                    // Output as Base64 with lines chopped at 64 characters
                    for (var i = 0; i < base64.Length; i += 70)
                    {
                        outputStream.WriteLine(base64, i, Math.Min(70, base64.Length - i));
                    }
                    outputStream.WriteLine("-----END RSA PRIVATE KEY-----");
                    return outputStream.ToString();
                }
            }
        }

        private static string GetPublicKey(RSACryptoServiceProvider csp)
        {
            byte[] sshrsaBytes = Encoding.Default.GetBytes("ssh-rsa");
            byte[] n = csp.ExportParameters(false).Modulus;
            byte[] e = csp.ExportParameters(false).Exponent;
            string buffer64;
            using (MemoryStream ms = new MemoryStream())
            {
                ms.Write(ToBytes(sshrsaBytes.Length), 0, 4);
                ms.Write(sshrsaBytes, 0, sshrsaBytes.Length);
                ms.Write(ToBytes(e.Length), 0, 4);
                ms.Write(e, 0, e.Length);
                ms.Write(ToBytes(n.Length + 1), 0, 4); //Remove the +1 if not Emulating Putty Gen
                ms.Write(new byte[] { 0 }, 0, 1); //Add a 0 to Emulate PuttyGen
                ms.Write(n, 0, n.Length);
                ms.Flush();
                buffer64 = Convert.ToBase64String(ms.ToArray());
            }

            return $"ssh-rsa {buffer64} xiwu@microsoft.com";
        }

        private static byte[] ToBytes(int i)
        {
            byte[] bts = BitConverter.GetBytes(i);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bts);
            }
            return bts;
        }

        private static void EncodeLength(BinaryWriter stream, int length)
        {
            if (length < 0) throw new ArgumentOutOfRangeException(nameof(length), "Length must be non-negative");
            if (length < 0x80)
            {
                // Short form
                stream.Write((byte)length);
            }
            else
            {
                // Long form
                var temp = length;
                var bytesRequired = 0;
                while (temp > 0)
                {
                    temp >>= 8;
                    bytesRequired++;
                }
                stream.Write((byte)(bytesRequired | 0x80));
                for (var i = bytesRequired - 1; i >= 0; i--)
                {
                    stream.Write((byte)(length >> (8 * i) & 0xff));
                }
            }
        }

        private static void EncodeIntegerBigEndian(BinaryWriter stream, byte[] value)
        {
            stream.Write((byte)0x02); // INTEGER
            var prefixZeros = 0;
            for (var i = 0; i < value.Length; i++)
            {
                if (value[i] != 0) break;
                prefixZeros++;
            }

            if (value.Length - prefixZeros == 0)
            {
                EncodeLength(stream, 1);
                stream.Write((byte)0);
            }
            else
            {
                if (value[prefixZeros] > 0x7f)
                {
                    // Add a prefix zero to force unsigned if the MSB is 1
                    EncodeLength(stream, value.Length - prefixZeros + 1);
                    stream.Write((byte)0);
                }
                else
                {
                    EncodeLength(stream, value.Length - prefixZeros);
                }
                for (var i = prefixZeros; i < value.Length; i++)
                {
                    stream.Write(value[i]);
                }
            }
        }
        #endregion
    }
}
