using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace ezCV.Application.Common.Extensions
{
    public class HashingTokenExtension
    {
        public static string HashToken(string sessionId)
        {
            // Use a consistent salt for token validation
            // This makes the hash function deterministic
            byte[] salt = new byte[16] {
        0x54, 0x72, 0x61, 0x56, 0x69, 0x6E, 0x68, 0x4D,
        0x61, 0x70, 0x73, 0x53, 0x61, 0x6C, 0x74, 0x21
    };

            // Use a strong hashing algorithm like PBKDF2
            using var pbkdf2 = new Rfc2898DeriveBytes(sessionId, salt, 10000, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(32);

            // Return just the hash
            return Convert.ToBase64String(hash);
        }
    }
}