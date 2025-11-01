using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ezCV.Application.Common.Extensions
{
    public class HashingExtension
    {
        public static string HashWithSHA256(string plainOtp)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // Convert the input string to a byte array and compute the hash.
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(plainOtp));

                // Convert byte array to a string (hexadecimal representation)
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2")); // "x2" for lowercase hex
                }
                return builder.ToString();
            }
        }
    }
}