using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirebaseManager.Firebase
{
    /// <summary>
    /// Firestore DTO interface.
    /// </summary>
    public interface IFirestoreDto
    {
        /// <summary>
        /// This field is used to name the collection for the dto document in Firestore.
        /// </summary>
        string CollectionName { get; }

        /// <summary>
        /// This field is used to uniquely identify a document in Firestore.
        /// The best idea is to use a field that is unique in the DTO object.
        /// </summary>
        string DocumentUniqueField { get; }
    }
}
