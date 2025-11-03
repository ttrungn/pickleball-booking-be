using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PickleBallBooking.Services.Utils;
public class HashHelper
{
    public static String HmacSHA256(string inputData, string key)
    {
        byte[] keyBytes = Encoding.UTF8.GetBytes(key);
        byte[] messageBytes = Encoding.UTF8.GetBytes(inputData);


        using (var hmacSHA256 = new HMACSHA256(keyBytes))
        {
            byte[] hashMessage = hmacSHA256.ComputeHash(messageBytes);
            string hex = BitConverter.ToString(hashMessage).Replace("-", "").ToLower();
            return hex;
        }
    }
}
