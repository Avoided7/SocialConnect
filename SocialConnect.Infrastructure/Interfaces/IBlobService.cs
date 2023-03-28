namespace SocialConnect.Infrastructure.Interfaces;

public interface IBlobService
{
    Task<bool> UploadFileAsync(Stream file, string filename);
    Task<bool> DeleteFileAsync(string filename);
}