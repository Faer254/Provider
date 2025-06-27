using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Provider.classes
{
    class Encrypter
    {
        private static readonly byte[] keyForList = Encoding.UTF8.GetBytes("68458967790326709471629129574358");
        private static readonly byte[] ivForList = Encoding.UTF8.GetBytes("9820435908626345");

        private static readonly byte[] keyForString = Encoding.UTF8.GetBytes("48123690571245963218754039682517");
        private static readonly byte[] ivForString = Encoding.UTF8.GetBytes("7195036284917356");

        public static byte[] EncryptListToBytes_AES(List<string> list)
        {
            using (MemoryStream ms = new MemoryStream())
            using (CryptoStream cs = new CryptoStream(ms, GetAesAlgorithm(keyForList, ivForList).CreateEncryptor(), CryptoStreamMode.Write))
            using (StreamWriter sw = new StreamWriter(cs))
            {
                foreach (string item in list)
                {
                    sw.WriteLine(item);
                }
                sw.Close();
                return ms.ToArray();
            }
        }

        public static List<string>? DecryptListFromBytes_AES(byte[] cipherText)
        {
            try
            {
                List<string> decryptedList = new List<string>();
                using (MemoryStream ms = new MemoryStream(cipherText))
                using (CryptoStream cs = new CryptoStream(ms, GetAesAlgorithm(keyForList, ivForList).CreateDecryptor(), CryptoStreamMode.Read))
                using (StreamReader sr = new StreamReader(cs))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        decryptedList.Add(line);
                    }
                    return decryptedList;
                }
            }
            catch
            {
                return null;
            }
        }

        public static byte[] EncryptStringToBytes_AES(string plainText)
        {
            using (MemoryStream ms = new MemoryStream())
            using (CryptoStream cs = new CryptoStream(ms, GetAesAlgorithm(keyForString, ivForString).CreateEncryptor(), CryptoStreamMode.Write))
            using (StreamWriter sw = new StreamWriter(cs))
            {
                sw.Write(plainText);
                sw.Close();
                return ms.ToArray();
            }
        }

        public static string? DecryptStringFromBytes_AES(byte[] cipherText)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream(cipherText))
                using (CryptoStream cs = new CryptoStream(ms, GetAesAlgorithm(keyForString, ivForString).CreateDecryptor(), CryptoStreamMode.Read))
                using (StreamReader sr = new StreamReader(cs))
                {
                    return sr.ReadToEnd();
                }
            }
            catch
            {
                return null;
            }
        }

        private static Aes GetAesAlgorithm(byte[] Key, byte[] IV)
        {
            Aes aesAlg = Aes.Create();
            aesAlg.Key = Key;
            aesAlg.IV = IV;
            return aesAlg;
        }
    }
}
