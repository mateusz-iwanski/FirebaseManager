using Google.Cloud.Firestore;
using System;
using System.Reflection;
using System.Threading.Tasks;
using NLog;

namespace FirebaseManager.Firestore
{
    public class FirestoreService : IFirestoreService
    {
        private readonly IFirestoreConnector _firestoreConnector;
        private readonly FirestoreDb _firestoreDb;
        private readonly ILogger _logger;

        public FirestoreService(IFirestoreConnector firestoreConnector, ILogger logger)
        {
            _firestoreConnector = firestoreConnector;
            _logger = logger;

            _firestoreConnector.Connect();
            _firestoreDb = firestoreConnector.GetFirestoreDb();
        }

        /// <summary>
        /// Insert the collection (IFirestoreDto.CollectionName) if it does not exist 
        /// with the document (the rest of the fields are marked as FirestoreProperty) if it does not exist
        /// with the document ID (IFirestoreDto.DocumentUniqueField).
        /// </summary>
        /// <param name="dto"></param>
        /// <returns>true if created, false if exists</returns>
        public async Task<bool> InsertDto(IFirestoreDto dto)
        {
            CollectionReference collection = _firestoreDb.Collection(dto.CollectionName);
            DocumentReference document = collection.Document(dto.DocumentUniqueField);

            if (!await IsDtoExists(document))
            {
                await collection.Document(dto.DocumentUniqueField).SetAsync(dto);
                _logger.Info($"Firestore document '{dto.DocumentUniqueField}' in collection '{dto.CollectionName}' created.");
                return true;
            }

            _logger.Info($"Firestore document '{dto.DocumentUniqueField}' in collection '{dto.CollectionName}' already exists.");

            return false;
        }

        /// <summary>
        /// Update document (IFirestoreDto.DocumentUniqueField) in collection (IFirestoreDto.CollectionName) with the DTO object.
        /// </summary>
        /// <param name="dto"></param>
        /// <returns>true if updated, false if the document does not exist</returns>
        public async Task<bool> UpdateDto(IFirestoreDto dto)
        {
            CollectionReference collection = _firestoreDb.Collection(dto.CollectionName);
            DocumentReference document = collection.Document(dto.DocumentUniqueField);

            if (await IsDtoExists(document))
            {
                await collection.Document(dto.DocumentUniqueField).SetAsync(dto);
                _logger.Info($"Firestore document '{dto.DocumentUniqueField}' in collection '{dto.CollectionName}' updated.");
                return true;
            }

            _logger.Info($"Firestore document '{dto.DocumentUniqueField}' in collection '{dto.CollectionName}' does not exist.");

            return false;
        }

        /// <summary>
        /// Delete document (IFirestoreDto.DocumentUniqueField) in collection (IFirestoreDto.CollectionName).
        /// </summary>
        /// <param name="dto"></param>
        /// <returns>true if deleted, false if the document soes not exist</returns>
        public async Task<bool> DeleteDto(IFirestoreDto dto)
        {
            CollectionReference collection = _firestoreDb.Collection(dto.CollectionName);
            DocumentReference document = collection.Document(dto.DocumentUniqueField);

            if (await IsDtoExists(document))
            {
                await collection.Document(dto.DocumentUniqueField).DeleteAsync();
                _logger.Info($"Firestore document '{dto.DocumentUniqueField}' in collection '{dto.CollectionName}' deleted.");
                return true;
            }

            _logger.Info($"Firestore document '{dto.DocumentUniqueField}' in collection '{dto.CollectionName}' does not exist.");

            return false;
        }

        /// <summary>
        /// Read document (IFirestoreDto.DocumentUniqueField) in collection (IFirestoreDto.CollectionName) and convert it to DTO object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collectionName"></param>
        /// <param name="documentUniqueField"></param>
        /// <returns>IFirestoreDto or null if </returns>
        public async Task<T> ReadDocumentAsync<T>(string collectionName, string documentUniqueField) where T : IFirestoreDto
        {
            CollectionReference collection = _firestoreDb.Collection(collectionName);
            DocumentReference document = collection.Document(documentUniqueField);

            if (await IsDtoExists(document))
            {
                DocumentSnapshot snapshot = await document.GetSnapshotAsync();
                T dto = snapshot.ConvertTo<T>();
                return dto;
            }

            return default;
        }

        /// <summary>
        /// Check if DTO object as document exists in firebase.
        /// 
        /// Checking the DTO document ID (IFirestoreDto.DocumentUniqueField) exists in the collection (IFirestoreDto.CollectionName)
        /// </summary>
        /// <param name="document">CollectionReference->Document(dto.DocumentUniqueField)</param>
        /// <returns></returns>
        private async Task<bool> IsDtoExists(DocumentReference document)
        {
            DocumentSnapshot snapshot = await document.GetSnapshotAsync();
            return snapshot.Exists;
        }
    }


}
