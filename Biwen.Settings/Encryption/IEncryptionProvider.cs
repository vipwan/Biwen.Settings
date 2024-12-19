// Licensed to the Biwen.Settings under one or more agreements.
// The Biwen.Settings licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.Settings Author: 万雅虎, Github: https://github.com/vipwan
// Biwen.Settings ,NET8+ 应用配置项管理模块
// Modify Date: 2024-09-18 17:27:19 IEncryptionProvider.cs

namespace Biwen.Settings.Encryption;

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
