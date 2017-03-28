using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace LogicReinc.Archive.Components
{
    public static class Hashing
    {
        public static string[] HashTags(string[] tags)
            => tags.Select(x => HashTag(x)).ToArray();

        public static string HashTag(string str)
        {
            return Hash("Tag" + str + "LRArchive", HashType.Sha1);
        }


        public static byte[] Hash(byte[] bytes, HashType type)
        {
            switch (type)
            {
                case HashType.MD5:
                    MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
                    bytes = md5.ComputeHash(bytes);
                    break;
                case HashType.Sha1:
                    SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider();
                    bytes = sha1.ComputeHash(bytes);
                    break;
                case HashType.Sha256:
                    SHA256CryptoServiceProvider sha256 = new SHA256CryptoServiceProvider();
                    bytes = sha256.ComputeHash(bytes);
                    break;
                case HashType.Sha512:
                    SHA512CryptoServiceProvider sha512 = new SHA512CryptoServiceProvider();
                    bytes = sha512.ComputeHash(bytes);
                    break;
            }

            return bytes;
        }
        public static string Hash(string toHash, HashType type)
        {
            return Convert.ToBase64String(Hash(Encoding.UTF8.GetBytes(toHash), type));
        }

        public enum HashType
        {
            MD5,
            Sha1,
            Sha256,
            Sha512
        }
    }
}
