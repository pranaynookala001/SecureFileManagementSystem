using SecureDocumentAPI.DTOs;

namespace SecureDocumentAPI.Services;

public interface IFileProcessingService
{
    Task<bool> ValidateFileAsync(byte[] fileData, string fileName, string contentType);
    Task<string> ExtractTextFromDocumentAsync(byte[] fileData, string fileType);
    Task<byte[]> ConvertDocumentFormatAsync(byte[] fileData, string fromFormat, string toFormat);
    Task<DocumentMetadataDto> ExtractMetadataAsync(byte[] fileData, string fileType);
    Task<bool> ScanForMalwareAsync(byte[] fileData);
    Task<bool> ScanForSensitiveDataAsync(byte[] fileData);
    Task<byte[]> CompressFileAsync(byte[] fileData);
    Task<byte[]> DecompressFileAsync(byte[] compressedData);
    Task<string> GenerateThumbnailAsync(byte[] fileData, string fileType);
    Task<bool> ValidateFileIntegrityAsync(byte[] fileData, string expectedHash);
}
