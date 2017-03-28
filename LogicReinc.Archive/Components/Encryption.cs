using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace LogicReinc.Archive.Components
{
    public class Encryption
    {
        private static Rfc2898DeriveBytes DerivePassword(string pass, string salt = "")
        {
            byte[] saltBytes = Encoding.ASCII.GetBytes(salt);
            Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(pass, (!string.IsNullOrEmpty(salt)) ? saltBytes : new byte[8]);
            return key;
        }

        public static bool EncryptStream(Stream str, Stream eStr, string password)
        {
            RijndaelManaged algo = new RijndaelManaged();
            algo.Padding = PaddingMode.PKCS7;
            Rfc2898DeriveBytes key = DerivePassword(password);
            algo.Key = key.GetBytes(algo.KeySize / 8);
            algo.IV = key.GetBytes(algo.BlockSize / 8);
            ICryptoTransform encryptor = algo.CreateEncryptor();

            using (CryptoStream crypto = CreateEncryptStream(eStr, password, "Archive"))
            {
                byte[] buffer = new byte[4096];
                int read;
                while (true)
                {
                    read = str.Read(buffer, 0, buffer.Length);
                    if (read == 0)
                        break;
                    crypto.Write(buffer, 0, read);
                }
                crypto.FlushFinalBlock();
            }

            return true;
        }
        public static bool DecryptStream(Stream str, Stream eStr, string password)
        {
            using (CryptoStream crypto = CreateDecryptStream(eStr, password, "Archive"))
            {
                byte[] buffer = new byte[4096];
                int read;
                while (true)
                {
                    read = str.Read(buffer, 0, buffer.Length);
                    if (read == 0)
                        break;
                    crypto.Write(buffer, 0, read);
                }
                crypto.FlushFinalBlock();
            }

            return true;
        }


        public static CryptoStream CreateEncryptStream(Stream str, string password, string saltStr)
        {
            RijndaelManaged algo = new RijndaelManaged();

            algo.Padding = PaddingMode.PKCS7;
            try
            {
                byte[] salt = Encoding.ASCII.GetBytes(saltStr);

                Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(password, salt);
                algo.Key = key.GetBytes(algo.KeySize / 8);
                algo.IV = key.GetBytes(algo.BlockSize / 8);

                ICryptoTransform encryptor = algo.CreateEncryptor();
                return new CryptoStream(str, encryptor, CryptoStreamMode.Write);
            }
            finally
            {
                if (algo != null)
                    algo.Clear();
            }
        }
        public static CryptoStream CreateDecryptStream(Stream str, string password, string saltStr)
        {
            RijndaelManaged algo = new RijndaelManaged();
            algo.Padding = PaddingMode.PKCS7;
            try
            {
                byte[] salt = Encoding.ASCII.GetBytes(saltStr);

                Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(password, salt);
                algo.Key = key.GetBytes(algo.KeySize / 8);
                algo.IV = key.GetBytes(algo.BlockSize / 8);

                ICryptoTransform decryptor = algo.CreateDecryptor();


                return new CryptoStream(str, decryptor, CryptoStreamMode.Read);

            }
            finally
            {
                if (algo != null)
                    algo.Clear();
            }
        }
    }
}
