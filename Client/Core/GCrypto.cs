using Windows.Security.Credentials;

namespace Eudora.Net.Core
{
    public static class GCrypto
    {
        private static string ResourceName = "Eudora.Net";

        public static string EncryptString(string plainText)
        {
            string encrypted = string.Empty;

            return encrypted;
        }

        public static string DecryptString(string encrypted)
        {
            string decrypted = string.Empty;

            return decrypted;
        }

        public static string GenerateMasterKey()
        {
            return $"{Guid.NewGuid()}{Guid.NewGuid()}";
        }

        public static void StoreMasterKey(string key)
        {
            var vault = new PasswordVault();
            var credential = vault.Retrieve(ResourceName, Environment.UserName);
            if (credential is null)
            {
                vault.Add(new PasswordCredential(ResourceName, Environment.UserName, key));
            }
            else
            {
                vault.Remove(credential);
                vault.Add(new PasswordCredential(ResourceName, Environment.UserName, key));
            }
        }

        public static string GetMasterKey()
        {
            var vault = new PasswordVault();
            var credential = vault.Retrieve(ResourceName, Environment.UserName);
            if(credential is null)
            {
                return string.Empty;
            }
            return credential.Password;
        }
    }
}
