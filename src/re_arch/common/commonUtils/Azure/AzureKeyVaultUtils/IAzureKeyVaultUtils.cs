using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Luna.Common.Utils.Azure.AzureKeyvaultUtils
{
    public interface IAzureKeyVaultUtils
    {
        /// <summary>
        /// Set a secret in key vault
        /// </summary>
        /// <param name="secretName">The secret name</param>
        /// <param name="value">The secret value</param>
        /// <returns>True if the secret is set, False otherwise</returns>
        Task<bool> SetSecretAsync(string secretName, string value);

        /// <summary>
        /// Get a secret from key vault
        /// </summary>
        /// <param name="secretName">The secret name</param>
        /// <returns>The secret value as string. Null if failed to get the value.</returns>
        Task<string> GetSecretAsync(string secretName);

        /// <summary>
        /// Disable all version of a secret from key vault
        /// </summary>
        /// <param name="secretName">The secret name</param>
        /// <returns>True if the secret is disabled, False otherwise</returns>
        Task<bool> DisableSecretAsync(string secretName);

        /// <summary>
        /// Delete a secret from key vault
        /// </summary>
        /// <param name="secretName">The secret name</param>
        /// <returns>True if the secret is deleted, False otherwise</returns>
        Task<bool> DeleteSecretAsync(string secretName);

    }
}
