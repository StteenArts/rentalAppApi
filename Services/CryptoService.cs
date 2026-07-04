using System.Security.Cryptography;
using System.Text;

namespace RentalApp.Services;

public class CryptoService
{
    private readonly byte[] key;

    public CryptoService(IConfiguration configuration)
    {
        var aesKey = configuration["Crypto:AesKey"];

        if (string.IsNullOrEmpty(aesKey) || Encoding.UTF8.GetByteCount(aesKey) != 32)
            throw new InvalidOperationException(
                "Crypto:AesKey must be configured with a 32-byte value (dotnet user-secrets in dev, env var in containers).");

        key = Encoding.UTF8.GetBytes(aesKey);
    }

    public byte[] Encrypt(byte[] data)
    {
        using var aes = Aes.Create();
        aes.Key = key;
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
        aes.Key = key;

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
