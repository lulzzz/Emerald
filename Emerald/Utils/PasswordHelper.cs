using System;
using System.Linq;
using System.Security.Cryptography;

namespace Emerald.Utils
{
    public static class PasswordHelper
    {
        public static string HashPassword(string password)
        {
            if (password == null) throw new ArgumentNullException(nameof(password));

            byte[] salt, bytes;

            using (var rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, 16, 1000))
            {
                salt = rfc2898DeriveBytes.Salt;
                bytes = rfc2898DeriveBytes.GetBytes(32);
            }

            var inArray = new byte[49];

            Buffer.BlockCopy(salt, 0, inArray, 1, 16);
            Buffer.BlockCopy(bytes, 0, inArray, 17, 32);

            return Convert.ToBase64String(inArray);
        }
        public static bool VerifyHashedPassword(string hashedPassword, string password)
        {
            if (hashedPassword == null) throw new ArgumentNullException(nameof(hashedPassword));
            if (password == null) throw new ArgumentNullException(nameof(password));

            var numArray = Convert.FromBase64String(hashedPassword);
            if (numArray.Length != 49 || numArray[0] != 0) return false;

            var salt = new byte[16];
            var a = new byte[32];

            Buffer.BlockCopy(numArray, 1, salt, 0, 16);
            Buffer.BlockCopy(numArray, 17, a, 0, 32);

            byte[] bytes;

            using (var rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, salt, 1000))
            {
                bytes = rfc2898DeriveBytes.GetBytes(32);
            }

            if (a.Length != bytes.Length) return false;

            return !a.Where((t, i) => t != bytes[i]).Any();
        }
    }
}