using Google.Cloud.Firestore;

namespace FirebaseManager.Firebase
{
    public interface IFirestoreConnector
    {
        void Connect();
        FirestoreDb GetFirestoreDb();
    }
}