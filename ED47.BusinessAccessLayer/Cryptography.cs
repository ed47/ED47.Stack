using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Caching;
using System.Security.Cryptography;
using System.Web.Helpers;

namespace ED47.BusinessAccessLayer
{
    /// <summary>
    /// Cryptography helper class.
    /// </summary>
    public static class Cryptography
    {
        public const string EncryptedFlag = "]~[";
        public const string IVFlag = "]*[";
        public const string KeyHashFlag = "]![";

        private static IDictionary<string, CryptoConfig> Configurations { get; set; }

        static Cryptography()
        {
            Configurations = new Dictionary<string, CryptoConfig>();
        }

        /// <summary>
        /// Configures the cryptography for the current application.
        /// </summary>
        /// <param name="symmetricAlgorithm">The instance of the symmetric algorithm.</param>
        /// <param name="key">The cryptographic key.</param>
        /// <param name="iv">The cryptographic initialization vector.</param>
        public static void Configure(SymmetricAlgorithm symmetricAlgorithm, byte[] key, byte[] iv)
        {
            // ReSharper disable RedundantArgumentDefaultValue
            var keyHash = Crypto.Hash(key, "sha256");
            // ReSharper restore RedundantArgumentDefaultValue

            if (Configurations.ContainsKey(keyHash))
                return;
            
            symmetricAlgorithm.Key = key;
            symmetricAlgorithm.IV = iv;
            Configurations.Add(keyHash, new CryptoConfig
                                        {
                                            SymmetricAlgorithm = symmetricAlgorithm,
                                            BaseIV = iv
                                        });
        }



        public static void EncryptProperties<TBusinessEntity>(object entity)
        {
            if (!Configurations.Any())
                return;

            if (entity == null)
                return;

            var encryptedProperties = Cryptography.GetEncryptedProprerties(typeof(TBusinessEntity)).ToList();

            if (encryptedProperties.Any())
            {
                var targetEntity = entity.GetType();

                foreach (var encryptedProperty in encryptedProperties)
                {
                    if (encryptedProperty.PropertyType != typeof(String))
                        continue;

                    var targetProperty = targetEntity.GetProperty(encryptedProperty.Name, BindingFlags.Public | BindingFlags.Instance);

                    if (targetProperty == null || targetProperty.PropertyType != typeof(String))
                        continue;

                    var value = targetProperty.GetValue(entity, null) as String;

                    if (String.IsNullOrWhiteSpace(value))
                        return;

                    if (CheckIsEncrypted(value))
                        return;

                    targetProperty.SetValue(entity, EncryptedFlag + Encrypt(value), null);
                }
            }
        }

        public static void DecryptProperties(object entity)
        {
            if (!Configurations.Any())
                return;

            if (entity == null)
                return;

            var encryptedProperties = Cryptography.GetEncryptedProprerties(entity.GetType()).ToList();

            if (encryptedProperties.Any())
            {

                foreach (var encryptedProperty in encryptedProperties)
                {
                    if (encryptedProperty.PropertyType != typeof(String))
                        continue;

                    var value = encryptedProperty.GetValue(entity, null) as String;

                    if (!CheckIsEncrypted(value))
                        continue;

                    encryptedProperty.SetValue(entity, Decrypt(value), null);
                }

            }
        }

