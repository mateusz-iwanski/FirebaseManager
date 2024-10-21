using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirebaseManager.Firestore
{
    /// <summary>
    /// Firestore DTO interface.
    /// 
    /// DTO example:
    /// <code>
    /// [FirestoreData]
    /// public record ItemDto : IBaseDto, IResponseDto, IFirestoreDto
    /// {
    ///     [FirestoreProperty]
    ///     public string ItemCode { get; init; }
    ///     [FirestoreProperty]
    ///     public string ItemName { get; init; }
    ///     [FirestoreProperty]
    ///     public string LanguageCode { get; init; }
    ///     
    ///     public string CollectionName { get => "items"; }
    ///     public string DocumentUniqueField { get => ItemCode; }
    /// }
    /// </code>
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
