using Google.Cloud.Firestore;
using System.Threading.Tasks;

namespace FirebaseManager.Firestore
{
    public interface IFirestoreService
    {
        Task<bool> InsertDtoAsync(IFirestoreDto dto);
        Task<bool> UpdateDtoAsync(IFirestoreDto dto);
        Task<bool> DeleteDtoAsync(IFirestoreDto dto);
        Task<bool> DeleteDtoAsync(DocumentReference docRef);
        Task<T> ReadDocumentAsync<T>(string collectionName, string documentUniqueField) where T : IFirestoreDto;
        Task<bool> InsertDtoWithSubDtoAsync(IFirestoreDto dto, IFirestoreDto subCollectionDto);
        Task<bool> IsDtoExistsAsync(DocumentReference document);
        FirestoreDb GetFirestoreDb();
    }
}