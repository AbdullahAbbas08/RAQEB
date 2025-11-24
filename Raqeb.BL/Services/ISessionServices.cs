using Microsoft.AspNetCore.Http;
using Raqeb.DoL.Enums;
using Raqeb.Shared.Helpers;
using Newtonsoft.Json;
using PdfSharpCore.Drawing.BarCodes;
using System.Reflection;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Raqeb.BL.Services
{
    public interface ISessionServices 
    {
        HttpContext HttpContext { get; set; }
        int? UserId { get; }
        int? CustomerID { get; }
        string UserName { get; }
        int? RoleId { get; }
        int UserRoleId { get; }
        int? OrganizationId { get; }
        string UserTokenId { get; set; }
        //string MachineName { get; }
        //string MachineIP { get; }
        //string Browser { get; }
        //string Url { get; }
        string Culture { get; set; }
        bool CultureIsArabic { get; }

        string ApplicationType { get; set; }
        string EmployeeFullNameAr { get; }
        string EmployeeFullNameEn { get; }
        string OrganizationNameAr { get; }
        string OrganizationNameEn { get; }
        string RoleNameAr { get; }
        string RoleNameEn { get; }
        string IsEmployee { get; }
        string ClientIP { get; }
        string FaxUserId { get; }
        void ClearSessionsExcept(params string[] keys);
        string Decrypt(string cipherText);
    }

    public class SessionServices :  ISessionServices
    {
        private readonly IHttpContextAccessor _HttpContextAccessor;
        private readonly IEncryptionServices _EncryptionServices;

        private readonly string Key = "23A5A8E6-9000-4D61-9E1C-6C498D14EDF5"; //Key For Encryption and Decryption

        public static string DecryptStringAES(string cipherText)
        {
            if (cipherText == null || cipherText == "")
            {
                return "";
            }
            else
            {
                var keybytes = Encoding.UTF8.GetBytes("4512631236589784");
                var iv = Encoding.UTF8.GetBytes("4512631236589784");
                cipherText = cipherText.Replace(" ", "+");
                var encrypted = Convert.FromBase64String(cipherText);
                var decriptedFromJavascript = DecryptStringFromBytes(encrypted, keybytes, iv);
                return decriptedFromJavascript;
            }
        }

        public static string EncriptStringAES(string cipherText)
        {
            if (cipherText == null || cipherText == "")
            {
                return "";
            }
            else
            {
                var keybytes = Encoding.UTF8.GetBytes("4512631236589784");
                var iv = Encoding.UTF8.GetBytes("4512631236589784");

                var decriptedFromJavascript = EncryptStringToBytes(cipherText, keybytes, iv);
                var x = Convert.ToBase64String(decriptedFromJavascript).ToString();
                return x;
            }
        }
        private static string DecryptStringFromBytes(byte[] cipherText, byte[] key, byte[] iv)
        {
            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
            {
                throw new ArgumentNullException("cipherText");
            }
            if (key == null || key.Length <= 0)
            {
                throw new ArgumentNullException("key");
            }
            if (iv == null || iv.Length <= 0)
            {
                throw new ArgumentNullException("key");
            }

            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;

            // Create an RijndaelManaged object
            // with the specified key and IV.
            using (var rijAlg = new RijndaelManaged())
            {
                //Settings
                rijAlg.Mode = CipherMode.CBC;
                rijAlg.Padding = PaddingMode.PKCS7;
                rijAlg.FeedbackSize = 128;

                rijAlg.Key = key;
                rijAlg.IV = iv;

                // Create a decrytor to perform the stream transform.
                var decryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV);

                try
                {
                    // Create the streams used for decryption.
                    using (var msDecrypt = new MemoryStream(cipherText))
                    {
                        using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {

                            using (var srDecrypt = new StreamReader(csDecrypt))
                            {
                                // Read the decrypted bytes from the decrypting stream
                                // and place them in a string.
                                plaintext = srDecrypt.ReadToEnd();

                            }

                        }
                    }
                }
                catch
                {
                    plaintext = "keyError";
                }
            }

            return plaintext;
        }


        private static byte[] EncryptStringToBytes(string plainText, byte[] key, byte[] iv)
        {
            // Check arguments.
            if (plainText == null || plainText.Length <= 0)
            {
                throw new ArgumentNullException("plainText");
            }
            if (key == null || key.Length <= 0)
            {
                throw new ArgumentNullException("key");
            }
            if (iv == null || iv.Length <= 0)
            {
                throw new ArgumentNullException("key");
            }
            byte[] encrypted;
            // Create a RijndaelManaged object
            // with the specified key and IV.
            using (var rijAlg = new RijndaelManaged())
            {
                rijAlg.Mode = CipherMode.CBC;
                rijAlg.Padding = PaddingMode.PKCS7;
                rijAlg.FeedbackSize = 128;

                rijAlg.Key = key;
                rijAlg.IV = iv;

                // Create a decrytor to perform the stream transform.
                var encryptor = rijAlg.CreateEncryptor(rijAlg.Key, rijAlg.IV);

                // Create the streams used for encryption.
                using (var msEncrypt = new MemoryStream())
                {
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (var swEncrypt = new StreamWriter(csEncrypt))
                        {
                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }
            // Return the encrypted bytes from the memory stream.
            return encrypted;
        }

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
                    return "";
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
            catch (Exception) { return null; }

        }
        public SessionServices(IHttpContextAccessor httpContextAccessor, IEncryptionServices encryptionServices)
        {
            _HttpContextAccessor = httpContextAccessor;
            _EncryptionServices = encryptionServices;
        }

        public HttpContext HttpContext
        {
            get
            {
                return _HttpContextAccessor.HttpContext;
            }
            set
            {
                _HttpContextAccessor.HttpContext = value;
            }
        }

        public int? UserId
        {
            get
            {
                if (HttpContext.User == null)
                    return null;
                var claim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (claim == null)
                    return null;
                var _UserId = EncryptHelper.DecryptString(claim.Value);
                return int.Parse(_UserId);
            }
        }
        

        public string UserName
        {
            get
            {
                if (HttpContext == null || HttpContext.User == null || HttpContext.User.Identity == null)
                    return null;
                return Decrypt(HttpContext.User.Identity.Name);
            }
        }

        public int? OrganizationId
        {
            get
            {
                if (HttpContext.User == null)
                    return null;
                var claim = HttpContext.User.FindFirst(Encrypt("CurrentOrganizationId"));
                if (claim == null)
                    return null;
                return int.Parse(Decrypt(claim.Value));
            }
        }
        
        public int? CustomerID 
        {
            get
            {
                if (HttpContext.User == null)
                    return null;
                var claim = HttpContext.User.FindFirst(ClaimTypes.Name);
                if (string.IsNullOrEmpty(claim.Value))
                    return null;
                return int.Parse(claim.Value);
            }
        }

        public int UserRoleId
        {
            get
            {
                if (HttpContext.User == null)
                    return 0;
                var claim = HttpContext.User.FindFirst(Encrypt("CurrentUserRoleId"));
                if (claim == null)
                    return 0;
                return int.Parse(Decrypt(claim.Value));
            }
        }

        public int? RoleId
        {
            get
            {
                if (HttpContext.User == null)
                    return null;
                var claim = HttpContext.User.FindFirst(Encrypt("CurrentRoleId"));
                if (claim == null)
                    return null;
                return int.Parse(Decrypt(claim.Value));
            }
        }
        public string UserTokenId
        {
            get => string.IsNullOrEmpty(HttpContext.Session.GetString("userTokenId")) ? "0" : HttpContext.Session.GetString("userTokenId");
            set => HttpContext.Session.SetString("userTokenId", value.ToString());
        }

        //public string MachineName
        //{
        //    get
        //    {
        //        return string.Empty;
        //    }
        //}

        //public string MachineIP
        //{
        //    get
        //    {
        //        return string.Empty;
        //    }
        //}

        //public string Browser
        //{
        //    get
        //    {
        //        return string.Empty;
        //    }
        //}

        //public string Url
        //{


        //    get
        //    {
        //        return string.Empty;
        //    }
        //}

        public string Culture


        {
            get => (HttpContext == null || string.IsNullOrEmpty(HttpContext.Session.GetString("culture"))) ? "ar" : HttpContext.Session.GetString("culture");
            set => HttpContext.Session.SetString("culture", value.ToString());
        }

        public bool CultureIsArabic
        {
            get
            {
                return Culture == "ar";
            }
        }


        public string ApplicationType
        {
            get => string.IsNullOrEmpty(HttpContext.Session.GetString("applicationType")) ? "1" : HttpContext.Session.GetString("applicationType");
            set => HttpContext.Session.SetString("applicationType", value.ToString());
        }

        public string EmployeeFullNameAr
        {
            get
            {
                if (HttpContext?.User == null)
                    return null;
                var claim = HttpContext.User.FindFirst(Encrypt("UserFullNameAr"));
                if (claim == null)
                    return null;
                return Decrypt(claim.Value).ToString();
            }
        }

        public string EmployeeFullNameEn
        {
            get
            {
                if (HttpContext.User == null)
                    return null;
                var claim = HttpContext.User.FindFirst(Encrypt("UserFullNameEn"));
                if (claim == null)
                    return null;
                return Decrypt(claim.Value).ToString();
            }
        }

        public string OrganizationNameAr
        {
            get
            {
                if (HttpContext.User == null)
                    return null;
                var claim = HttpContext.User.FindFirst(Encrypt("CurrentOrganizationNameAr"));
                if (claim == null)
                    return null;
                return Decrypt(claim.Value).ToString();
            }
        }

        public string OrganizationNameEn
        {
            get
            {
                if (HttpContext.User == null)
                    return null;
                var claim = HttpContext.User.FindFirst(Encrypt("CurrentOrganizationNameEn"));
                if (claim == null)
                    return null;
                return Decrypt(claim.Value).ToString();
            }
        }

        public string RoleNameAr
        {
            get
            {
                if (HttpContext.User == null)
                    return null;
                var claim = HttpContext.User.FindFirst(Encrypt("CurrentRoleNameAr"));
                if (claim == null)
                    return null;
                return Decrypt(claim.Value).ToString();
            }
        }

        public string RoleNameEn
        {
            get
            {
                if (HttpContext.User == null)
                    return null;
                var claim = HttpContext.User.FindFirst(Encrypt("CurrentRoleNameEn"));
                if (claim == null)
                    return null;
                return Decrypt(claim.Value).ToString();
            }
        }
        public string IsEmployee
        {
            get
            {
                if (HttpContext.User == null)
                    return null;
                var claim = HttpContext.User.FindFirst(Encrypt("CurrentRoleIsEmployee"));
                if (claim == null)
                    return null;
                return Decrypt(claim.Value).ToString();
            }
        }
        public string ClientIP
        {
            get
            {
                if (HttpContext.User == null || HttpContext.User.Identity == null)
                    return null;
                return HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            }
        }
        public string FaxUserId
        {
            get
            {
                if (HttpContext.User == null || HttpContext.User.Identity == null)
                    return null;
                var claim = HttpContext.User.FindFirst(Encrypt("FaxUserId"));
                if (claim == null)
                    return null;
                return Decrypt(claim.Value).ToString();
            }
        }

        #region Private Methods

        private T GetClaim<T>(string key, T defaultValue = default(T))
        {
            T result = defaultValue;
            var value = HttpContext.User.HasClaim(x => x.Type == key) ? HttpContext.User.FindFirst(key).Value : null;
            if (!string.IsNullOrEmpty(value))
            {
                try
                {
                    Type t = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
                    result = (T)Convert.ChangeType(value, t);
                }
                catch
                {
                    result = defaultValue;
                }
            }
            return result;
        }

        private string SharedEncryptionKey { get; set; }

        private string EncryptionKey
        {
            get
            {
                var result = SharedEncryptionKey ?? GetCookie<string>("_k", null, false);
                if (result == null)
                {
                    result = Guid.NewGuid().ToString();
                    SharedEncryptionKey = result;
                    SetCookie("_k", result, false);
                }
                return result;
            }
        }

        private void SetCookie<T>(string key, T value, bool encrypt = true)
        {
            var str = Convert.ToString(value);
            if (encrypt)
            {
                str = _EncryptionServices.EncryptString(str, EncryptionKey, key);
            }
            // security fix
            var cookieOptions = new CookieOptions
            {
                Secure = true,
                HttpOnly = true,
            };

            _HttpContextAccessor.HttpContext.Response.Cookies.Append(key, str, cookieOptions);
        }

        private T GetCookie<T>(string key, T defaultValue = default(T), bool decrypt = true)
        {
            T result = defaultValue;
            var value = _HttpContextAccessor.HttpContext.Request.Cookies[key];
            if (!string.IsNullOrEmpty(value))
            {
                try
                {
                    if (decrypt)
                    {
                        value = _EncryptionServices.DecryptString(value, EncryptionKey, key);
                    }
                    Type t = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
                    result = (T)Convert.ChangeType(value, t);
                }
                catch
                {
                    result = defaultValue;
                }
            }
            return result;
        }

        #endregion Private Methods


        public void ClearSessionsExcept(params string[] keys)
        {
            //store
            string[] values = new string[keys.Length];
            for (int i = 0; i < keys.Length; i++)
            {
                PropertyInfo propInfo = typeof(SessionServices).GetProperty(keys[i]);
                values[i] = propInfo.GetValue(this, null).ToString();
            }
            //clear
            this.HttpContext.Session.Clear();
            //reset
            for (int i = 0; i < keys.Length; i++)
            {
                PropertyInfo propInfo = typeof(SessionServices).GetProperty(keys[i]);
                propInfo.SetValue(this, values[i]);
            }
        }
    }
}
