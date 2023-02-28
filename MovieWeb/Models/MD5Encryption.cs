using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace MovieWeb.Models
{
    public class MD5Encryption
    {
        public static String GetMD5(String password)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] mahoamd5;
            UTF8Encoding encode = new UTF8Encoding();
            mahoamd5 = md5.ComputeHash(encode.GetBytes(password));
            StringBuilder data = new StringBuilder();
            for (int i = 0; i < mahoamd5.Length; i++)
            {
                data.Append(mahoamd5[i].ToString("x2"));
            }
            return data.ToString();
        }

    }
}