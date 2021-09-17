using Luna.Common.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Luna.Common.Test
{
    public class MockKeyVaultUtils : IAzureKeyVaultUtils
    {
        private Dictionary<string, string> _mockKeyVault;

        public MockKeyVaultUtils()
        {
            _mockKeyVault = new Dictionary<string, string>();
        }

        public async Task<bool> DeleteSecretAsync(string secretName)
        {
            if (_mockKeyVault.ContainsKey(secretName))
            {
                _mockKeyVault.Remove(secretName);
                return true;
            }

            return false;
        }

        public async Task<bool> DisableSecretAsync(string secretName)
        {
            throw new NotImplementedException();
        }

        public async Task<string> GetSecretAsync(string secretName)
        {
            if (_mockKeyVault.ContainsKey(secretName))
            {
                return _mockKeyVault[secretName];
            }
            else
            {
                return null;
            }
        }

        public async Task<bool> SetSecretAsync(string secretName, string value)
        {
            if (_mockKeyVault.ContainsKey(secretName))
            {
                _mockKeyVault[secretName] = value;
            }
            else
            {
                _mockKeyVault.Add(secretName, value);
            }

            return true;
        }
    }
}
