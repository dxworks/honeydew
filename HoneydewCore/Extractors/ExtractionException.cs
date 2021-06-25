using System;

namespace HoneydewCore.Extractors
{
    public class ExtractionException : Exception
    {
        public ExtractionException(string message) : base(message)
        {
        }
    }
}