namespace SecureDocumentAPI.Services;

public interface IEncryptionService
{
    Task<byte[]> EncryptDataAsync(byte[] data, string key);
    Task<byte[]> DecryptDataAsync(byte[] encryptedData, string key);
    Task<string> GenerateEncryptionKeyAsync();
    Task<string> EncryptKeyAsync(string key, string masterKey);
    Task<string> DecryptKeyAsync(string encryptedKey, string masterKey);
    Task<bool> ValidateEncryptionKeyAsync(string key);
    Task<byte[]> GenerateRandomBytesAsync(int length);
    Task<string> HashDataAsync(byte[] data);
    Task<bool> VerifyHashAsync(byte[] data, string hash);
}
