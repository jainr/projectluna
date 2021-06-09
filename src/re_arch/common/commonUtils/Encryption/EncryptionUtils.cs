using Luna.Common.Utils.LoggingUtils.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Luna.Common.Utils
{
    public class EncryptionUtils : IEncryptionUtils
    {
        private ILogger<EncryptionUtils> _logger;
        private byte[] _key;

        private const string LUNA_SIGNITURE = "luna";

        [ActivatorUtilitiesConstructor]
        public EncryptionUtils(IOptionsMonitor<EncryptionConfiguration> option,
            ILogger<EncryptionUtils> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _key = Encoding.UTF8.GetBytes(option.CurrentValue.SymmetricKey);
        }

        /// <summary>
        /// Encrypt string with symmetric key
        /// If the string is already encrypted using the same symmetric key, it will not be encrypted again
        /// Consider a string is encrypted with the same symmetric key when:
        /// 1. The string contains the luna signiture
        /// 2. The string can be decrypted with the current symmetric key
        /// </summary>
        /// <param name="input">The string needs to be encrypted</param>
        /// <returns>The encrypte base64 string</returns>
        public async Task<string> EncryptStringWithSymmetricKeyAsync(string input)
        {
            try
            {
                if (input.EndsWith(LUNA_SIGNITURE))
                {
                    if (!string.IsNullOrEmpty(await DecryptStringWithSymmetricKeyAsync(input)))
                    {
                        _logger.LogDebug("The input is already encrypted by the same key.");
                        return input;
                    }
                }

                using (MemoryStream stream = new MemoryStream())
                {
                    using (Aes aes = Aes.Create())
                    {
                        aes.Key = _key;

                        byte[] iv = aes.IV;
                        stream.Write(iv, 0, iv.Length);

                        using (CryptoStream cryptoStream = new CryptoStream(
                            stream,
                            aes.CreateEncryptor(),
                            CryptoStreamMode.Write))
                        {
                            using (StreamWriter encryptWriter = new StreamWriter(cryptoStream))
                            {
                                await encryptWriter.WriteLineAsync(input);
                            }

                            _logger.LogDebug("The string was encrypted.");

                            var array = stream.ToArray();

                            // Return the encrytped string with the Luna signiture
                            // This signiture allows us easily tell if a string is encrytped by Luna service
                            return Convert.ToBase64String(array) + LUNA_SIGNITURE;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new LunaServerException($"The encryption failed.", innerException: ex);
            }
        }

        /// <summary>
        /// Decrypt string with symmetric key
        /// </summary>
        /// <param name="input">The encrypted string</param>
        /// <returns>The decrytped string. null if decryption failed</returns>
        public async Task<string> DecryptStringWithSymmetricKeyAsync(string input)
        {
            if (!input.EndsWith(LUNA_SIGNITURE))
            {
                _logger.LogDebug("The input is not a string encrypted by Luna service.");
                return null;
            }

            // Remove the Luna signiture
            input = input.Substring(0, input.Length - LUNA_SIGNITURE.Length);

            try
            {
                byte[] buffer = Convert.FromBase64String(input);

                using (MemoryStream memoryStream = new MemoryStream(buffer))
                {
                    using (Aes aes = Aes.Create())
                    {
                        byte[] iv = new byte[aes.IV.Length];
                        int numBytesToRead = aes.IV.Length;
                        int numBytesRead = 0;
                        while (numBytesToRead > 0)
                        {
                            int n = memoryStream.Read(iv, numBytesRead, numBytesToRead);
                            if (n == 0) break;

                            numBytesRead += n;
                            numBytesToRead -= n;
                        }

                        if (numBytesRead < aes.IV.Length)
                        {
                            throw new FormatException("The input doesn't contain a valid IV.");
                        }

                        if (memoryStream.Position == memoryStream.Length)
                        {
                            throw new FormatException("The encrypted string is empty.");
                        }

                        ICryptoTransform decryptor = aes.CreateDecryptor(
                            _key, iv);

                        using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader streamReader = new StreamReader(cryptoStream))
                            {
                                var result = await streamReader.ReadToEndAsync();
                                return result;
                            }
                        }
                    }
                }
            }
            catch(FormatException ex)
            {
                _logger.LogDebug(ex.Message);
                return null;
            }
            catch(CryptographicException ex)
            {
                _logger.LogDebug(ex.Message);
                return null;
            }
            catch(Exception ex)
            {
                throw new LunaServerException($"The decryption failed.", innerException: ex);
            }
        }
    }
}
