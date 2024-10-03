using System.Security.Cryptography;
using System.Text;
using Windows.Security.Credentials;

namespace Eudora.Net.Core
{
    public static class GCrypto
    {
        private static readonly string ResourceName = "Eudora.Net";

        private static readonly byte[] IV =
        {
            0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15
        };
        private static byte[] GetIV()
        {
            return IV;
        }

        public static string EncryptString(string plainText)
        {
            string encrypted = string.Empty;

            try
            {
                using Aes aes = Aes.Create();
                aes.Key = GetIV();// Encoding.Unicode.GetBytes(GetMasterKey());
                var encryptedBytes = aes.EncryptCbc(Encoding.Unicode.GetBytes(plainText), GetIV(), PaddingMode.None);
                encrypted = Encoding.Unicode.GetString(encryptedBytes);
            }
            catch(Exception ex)
            {
                Logger.Debug("GCrypto::EncryptString");
                Logger.Error(ex.Message);
            }

            return encrypted;
        }

        public static string DecryptString(string encrypted)
        {
            string decrypted = string.Empty;

            try
            {
                using Aes aes = Aes.Create();
                aes.Key = GetIV();// Encoding.Unicode.GetBytes(GetMasterKey());
                var decryptedBytes = aes.DecryptCbc(Encoding.Unicode.GetBytes(encrypted), GetIV(), PaddingMode.None);
                decrypted = Encoding.Unicode.GetString(decryptedBytes);
            }
            catch(Exception ex)
            {
                Logger.Debug("GCrypto::DecryptString");
                Logger.Error(ex.Message);
            }

            return decrypted;
        }

        public static string GenerateMasterKey()
        {
            return $"{Guid.NewGuid()}{Guid.NewGuid()}";
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
            catch
            {
                Logger.Debug("SetMasterKey() : Credential not found");
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
            catch 
            {
                Logger.Debug("GetMasterKey(): Credential not found");
                return string.Empty; 
            }
        }
    }
}
