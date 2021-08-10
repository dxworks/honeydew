using System;

namespace HoneydewCore.Logging
{
    public interface IProgressLoggerFactory
    {
        public IProgressLogger CreateProgressLogger(int totalCount, string name, string parentName, ConsoleColor color);
    }
}
