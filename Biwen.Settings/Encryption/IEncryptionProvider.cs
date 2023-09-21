using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biwen.Settings.Encryption
{
    public interface IEncryptionProvider
    {
        /// <summary>
        /// 秘钥
        /// </summary>
        string Key { get; }

        /// <summary>
        /// 加密,注意加密如果失败,请返回原文
        /// </summary>
        /// <param name="plainText"></param>
        /// <returns></returns>
        string Encrypt(string plainText);
        /// <summary>
        /// 解密,注意解密如果失败,请返回原文
        /// </summary>
        /// <param name="cipherText"></param>
        /// <returns></returns>
        string Decrypt(string cipherText);
    }
}
