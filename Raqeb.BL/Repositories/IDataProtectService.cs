using System.Security.Cryptography;
using System.Text;

namespace Raqeb.BL.Repositories
{
    public interface IDataProtectRepository 
    {
        string Encrypt(string input);
        string Decrypt(string cipherText);
    }

    public class DataProtectRepository : IDataProtectRepository
    {

        private readonly string Key = "23A5A8E6-9000-4D61-9E1C-6C498D14EDF5"; //Key For Encryption and Decryption

        public string Encrypt(string clearText)
        {
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
                            cs.Close();
                        }
                        clearText = Convert.ToBase64String(ms.ToArray()).Replace("/", "CfDJ8OBfQIsnvBREihT6eG7K").Replace("+", "CfDfQIsnvBREihT6eG7K").Replace("=", "CfDJ8OBfQIsnvT6eG7K");
                    }
                }
                return clearText;
            }
            catch (Exception) { return null; }

        }

        public string Decrypt(string cipherText)
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
                            cs.Close();
                        }
                        cipherText = Encoding.Unicode.GetString(ms.ToArray());
                    }
                }
                return cipherText;
            }
            catch (Exception ex)
            {
                return null;
            }

        }
    }

}