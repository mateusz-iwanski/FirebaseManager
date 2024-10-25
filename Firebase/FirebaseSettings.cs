using FirebaseManager.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirebaseManager.Firebase
{
    /// <summary>
    /// The class with the settings for the Firebase service.
    /// 
    /// Used to bind data from the appsettings.json file.
    /// </summary>
    /// <remarks>
    /// In appsettings.json add the following:
    /// 
    /// "Firebase": {
    ///     "Firestore": {
    ///         "ProjectId": "",
    ///         "ApiKeyFilePath": ""
    ///     }
    ///  }
    ///  
    /// Register in DI container options:
    /// services.Configure<FirebaseSettings>(context.Configuration.GetSection("Firebase").GetSection("Firestore"));
    /// 
    /// </remarks>

    public class FirebaseSettings
    {
        private string? _projectId;
        private string? _apiKeyFilePath;
        private string? _storageBucketName;
        private string? _localDirectoryForFileToDownload;
        private string? _firestoreDirectoryForFileToUpload;
        public string? ProjectId
        {
            get { return _projectId ?? throw new SettingsException($"In appsettings.json Firebase->ProjectId not exists"); }
            set => _projectId = value;
        }

        public string? ApiKeyFilePath
        {
            get { return _apiKeyFilePath ?? throw new SettingsException($"In appsettings.json Firebase->ApiKeyFilePath not exists"); }
            set => _apiKeyFilePath = value;
        }

        public string? StorageBucketName
        {
            get { return _storageBucketName ?? throw new SettingsException($"In appsettings.json Firebase->StorageBucketName not exists"); }
            set => _storageBucketName = value;
        }

        public string? LocalDirectoryForfileToDownload
        {
            get { return _localDirectoryForFileToDownload ?? throw new SettingsException($"In appsettings.json Firebase->LocalDirectoryForfileToDownload not exists"); }
            set => _localDirectoryForFileToDownload = value;
        }

        public string? FirestoreDirectoryForFileToUpload
        {
            get { return _firestoreDirectoryForFileToUpload ?? throw new SettingsException($"In appsettings.json Firebase->FirestoreDirectoryForFileToUpload not exists"); }
            set => _firestoreDirectoryForFileToUpload = value;
        }
    }
}
