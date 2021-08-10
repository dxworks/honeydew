using System;
using System.Collections.Generic;
using HoneydewCore.Logging;
using ShellProgressBar;

namespace Honeydew
{
    public class ShellProgressBarLoggerFactory : IProgressLoggerFactory
    {
        private readonly Dictionary<string, IProgressBar> _progressBars = new();

        private IProgressBar _rootProgressBar;

        public IProgressLogger CreateProgressLogger(int totalCount, string name, string parentName, ConsoleColor color)
        {
            var options = new ProgressBarOptions
            {
                ProgressCharacter = '-',
                CollapseWhenFinished = color == ConsoleColor.Blue,
                ForegroundColor = color
            };

            if (string.IsNullOrEmpty(parentName))
            {
                _rootProgressBar ??= new ProgressBar(totalCount, name, options);

                if (!_progressBars.ContainsKey("root"))
                {
                    _progressBars.Add("root", _rootProgressBar);
                }

                return new ShellProgressBarLogger(_rootProgressBar, name);
            }

            ChildProgressBar childProgressBar;
            ShellProgressBarLogger progressBarLogger;

            if (_progressBars.TryGetValue(parentName, out var progressLogger))
            {
                childProgressBar = progressLogger.Spawn(totalCount, name, options);
                _progressBars.Add(name, childProgressBar);

                progressBarLogger = new ShellProgressBarLogger(childProgressBar, name);
                return progressBarLogger;
            }

            childProgressBar = _rootProgressBar.Spawn(totalCount, name, options);
            _progressBars.Add(name, childProgressBar);

            progressBarLogger = new ShellProgressBarLogger(childProgressBar, name);
            return progressBarLogger;
        }
    }
}
