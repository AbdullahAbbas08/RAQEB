using System.Security.Cryptography;
using System.Text;

namespace Raqeb.Shared.Helpers
{
    public static class EncryptHelper
    {
        static readonly string Key = "23A5A8E6-9000-4D61-9E1C-6C498D14EDF5"; //Key For Encryption and Decryption
        public static class AppSettingsSimulation
        {
            public static string EncryptKey { get; set; } = "1203199320052021";
            public static string EncryptIV { get; set; } = "1203199320052021";
        }

        public static string Encrypt(string clearText)
        {
            if (string.IsNullOrEmpty(clearText))
                return "";
            try
            {
                if (string.IsNullOrEmpty(clearText))
                    return null;
                byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
                using (Aes encryptor = Aes.Create())
                {
                    Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(Key, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                    encryptor.Key = pdb.GetBytes(32);
                    encryptor.IV = pdb.GetBytes(16);
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(clearBytes, 0, clearBytes.Length);
                        }
                        clearText = Convert.ToBase64String(ms.ToArray()).Replace("/", "CfDJ8OBfQIsnvBREihT6eG7K").Replace("+", "CfDfQIsnvBREihT6eG7K").Replace("=", "CfDJ8OBfQIsnvT6eG7K");
                    }
                }
                return clearText;
            }
            catch (Exception) { return null; }
        }

        public static string Decrypt(string cipherText)
        {
            try
            {
                if (string.IsNullOrEmpty(cipherText))
                    return null;
                cipherText = cipherText.Replace("CfDJ8OBfQIsnvBREihT6eG7K", "/").Replace("CfDfQIsnvBREihT6eG7K", "+").Replace("CfDJ8OBfQIsnvT6eG7K", "=");
                byte[] cipherBytes = Convert.FromBase64String(cipherText);
                using (Aes encryptor = Aes.Create())
                {
                    Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(Key, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                    encryptor.Key = pdb.GetBytes(32);
                    encryptor.IV = pdb.GetBytes(16);
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(cipherBytes, 0, cipherBytes.Length);
                        }
                        cipherText = Encoding.Unicode.GetString(ms.ToArray());
                    }
                }
                return cipherText;
            }
            catch (Exception) { return null; }

        }

        public static string DecryptString(string cipherText)
        {
            Aes aes = GetEncryptionAlgorithm();
            //if(success)
            //{
            byte[] buffer = Convert.FromBase64String(cipherText);
            MemoryStream memoryStream = new MemoryStream(buffer);
            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
            StreamReader streamReader = new StreamReader(cryptoStream);
            return streamReader.ReadToEnd();
        }

        public static string EncryptString(string plainText)
        {
            Aes aes = GetEncryptionAlgorithm();
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);

            using (MemoryStream memoryStream = new MemoryStream())
            {
                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                {
                    cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                    cryptoStream.FlushFinalBlock();
                    byte[] encryptedBytes = memoryStream.ToArray();
                    return Convert.ToBase64String(encryptedBytes);
                }
            }
        }

        private static Aes GetEncryptionAlgorithm()
        {
            Aes aes = Aes.Create();
            var secret_key = Encoding.UTF8.GetBytes(AppSettingsSimulation.EncryptKey);
            var initialization_vector = Encoding.UTF8.GetBytes(AppSettingsSimulation.EncryptIV);
            aes.Key = secret_key;
            aes.IV = initialization_vector;
            return aes;
        }

        public static string ShiftString(string input, int shift)
        {
            StringBuilder sb = new StringBuilder();

            foreach (char c in input)
            {
                char shiftedChar = (char)(c + shift);
                sb.Append(shiftedChar);
            }

            return sb.ToString();
        }

    }
}
