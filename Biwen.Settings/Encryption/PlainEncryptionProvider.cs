namespace Biwen.Settings.Encryption
{
    /// <summary>
    /// 提供最简单的混淆
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