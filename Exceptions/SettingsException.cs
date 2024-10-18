using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirebaseManager.Exceptions
{
    public class SettingsException : CustomException
    {
        public SettingsException(string message) : base(message)
        {
        }

        public SettingsException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
