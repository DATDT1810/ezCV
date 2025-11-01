using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualBasic;

namespace ezCV.Application.Common.Extensions
{
    public class GenarateOtpExtension
    {
        public static string GenarateOtp()
        {
            var random = new Random();
            var otp = random.Next(100000, 999999).ToString(CultureInfo.InvariantCulture);
            return otp;
        }
    }
}