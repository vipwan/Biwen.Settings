namespace Biwen.Settings.Encryption
{
    /// <summary>
    /// 提供最简单的混淆
    /// </summary>
    public class PlainEncryptionProvider : IEncryptionProvider
    {
        public string Key => "^1234567890qwertyuiop~!@#$%^";

        public virtual string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText)) return cipherText;
            return cipherText.Replace(Key, "");
        }

        public virtual string Encrypt(string plainText)
        {
            return $"{Key}{plainText}{Key}";
        }
    }


    /// <summary>
    /// 空的加密提供者,不做任何加密
    /// </summary>
    public sealed class EmptyEncryptionProvider : IEncryptionProvider
    {
        public string Key => "";

        public string Decrypt(string cipherText)
        {
            return cipherText;
        }

        public string Encrypt(string plainText)
        {
            return plainText;
        }
    }
}