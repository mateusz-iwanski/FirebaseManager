using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirebaseManager.Exceptions
{
    public class ArgumentException : CustomException
    {
        public ArgumentException(string message) : base(message)
        {
        }

        public ArgumentException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
