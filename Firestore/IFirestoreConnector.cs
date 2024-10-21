using Google.Cloud.Firestore;

namespace FirebaseManager.Firestore
{
    public interface IFirestoreConnector
    {
        void Connect();
        FirestoreDb GetFirestoreDb();
    }
}