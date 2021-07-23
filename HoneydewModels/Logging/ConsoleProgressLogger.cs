using System;

namespace HoneydewCore.Logging
{
    public class ConsoleProgressLogger : IProgressLogger
    {
        public void LogLine(string value)
        {
            Console.WriteLine(value);
        }

        public void Log(string value)
        {
            Console.Write(value);
        }
    }
}