using FirebaseAdmin;
using FirebaseManager.Firebase;
using FirebaseManager.Firestore;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Threading.Tasks;
using NLog;

namespace FirebaseManager.Storage
{
    /// <summary>
    /// Connector to Firebase Storage
    /// </summary>
    public class FirestorageConnector : IFirestorageConnector
    {
        private readonly IOptions<FirebaseSettings> _settings;
        private readonly ILogger _logger;        

        public FirestorageConnector(IOptions<FirebaseSettings> settings, ILogger logger)
        {
            _settings = settings;
            _logger = logger;
        }

        public void Initialize()
        {
            // Initialize Firebase
            var firebaseOptions = new AppOptions()
            {
                Credential = GoogleCredential.FromFile(_settings.Value.ApiKeyFilePath),
                ProjectId = _settings.Value.ProjectId
            };
            FirebaseApp.Create(firebaseOptions);            
        }
       
    }
}