        /// <summary>
        /// Decrypts a single value.
        /// </summary>
        /// <param name="value">The value to decrypt.</param>
        /// <returns>The decrypted value.</returns>
        public static string Decrypt(string value)
        {
            if (!CheckIsEncrypted(value))
                return value;
            
            var split = value.Split(new[]{']'}, StringSplitOptions.RemoveEmptyEntries).ToDictionary(el => "]" + el.Substring(0, 2), el => el.Substring(2));
            value = split[EncryptedFlag];
            string keyHash;
            split.TryGetValue(KeyHashFlag, out keyHash);
            var configuration = CryptoConfig(new[]{keyHash});
            var iv = configuration.BaseIV;
            string splitIV;

            if(split.TryGetValue(IVFlag, out splitIV))
                iv = Convert.FromBase64String(splitIV);

            using (var decryptor = configuration.SymmetricAlgorithm.CreateDecryptor(configuration.SymmetricAlgorithm.Key, iv))
            {
                using (var msEncrypt = new MemoryStream(Convert.FromBase64String(value)))
                {
                    using (var csEncrypt = new CryptoStream(msEncrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (var decrypt = new StreamReader(csEncrypt))
                        {
                            return decrypt.ReadToEnd();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Encrypts a value.
        /// </summary>
        /// <param name="value">The value to encrypt.</param>
        /// <returns>The encrypted value.</returns>
        public static string Encrypt(string value)
        {
            var configuration = Configurations.Last();
            configuration.Value.SymmetricAlgorithm.GenerateIV();
            var iv = configuration.Value.SymmetricAlgorithm.IV;

            using (var encryptor = configuration.Value.SymmetricAlgorithm.CreateEncryptor(configuration.Value.SymmetricAlgorithm.Key, iv))
            {
                using (var msEncrypt = new MemoryStream())
                {
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (var swEncrypt = new StreamWriter(csEncrypt))
                        {
                            //Write all data to the stream.
                            swEncrypt.Write(value);
                        }
                        return Convert.ToBase64String(msEncrypt.ToArray()) + IVFlag + Convert.ToBase64String(iv) + KeyHashFlag + configuration.Key;
                    }
                }
            }
        }

        /// <summary>
        /// Checks if a value is encrypted.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <returns><c>True</c> if the value is encrypted.</returns>
        public static bool CheckIsEncrypted(string value)
        {
            if (value == null)
                return false;

            return value.StartsWith(EncryptedFlag);
        }

        /// <summary>
        /// Get all the properties that are marked as encrypted. 
        /// </summary>
        /// <param name="entityType">The type to get encrypted properties.</param>
        /// <returns>The list of properites that are marked as encrypted.</returns>
        public static IEnumerable<PropertyInfo> GetEncryptedProprerties(Type entityType)
        {
            var cacheKey = "EncryptedProperties[" + entityType.FullName + "]";
            var cache = MemoryCache.Default.Get(cacheKey) as IEnumerable<PropertyInfo>;

            if (cache == null)
            {
                cache =
                    entityType
                        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                        .Where(el => el.GetCustomAttributes(typeof(EncryptedFieldAttribute), false).Any());

                MemoryCache.Default.Add(new CacheItem(cacheKey, cache), new CacheItemPolicy { Priority = CacheItemPriority.NotRemovable });
            }

            return cache;
        }

        /// <summary>
        /// Decrypts a stream.
        /// </summary>
        /// <param name="fileStream">The stream to decrypt.</param>
        /// <param name="keyHash">The optional hash of the key used to encrypt the file.</param>
        public static Stream Decrypt(Stream fileStream, string keyHash = null)
        {
            var decryptedStream = new MemoryStream();
            var configuration = CryptoConfig(new[] { keyHash });

            using (var decryptor = configuration.SymmetricAlgorithm.CreateDecryptor(configuration.SymmetricAlgorithm.Key, configuration.BaseIV))
            {
                if(fileStream.CanSeek)
                    fileStream.Seek(0, SeekOrigin.Begin);

                using (var decrypt = new CryptoStream(fileStream, decryptor, CryptoStreamMode.Read))
                {
                    decrypt.CopyTo(decryptedStream);
                    decryptedStream.Seek(0, SeekOrigin.Begin);
                }

                return decryptedStream;
            }
        }

        /// <summary>
        ///Encrypts a stream.
        /// </summary>
        /// <param name="writeStream">The stream to encrypt into.</param>
        /// <param name="keyHash">The hash of the key used to encrypt the file.</param>
        public static Stream Encrypt(Stream writeStream, out string keyHash)
        {
            var configuration = Configurations.Last();
            keyHash = configuration.Key;
            var encryptor = configuration.Value.SymmetricAlgorithm.CreateEncryptor(configuration.Value.SymmetricAlgorithm.Key, configuration.Value.BaseIV);
            var csEncrypt = new CryptoStream(writeStream, encryptor, CryptoStreamMode.Write);
            
            return csEncrypt;
        }

        private static CryptoConfig CryptoConfig(string[] keyHash)
        {
            if (keyHash == null || keyHash.Length == 0 || keyHash[0] == null)
                return Configurations.First().Value;
            
            CryptoConfig configuration;

            if (Configurations.TryGetValue(keyHash[0], out configuration))
                return configuration;

            throw new ApplicationException(
                String.Format("Data was encrypted with key hash {0} but key is not currently configured!", keyHash[0]));
        }
    }

    /// <summary>
    /// Marks a Business Entity property as to be encrypted in the data store.
    /// Cryptography.Configure() must be called in the application initialisation.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class EncryptedFieldAttribute : Attribute
    {
    }

    internal class CryptoConfig
    {
        internal SymmetricAlgorithm SymmetricAlgorithm { get; set; }
        internal byte[] BaseIV { get; set; }
    }
}
