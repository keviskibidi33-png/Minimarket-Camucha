namespace Minimarket.Domain.Interfaces;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(Stream fileStream, string fileName, string folder);
    Task<bool> DeleteFileAsync(string filePath);
    string GetFileUrl(string filePath);
}

