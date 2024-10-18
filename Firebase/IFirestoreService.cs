using Google.Cloud.Firestore;
using System.Threading.Tasks;

namespace FirebaseManager.Firebase
{
    public interface IFirestoreService
    {
        Task<bool> InsertDto(IFirestoreDto dto);
        Task<bool> UpdateDto(IFirestoreDto dto);
        Task<bool> DeleteDto(IFirestoreDto dto);
        Task<T> ReadDocumentAsync<T>(string collectionName, string documentUniqueField) where T : IFirestoreDto;
    }
}