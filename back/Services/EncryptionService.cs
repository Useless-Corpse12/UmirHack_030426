using System.Security.Cryptography;
using System.Text;
using DeliveryAggregator.Services.Interfaces;

namespace DeliveryAggregator.Services;

public class EncryptionService : IEncryptionService
{
    private readonly byte[] _key;
    private readonly byte[] _iv;

    public EncryptionService(IConfiguration config)
    {
        var keyStr = config["Encryption:Key"]
            ?? throw new InvalidOperationException("Encryption:Key не задан в appsettings.json");
        var ivStr = config["Encryption:IV"]
            ?? throw new InvalidOperationException("Encryption:IV не задан в appsettings.json");

        // Key должен быть 32 байта (AES-256), IV — 16 байт
        _key = Encoding.UTF8.GetBytes(keyStr.PadRight(32).Substring(0, 32));
        _iv  = Encoding.UTF8.GetBytes(ivStr.PadRight(16).Substring(0, 16));
    }

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText)) return plainText;

        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV  = _iv;

        using var encryptor = aes.CreateEncryptor();
        var bytes = Encoding.UTF8.GetBytes(plainText);
        var encrypted = encryptor.TransformFinalBlock(bytes, 0, bytes.Length);
        return Convert.ToBase64String(encrypted);
    }

    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText)) return cipherText;

        try
        {
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV  = _iv;

            using var decryptor = aes.CreateDecryptor();
            var bytes = Convert.FromBase64String(cipherText);
            var decrypted = decryptor.TransformFinalBlock(bytes, 0, bytes.Length);
            return Encoding.UTF8.GetString(decrypted);
        }
        catch
        {
            // Если данные не зашифрованы (старые записи) — вернуть как есть
            return cipherText;
        }
    }
}
