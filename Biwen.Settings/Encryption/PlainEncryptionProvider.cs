// Licensed to the Biwen.Settings under one or more agreements.
// The Biwen.Settings licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.Settings Author: 万雅虎, Github: https://github.com/vipwan
// Biwen.Settings ,NET8+ 应用配置项管理模块
// Modify Date: 2024-09-18 17:27:25 PlainEncryptionProvider.cs

namespace Biwen.Settings.Encryption;

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