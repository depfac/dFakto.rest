using System;
using System.Runtime.Serialization;

namespace dFakto.Rest
{
    [Serializable]
    public class RestException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public RestException()
        {
        }

        public RestException(string message) : base(message)
        {
        }

        public RestException(string message, Exception inner) : base(message, inner)
        {
        }

        protected RestException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}