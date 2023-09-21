using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biwen.Settings.Encryption
{
    /// <summary>
    /// 简单的明文混淆加密解密
    /// </summary>
    public sealed class PlainEncryptionProvider : IEncryptionProvider
    {
        public string Key => "^1234567890qwertyuiop~!@#$%^";

        public string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText)) return cipherText;
            return cipherText.Replace(Key, "");
        }

        public string Encrypt(string plainText)
        {
            return $"{Key}{plainText}{Key}";
        }
    }
}