
namespace FirebaseManager.Storage
{
    public interface IFirestorageService
    {
        Task UploadFileAsync(string localFilePath, string objectName);
        Task DeleteFileAsync(string objectName);
        Task<bool> CheckFileExistsAsync(string firestoreageUri);
        Task CreateDirectoryAsync(string directoryPath);
        Task<bool> CheckDirectoryExistsAsync(string directoryPath);
    }
}