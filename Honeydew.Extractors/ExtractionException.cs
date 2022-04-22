using System;

namespace Honeydew.Extractors;

public class ExtractionException : Exception
{
    public ExtractionException(string message) : base(message)
    {
    }
}
