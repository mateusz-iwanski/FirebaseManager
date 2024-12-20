﻿using Google.Cloud.Firestore;
using System;
using System.Reflection;
using System.Threading.Tasks;
using NLog;
using Google.Cloud.Firestore.V1;
using FirebaseAdmin;
using FirebaseManager.Exceptions;
using System.Collections.Concurrent;
using System.Reflection.Metadata;

namespace FirebaseManager.Firestore
{
    public class FirestoreService : IFirestoreService
    {
        private readonly IFirestoreConnector _firestoreConnector;
        private readonly FirestoreDb _firestoreDb;
        private readonly ILogger _logger;
        private readonly ConcurrentDictionary<string, DocumentSnapshot> _cache;

        public FirestoreService(IFirestoreConnector firestoreConnector, ILogger logger)
        {
            _firestoreConnector = firestoreConnector;
            _logger = logger;
            _cache = new ConcurrentDictionary<string, DocumentSnapshot>();

            _firestoreConnector.Connect();
            _firestoreDb = firestoreConnector.GetFirestoreDb();
        }

        public FirestoreDb GetFirestoreDb() => _firestoreDb;

        /// <summary>
        /// Insert the collection (IFirestoreDto.CollectionName) if it does not exist 
        /// with the document ID (IFirestoreDto.DocumentUniqueField).
        /// If document IFirestoreDto.DocumentUniqueField is null, the document will have random id.
        /// </summary>
        /// <param name="dto"></param>
        /// <returns>true if created, false if exists</returns>        
        public async Task<bool> InsertDtoAsync(IFirestoreDto dto)
        {            

            if (dto.DocumentUniqueField != null)
            {
                DocumentReference document = _firestoreDb.Collection(dto.CollectionName).Document(dto.DocumentUniqueField);

                if (!await IsDtoExistsAsync(document))
                {
                    await document.SetAsync(dto);
                    _cache[document.Path] = await document.GetSnapshotAsync();
                    _logger.Info($"Firestore document '{document.Id}' in collection '{document.Parent.Id}' created.");
                    return true;
                }
                else
                {
                    _logger.Info($"Firestore document '{document.Id}' in collection '{document.Parent.Id}' already exists.");
                    return false;
                }
            }
            else
            {
                CollectionReference collection = _firestoreDb.Collection(dto.CollectionName);
                DocumentReference documentRandomId = await collection.AddAsync(dto);
                _cache[documentRandomId.Path] = await documentRandomId.GetSnapshotAsync();
                _logger.Info($"Firestore document '{documentRandomId.Id}' in collection '{documentRandomId.Parent.Id}' created with random ID.");
                return true;
            }
        }

        /// <summary>
        /// Update document (IFirestoreDto.DocumentUniqueField) in collection (IFirestoreDto.CollectionName) with the DTO object.
        /// </summary>
        /// <param name="dto"></param>
        /// <returns>true if updated, false if the document does not exist</returns>        
        public async Task<bool> UpdateDtoAsync(IFirestoreDto dto)
        {
            DocumentReference document = _firestoreDb.Collection(dto.CollectionName).Document(dto.DocumentUniqueField);

            if (await IsDtoExistsAsync(document))
            {
                await document.SetAsync(dto);
                _cache[document.Path] = await document.GetSnapshotAsync();
                _logger.Info($"Firestore document '{document.Id}' in collection '{document.Parent.Id}' updated.");
                return true;
            }

            _logger.Info($"Can't update. Firestore document '{document.Id}' in collection '{document.Parent.Id}' does not exist.");
            return false;
        }

        /// <summary>
        /// Delete document in collection by document reference
        /// </summary>
        /// <param name="dto"></param>
        /// <returns>true if deleted, false if the document soes not exist</returns>
        public async Task<bool> DeleteDtoAsync(DocumentReference docRef)
        {
            if (await IsDtoExistsAsync(docRef))
            {
                await docRef.DeleteAsync();
                _cache.TryRemove(docRef.Path, out _);
                _logger.Info($"Firestore document '{docRef.Id}' in collection '{docRef.Parent.Id}' deleted.");
                return true;
            }

            _logger.Info($"Can't delete. Firestore document '{docRef.Id}' in collection '{docRef.Parent.Id}' does not exist.");
            return false;
        }

