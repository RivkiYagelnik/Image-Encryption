using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public static class KeyManager
{
    private static readonly string KeyFilePath = "key.txt";
    private static readonly string IVFilePath = "iv.txt";

    public static void GenerateAndSaveKeys()
    {
        using (Aes aesAlg = Aes.Create())
        {
            File.WriteAllText(KeyFilePath, Convert.ToBase64String(aesAlg.Key));
            File.WriteAllText(IVFilePath, Convert.ToBase64String(aesAlg.IV));
        }
    }

    public static byte[] GetKey()
    {
        return Convert.FromBase64String(File.ReadAllText(KeyFilePath));
    }

    public static byte[] GetIV()
    {
        return Convert.FromBase64String(File.ReadAllText(IVFilePath));
    }
}
