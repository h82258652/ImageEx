using System;
using System.Security.Cryptography;
using System.Text;

namespace Controls.Utils
{
    internal static class HashHelper
    {
        internal static string GenerateMD5Hash(string input, string prefix = "", string suffix = "")
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            using (var md5 = MD5.Create())
            {
                var buffer = Encoding.UTF8.GetBytes(prefix + input + suffix);
                var hashResult = md5.ComputeHash(buffer);
                return BitConverter.ToString(hashResult).Replace("-", string.Empty);
            }
        }
    }
}