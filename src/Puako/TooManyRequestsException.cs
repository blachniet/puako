using System;

namespace Puako
{
    [System.Serializable]
    public class TooManyRequestsException : System.Exception
    {
        public TimeSpan? RetryAfter { get; set; }

        public TooManyRequestsException() { }

        public TooManyRequestsException(string message) : base(message) { }

        public TooManyRequestsException(string message, System.Exception inner) : base(message, inner) { }

        protected TooManyRequestsException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}