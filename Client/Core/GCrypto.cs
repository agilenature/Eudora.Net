using System.Security.Cryptography;
using System.Text;
using Windows.Security.Credentials;

namespace Eudora.Net.Core
{
    public static class GCrypto
    {
        private static Encoding encoding = Encoding.UTF8;
        private static readonly string ResourceName = "Eudora.Net";

        private static readonly byte[] IV =
        {
            0x01, 0x02, 0x03, 0x04, 0x05,
            0x01, 0x02, 0x03, 0x04, 0x05,
            0x01
        };

        private static byte[] GetIV()
        {
            //return encoding.GetBytes(GetMasterKey());
            return IV;
        }

        public static string EncryptString(string plainText)
        {
            string encrypted = string.Empty;

            try
            {
                using Aes aes = Aes.Create();
                aes.Key = GetIV();
                var encryptedBytes = aes.EncryptCbc(encoding.GetBytes(plainText), GetIV(), PaddingMode.Zeros);
                encrypted = encoding.GetString(encryptedBytes);
            }
            catch(Exception ex)
            {
                FaultReporter.Error(ex);
            }

            return encrypted;
        }

        public static string DecryptString(string encrypted)
        {
            string decrypted = string.Empty;

            try
            {
                using Aes aes = Aes.Create();
                aes.Key = GetIV();
                var decryptedBytes = aes.DecryptCbc(encoding.GetBytes(encrypted), GetIV(), PaddingMode.Zeros);
                decrypted = encoding.GetString(decryptedBytes);
            }
            catch(Exception ex)
            {
                FaultReporter.Error(ex);
            }

            return decrypted;
        }

        public static string GenerateMasterKey()
        {
            string key = $"{Guid.NewGuid()}";
            key = key.Replace("-", "").Trim();
            key = key.Substring(0, 16);
            return key;
        }

        public static void SetMasterKey(string key)
        {
            var vault = new PasswordVault();

            try
            {
                var credential = vault.Retrieve(ResourceName, Environment.UserName);

                // If a key exists already for this installation of Eudora.Net,
                // don't bother editing the record. Just supplant it with the new key.
                // TODO: Investigate the security implications of this thinking
                if (credential is not null)
                {
                    vault.Remove(credential);
                }
            }
            catch(Exception ex)
            {
                FaultReporter.Warning(ex);
            }
            
            vault.Add(new PasswordCredential(ResourceName, Environment.UserName, key));
        }

        public static string GetMasterKey()
        {
            var vault = new PasswordVault();

            try
            {
                var credential = vault.Retrieve(ResourceName, Environment.UserName);
                if (credential is null)
                {
                    return string.Empty;
                }
                return credential.Password;
            }
            catch(Exception ex)
            {
                FaultReporter.Warning(ex);
                return string.Empty; 
            }
        }
    }
}
