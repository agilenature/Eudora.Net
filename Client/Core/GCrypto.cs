using System.Security.Cryptography;
using System.Text;
using Windows.Security.Credentials;

namespace Eudora.Net.Core
{
    public static class GCrypto
    {
        private static Encoding encoding = Encoding.UTF8;
        private static readonly string ResourceName = "Eudora.Net";

        public static string EncryptString(string plainText)
        {
            string encrypted = string.Empty;

            try
            {
                var plainBytes = encoding.GetBytes(plainText);
                var encryptedBytes = ProtectedData.Protect(plainBytes, null, DataProtectionScope.CurrentUser);
                encrypted = Convert.ToBase64String(encryptedBytes);
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
                var encryptedBytes = Convert.FromBase64String(encrypted);
                var decryptedBytes = ProtectedData.Unprotect(encryptedBytes, null, DataProtectionScope.CurrentUser);
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
