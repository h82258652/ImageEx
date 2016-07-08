using System;
using System.Security.Cryptography;
using System.Text;

namespace Controls.Utils
{
    public static class HashHelper
    {
        public static string GenerateMd5Hash(string input, string prefix = "", string suffix = "")
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            using (var md5 = new MD5CryptoServiceProvider())
            {
                var bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(prefix + input + suffix));
                var buffer = new StringBuilder();
                foreach (var b in bytes)
                {
                    buffer.Append(b.ToString("x2"));
                }
                return buffer.ToString();
            }
        }
    }
}