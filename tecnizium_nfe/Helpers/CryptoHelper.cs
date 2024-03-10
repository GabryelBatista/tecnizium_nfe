using System.Security.Cryptography;
using System.Text;

namespace tecnizium_nfe.Helpers;

abstract public class CryptoHelper
{
    public static string Encrypt(string text, string key)
    {
        using (RijndaelManaged rijndael = new RijndaelManaged())
        {
            rijndael.Key = Encoding.UTF8.GetBytes(key);

            using (ICryptoTransform encryptor = rijndael.CreateEncryptor())
            {
                byte[] dadosCriptografados =
                    encryptor.TransformFinalBlock(Encoding.UTF8.GetBytes(text), 0, text.Length);
                return Convert.ToBase64String(dadosCriptografados);
            }
        }
    }

    public static string Decrypt(string text, string key)
    {
        using (RijndaelManaged rijndael = new RijndaelManaged())
        {
            rijndael.Key = Encoding.UTF8.GetBytes(key);

            using (ICryptoTransform decryptor = rijndael.CreateDecryptor())
            {
                byte[] dadosCriptografados = Convert.FromBase64String(text);
                return Encoding.UTF8.GetString(decryptor.TransformFinalBlock(dadosCriptografados, 0,
                    dadosCriptografados.Length));
            }
        }
    }
}