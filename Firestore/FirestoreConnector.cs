using Google.Cloud.Firestore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using static Google.Cloud.Firestore.V1.StructuredQuery.Types;
using FirebaseManager.Exceptions;
using FirebaseManager.Firebase;

namespace FirebaseManager.Firestore
{
    /// <summary>
    /// FirestoreConnector is used to connect to Firestore database.
    /// </summary>
    public class FirestoreConnector : IFirestoreConnector
    {
        private readonly IOptions<FirebaseSettings> _settings;
        private FirestoreDb _db;
        private readonly ILogger<FirestoreConnector> _logger;

        public FirestoreConnector(IOptions<FirebaseSettings> settings, ILogger<FirestoreConnector> logger)
        {
            _settings = settings;
            _logger = logger;
        }

        public void Connect()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + _settings.Value.ApiKeyFilePath;
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", path);

            _db = FirestoreDb.Create(_settings.Value.ProjectId);
            _logger.LogInformation($"FirestoreDb connected to project {_settings.Value.ProjectId}");
        }

        public FirestoreDb GetFirestoreDb()
        {
            if (_db == null)
                throw new FirestoreException("FirestoreDb is not initialized. Call Connect() method first.");

            return _db;
        }


    }
}
