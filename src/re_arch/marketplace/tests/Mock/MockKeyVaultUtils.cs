using Luna.Common.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Luna.Marketplace.Test.Mock
{
    public class MockKeyVaultUtils : IAzureKeyVaultUtils
    {
        public async Task<bool> DeleteSecretAsync(string secretName)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> DisableSecretAsync(string secretName)
        {
            throw new NotImplementedException();
        }

        public async Task<string> GetSecretAsync(string secretName)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> SetSecretAsync(string secretName, string value)
        {
            throw new NotImplementedException();
        }
    }
}
