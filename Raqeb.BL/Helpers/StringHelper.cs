using Microsoft.AspNetCore.DataProtection;

namespace Raqeb.BL.Helpers
{
    public static class PasswordHelper
    {
        private static readonly string encryptionKey = "C@ns@leaAppM@ia$123#";
        private static readonly IDataProtectionProvider dataProtectionProvider = DataProtectionProvider.Create("TafeelMoiav1");
        private static readonly IDataProtector protector = dataProtectionProvider.CreateProtector(encryptionKey);

        public static string EncryptPassword(this string password) => protector.Protect(password);

        public static string DecryptPassword(this string encryptedPassword) => protector.Unprotect(encryptedPassword);
    }
}
