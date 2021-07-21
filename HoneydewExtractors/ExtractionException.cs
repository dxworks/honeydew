using System;

namespace HoneydewExtractors
{
    public class ExtractionException : Exception
    {
        public ExtractionException(string message) : base(message)
        {
        }
    }
}
