using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Agile.Core
{
    [Serializable]
    public class AgileException : Exception
    {
        public AgileException()
        {
        }

        public AgileException(string message)
            : base(message)
        {
        }

        public AgileException(string messageFormat, params object[] args)
            : base(string.Format(messageFormat, args))
        {
        }

        protected AgileException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public AgileException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
