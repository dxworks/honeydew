using System.Collections.Generic;

namespace HoneydewCore.Logging
{
    public interface IProgressLogger
    {
        void Log(string value);
        void Log();

        void CreateProgressBars(IEnumerable<string> progressBarNames);

        IProgressLoggerBar CreateProgressLogger(int totalCount, string text);
        void StopProgressBar(string solutionPath);
    }
}
