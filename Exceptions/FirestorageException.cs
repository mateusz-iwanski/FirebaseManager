using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirebaseManager.Exceptions
{
    public class FirestorageException : CustomException
    {
        public FirestorageException(string message) : base(message)
        {
        }

        public FirestorageException(string message, Exception innerException) : base(message, innerException) 
        { 
        }
    }
}
