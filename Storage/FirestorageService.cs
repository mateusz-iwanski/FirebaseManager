using FirebaseManager.Firebase;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using FirebaseAdmin;
using FirebaseManager.Exceptions;
using Google.Rpc;

namespace FirebaseManager.Storage
{
    /// <summary>
    /// Service to manage Firebase Storage
    /// </summary>
    public class FirestorageService : IFirestorageService
    {
        private readonly IFirestorageConnector _firestorageConnector;
        private readonly IOptions<FirebaseSettings> _settings;
        private readonly ILogger _logger;
        private StorageClient _storageClient;

        public FirestorageService(IFirestorageConnector firestorageConnector, IOptions<FirebaseSettings> settings, ILogger logger)
        {
            _firestorageConnector = firestorageConnector;
            _settings = settings;
            _logger = logger;

            _firestorageConnector.Initialize();
            _storageClient = StorageClient.Create(GoogleCredential.FromFile(_settings.Value.ApiKeyFilePath));
        }

        /// <summary>
        /// Upload file to Firebase Storage
        /// 
        /// If file exists on firebase storage, it will not be overwritten.
        /// </summary>
        /// <param name="localFilePath">Path to file</param>
        /// <param name="objectName">May contain a directory (directory/filename). If the directory does not exist, it will be created</param>
        /// <exception cref="FirestorageException">When problem with Firebase service</exception>
        /// <exception cref="FileLoadException">When file doesn't exist</exception>
        public async Task UploadFileAsync(string localFilePath, string objectName)
        {

            if (!File.Exists(localFilePath))
            {
                throw new FileLoadException($"File doesn't exist on {localFilePath}.");
            }            

            using (var fileStream = new FileStream(localFilePath, FileMode.Open))
            {
                var bucketName = _settings.Value.StorageBucketName;

                if (!await CheckFileExistsAsync($"{bucketName}/{objectName}"))
                {
                    _logger.Info($"File {localFilePath} already exists in {bucketName}/{objectName}");
                    return;
                }

                try 
                { 
                    await _storageClient.UploadObjectAsync(bucketName, objectName, null, fileStream);
                    _logger.Info($"File {localFilePath} uploaded to {bucketName}/{objectName}");
                }
                catch (Google.GoogleApiException ex)
                {
                    _logger.Error(ex, $"Error uploading file {localFilePath} to {bucketName}/{objectName}. Status code {ex.HttpStatusCode}");
                    throw new FirestorageException($"FirebaseError uploading file {localFilePath} to {bucketName}/{objectName}. Status code { ex.HttpStatusCode }", ex);                    
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, $"Error uploading file {localFilePath} to {bucketName}/{objectName}");
                    throw new FirestorageException($"FirebaseError uploading file {localFilePath} to {bucketName}/{objectName}", ex);
                }
            }
        }

        /// <summary>
        /// Delete file from Firebase Storage
        /// </summary>
        /// <param name="objectName">Can include directory (name-of-direcory/file)</param>
        /// <returns>true if deleted, false if not exists</returns>
        /// <exception cref="FirestorageException">When problem with Firebase service</exception>
        public async Task DeleteFileAsync(string objectName)
        {
            var bucketName = _settings.Value.StorageBucketName;

            if (!await CheckFileExistsAsync(objectName))
                throw new FirestorageException($"FirebaseError - {objectName} doesn't exist on Firestorage service.");
            
            try
            {
                await _storageClient.DeleteObjectAsync(bucketName, objectName);
                _logger.Info($"File {objectName} deleted from {bucketName}");
            }
            catch (Google.GoogleApiException ex)
            {
                _logger.Error(ex, $"FirebaseError deleting file {objectName} from {bucketName}. Status code {ex.HttpStatusCode}");
                throw new FirestorageException($"Error deleting file {objectName} from {bucketName}. Status code {ex.HttpStatusCode}", ex);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"FirebaseError deleting file {objectName} from {bucketName}");
                throw new FirestorageException($"Error deleting file {objectName} from {bucketName}", ex);
            }
        }

        /// <summary>
        /// Check if file exists in Firebase Storage
        /// </summary>
        /// <param name="objectName">Can include directory (name-of-direcory/file)</param>
        /// <returns>true if exists, false if not exists</returns>
        /// <exception cref="FirestorageException">When problem with Firebase service</exception>
        public async Task<bool> CheckFileExistsAsync(string firestoreageUri)
        {
            var bucketName = _settings.Value.StorageBucketName;

            try
            {
                await _storageClient.GetObjectAsync(bucketName, firestoreageUri);
                return true; 
            }
            catch (Google.GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return false;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"FirebaseError checking file {firestoreageUri} in {bucketName}");
                throw new FirestorageException($"Error checking file {firestoreageUri} in {bucketName}", ex);
            }
        }

        // how to add directory on firebase storage
        /// <summary>
        /// Create a directory on Firebase Storage if it does not exist
        /// </summary>
        /// <param name="directoryPath">The directory path to create</param>
        /// <exception cref="FirestorageException">When problem with Firebase service</exception>
        public async Task CreateDirectoryAsync(string directoryPath)
        {
            var bucketName = _settings.Value.StorageBucketName;

            try
            {
                // Check if the directory already exists
                if (await CheckDirectoryExistsAsync(directoryPath))
                {
                    return;
                }

                // Create the directory by uploading an empty object with a trailing slash
                await _storageClient.UploadObjectAsync(bucketName, directoryPath + "/", null, new MemoryStream());

                _logger.Info($"Directory {directoryPath} created in {bucketName}");
            }
            catch (Google.GoogleApiException ex)
            {
                _logger.Error(ex, $"Error creating directory {directoryPath} in {bucketName}. Status code {ex.HttpStatusCode}");
                throw new FirestorageException($"FirebaseError creating directory {directoryPath} in {bucketName}. Status code {ex.HttpStatusCode}", ex);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Error creating directory {directoryPath} in {bucketName}");
                throw new FirestorageException($"FirebaseError creating directory {directoryPath} in {bucketName}", ex);
            }
        }

        /// <summary>
        /// Check if a directory exists in Firebase Storage
        /// </summary>
        /// <param name="directoryPath">The directory path to check</param>
        /// <returns>true if the directory exists, false otherwise</returns>
        /// <exception cref="FirestorageException">When problem with Firebase service</exception>
        public async Task<bool> CheckDirectoryExistsAsync(string directoryPath)
        {
            var bucketName = _settings.Value.StorageBucketName;

            try
            {
                // Check if the directory exists by attempting to get its metadata
                var storageObject = await _storageClient.GetObjectAsync(bucketName, directoryPath + "/");
                return storageObject != null;
            }
            catch (Google.GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return false;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"FirebaseError checking directory {directoryPath} in {bucketName}");
                throw new FirestorageException($"Error checking directory {directoryPath} in {bucketName}", ex);
            }
        }




    }
}
