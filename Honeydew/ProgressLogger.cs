using System;
using System.Collections.Generic;
using HoneydewCore.Logging;
using Konsole;

namespace Honeydew
{
    public class ProgressLogger : IProgressLogger
    {
        private readonly Dictionary<string, IProgressBar> _progressBars = new();

        public void Log(object value)
        {
            Console.WriteLine(value);
        }

        public void Log()
        {
            Console.WriteLine();
        }

        public void CreateProgressBars(IEnumerable<string> progressBarNames)
        {
            foreach (var progressBarName in progressBarNames)
            {
                var progressBar = new ProgressBar(PbStyle.DoubleLine, 0);
                progressBar.Refresh(0, progressBarName);
                _progressBars.Add(progressBarName, progressBar);
            }
        }

        public IProgressLoggerBar CreateProgressLogger(int totalCount, string name)
        {
            if (_progressBars.TryGetValue(name, out var progressBar))
            {
                progressBar.Max = totalCount;
                try
                {
                    return new ProgressLoggerBar(progressBar, name);
                }
                catch (Exception)
                {
                    return new EmptyProgressLoggerBar(name);
                }
            }

            var createdProgressBar = new ProgressBar(PbStyle.DoubleLine, totalCount)
            {
                Max = totalCount
            };
            createdProgressBar.Refresh(0, name);
            _progressBars.Add(name, createdProgressBar);
            try
            {
                return new ProgressLoggerBar(createdProgressBar, name);
            }
            catch (Exception)
            {
                return new EmptyProgressLoggerBar(name);
            }
        }

        public void StopProgressBar(string name)
        {
            if (_progressBars.TryGetValue(name, out var progressBar))
            {
                progressBar.Refresh(progressBar.Max, $"Done");
            }
        }
    }
}