        /// <summary>
        /// Delete document (IFirestoreDto.DocumentUniqueField) in collection (IFirestoreDto.CollectionName).
        /// </summary>
        /// <param name="dto"></param>
        /// <returns>true if deleted, false if the document soes not exist</returns>
        public async Task<bool> DeleteDtoAsync(IFirestoreDto dto)
        {
            CollectionReference collection = _firestoreDb.Collection(dto.CollectionName);
            DocumentReference document = collection.Document(dto.DocumentUniqueField);

            if (await IsDtoExistsAsync(document))
            {
                await collection.Document(dto.DocumentUniqueField).DeleteAsync();
                _cache.TryRemove(document.Path, out _);
                _logger.Info($"Firestore document '{dto.DocumentUniqueField}' in collection '{dto.CollectionName}' deleted.");
                return true;
            }

            _logger.Info($"Firestore document '{dto.DocumentUniqueField}' in collection '{dto.CollectionName}' does not exist.");
            return false;
        }

        /// <summary>
        /// Add to document sub-collection (subCollectionDto.CollectionName) with the DTO object.
        /// If subCollectionDto.DocumentUniqueField null, the document will have random id, 
        /// otherwise, it will be the same as subCollectionDto.DocumentUniqueField.
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="subCollectionDto"></param>
        /// <returns></returns>
        public async Task<bool> InsertDtoWithSubDtoAsync(IFirestoreDto dto, IFirestoreDto subCollectionDto)
        {
            DocumentReference docRef = _firestoreDb.Collection(dto.CollectionName).Document(dto.DocumentUniqueField);

            if (!await IsDtoExistsAsync(docRef)) 
                throw new FirestorageException($"Firestore DocumentNotFound - ID {dto.DocumentUniqueField ?? "(not declare)"} from collection {dto.CollectionName}");

            if (subCollectionDto.DocumentUniqueField != null)
            {
                // Reference to the document in sub-collection
                DocumentReference docInsubCollection = docRef.Collection(subCollectionDto.CollectionName).Document(subCollectionDto.DocumentUniqueField);
                
                // Add the document to the sub-collection
                await docInsubCollection.SetAsync(subCollectionDto);
                _cache[docRef.Path] = await docRef.GetSnapshotAsync();

                _logger.Info($"Firestore document '{docInsubCollection.Id}' in sub-collection '{docInsubCollection.Parent.Id}' created.");
            }
            else
            {                
                // Reference to the sub-collection
                CollectionReference subCollection = docRef.Collection(subCollectionDto.CollectionName);

                // Add the document to the sub-collection
                DocumentReference subDocRef = await subCollection.AddAsync(subCollectionDto);
                _cache[docRef.Path] = await docRef.GetSnapshotAsync();
            }

            return true;
        }

        /// <summary>
        /// Read document (IFirestoreDto.DocumentUniqueField) in collection (IFirestoreDto.CollectionName) and convert it to DTO object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collectionName"></param>
        /// <param name="documentUniqueField"></param>
        /// <returns>IFirestoreDto or null if document not exists</returns>
        public async Task<T> ReadDocumentAsync<T>(string collectionName, string documentUniqueField) where T : IFirestoreDto
        {
            CollectionReference collection = _firestoreDb.Collection(collectionName);
            DocumentReference document = collection.Document(documentUniqueField);

            if (await IsDtoExistsAsync(document))
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
        //public async Task<bool> IsDtoExistsAsync(DocumentReference document)
        //{
        //    DocumentSnapshot snapshot = await document.GetSnapshotAsync();
        //    return snapshot.Exists;
        //}

        public async Task<bool> IsDtoExistsAsync(DocumentReference document)
        {
            if (_cache.TryGetValue(document.Path, out var cachedSnapshot))
            {
                return cachedSnapshot.Exists;
            }

            DocumentSnapshot snapshot = await document.GetSnapshotAsync();
            _cache[document.Path] = snapshot;
            return snapshot.Exists;
        }


    }


}
