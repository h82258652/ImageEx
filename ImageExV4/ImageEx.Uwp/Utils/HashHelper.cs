using System;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;

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

            var alg = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Md5);
            var data = CryptographicBuffer.ConvertStringToBinary(prefix + input + suffix, BinaryStringEncoding.Utf8);
            var hash = alg.HashData(data);
            return CryptographicBuffer.EncodeToHexString(hash);
        }
    }
}