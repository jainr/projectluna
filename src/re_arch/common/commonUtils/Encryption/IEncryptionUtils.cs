using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Luna.Common.Utils
{
    public interface IEncryptionUtils
    {
        /// <summary>
        /// Encrypt string with symmetric key
        /// If the string is already encrypted using the same symmetric key, it will not be encrypted again
        /// Consider a string is encrypted with the same symmetric key when:
        /// 1. The string contains the luna signiture
        /// 2. The string can be decrypted with the current symmetric key
        /// </summary>
        /// <param name="input">The string needs to be encrypted</param>
        /// <returns>The encrypte base64 string</returns>
        Task<string> EncryptStringWithSymmetricKeyAsync(string input);

        /// <summary>
        /// Decrypt string with symmetric key
        /// </summary>
        /// <param name="input">The encrypted string</param>
        /// <returns>The decrytped string. null if decryption failed</returns>
        Task<string> DecryptStringWithSymmetricKeyAsync(string input);
    }
}
