using System;

namespace HoneydewExtractors.Core
{
    public class ExtractionException : Exception
    {
        public ExtractionException(string message) : base(message)
        {
        }
    }
}
