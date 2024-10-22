
namespace FirebaseManager.Storage
{
    public interface IFirestorageService
    {
        Task UploadFileAsync(string localFilePath, string objectName);
        Task DeleteFileAsync(string objectName);
    }
}