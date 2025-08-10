using Microsoft.EntityFrameworkCore.Metadata.Internal;
using rulebot_backend.BLL.Definition;
using rulebot_backend.DAL.Definition;
using rulebot_backend.Entities;
using System.Security.Cryptography;
using System.Text;

namespace rulebot_backend.BLL.Implementation
{
    public class ConnectionService: IConnectionService
    {
        private static readonly string Key = "E9fG6vJ1uA7zT3kB4yXw9LsHd8qWeRmZ";
        private static readonly string IV = "A1b2C3d4E5f6G7h8";

        IConnectionRepository _connRepo;
        public ConnectionService(IConnectionRepository connRepo)
        {
            _connRepo = connRepo;
        }

        public Boolean checkConnection(string connectionString)
        {
            if(_connRepo.checkConnectionString(connectionString))
            {                
                return true;
            }

            return false;
        }

        public void StoreConnectionString(HttpContext context, string connectionString, string key)
        {
            var encrypted = Encrypt(connectionString);
            context.Session.SetString(key, encrypted);
        }

        public string? GetDecryptedConnectionString(HttpContext context, string key)
        {
            var encrypted = context.Session.GetString(key);
            return encrypted == null ? null : Decrypt(encrypted);
        }

        private static string Encrypt(string plainText)
        {
            using var aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(Key);
            aes.IV = Encoding.UTF8.GetBytes(IV);

            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream();
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            using (var writer = new StreamWriter(cs))
                writer.Write(plainText);

            return Convert.ToBase64String(ms.ToArray());
        }

        private static string Decrypt(string cipherText)
        {
            using var aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(Key);
            aes.IV = Encoding.UTF8.GetBytes(IV);

            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream(Convert.FromBase64String(cipherText));
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var reader = new StreamReader(cs);
            return reader.ReadToEnd();
        }
    }
}
