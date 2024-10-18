using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirebaseManager.Exceptions
{
    public class TypeAccessException : CustomException
    {
        public TypeAccessException(string message) : base(message)
        {
        }

        public TypeAccessException(string message, Exception inner) : base(message, inner)
        {
        }

    }
}
