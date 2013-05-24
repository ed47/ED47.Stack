using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Caching;
using System.Security.Cryptography;

namespace ED47.BusinessAccessLayer
{
    /// <summary>
    /// Cryptography helper class.
    /// </summary>
    public static class Cryptography
    {
        public const string EncryptedFlag = "]~[";

        private static SymmetricAlgorithm SymmetricAlgorithm { get; set; }

        /// <summary>
        /// Configures the cryptography for the current application.
        /// </summary>
        /// <param name="symmetricAlgorithm">The instance of the symmetric algorithm.</param>
        /// <param name="key">The cryptographic key.</param>
        /// <param name="iv">The cryptographic initialization vector.</param>
        public static void Configure(SymmetricAlgorithm symmetricAlgorithm, byte[] key, byte[] iv)
        {
            SymmetricAlgorithm = symmetricAlgorithm;
            SymmetricAlgorithm.Key = key;
            SymmetricAlgorithm.IV = iv;
        }

        public static void EncryptProperties<TBusinessEntity>(object entity)
        {
            if (SymmetricAlgorithm == null)
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
            if (SymmetricAlgorithm == null)
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

            value = value.Substring(EncryptedFlag.Length);

            using (var decryptor = SymmetricAlgorithm.CreateDecryptor(SymmetricAlgorithm.Key, SymmetricAlgorithm.IV))
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
            using (var encryptor = SymmetricAlgorithm.CreateEncryptor(SymmetricAlgorithm.Key, SymmetricAlgorithm.IV))
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
                        return Convert.ToBase64String(msEncrypt.ToArray());
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
        public static Stream Decrypt(Stream fileStream)
        {
            var decryptedStream = new MemoryStream();

            using (var decryptor = SymmetricAlgorithm.CreateDecryptor(SymmetricAlgorithm.Key, SymmetricAlgorithm.IV))
            {
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
        public static Stream Encrypt(Stream writeStream)
        {
            var encryptor = SymmetricAlgorithm.CreateEncryptor(SymmetricAlgorithm.Key, SymmetricAlgorithm.IV);
            var csEncrypt = new CryptoStream(writeStream, encryptor, CryptoStreamMode.Write);

            return csEncrypt;
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
}
