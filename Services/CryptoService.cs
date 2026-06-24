using System.Security.Cryptography;
using System.Text;

namespace RentalApp.Services;

public class CryptoService
{
    private readonly string key = "12345678901234567890123456789012"; // 32 chars

    public byte[] Encrypt(byte[] data)
    {
        using var aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(key);
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream();

        ms.Write(aes.IV, 0, aes.IV.Length);

        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        {
            cs.Write(data, 0, data.Length);
        }

        return ms.ToArray();
    }

    public byte[] Decrypt(byte[] data)
    {
        using var aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(key);

        var iv = new byte[16];
        Array.Copy(data, iv, 16);
        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

        using var ms = new MemoryStream(data, 16, data.Length - 16);
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var result = new MemoryStream();

        cs.CopyTo(result);
        return result.ToArray();
    }
}