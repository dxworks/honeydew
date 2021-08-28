﻿using System;
using System.Collections.Generic;
using HoneydewCore.Logging;

namespace Honeydew
{
    public class NoBarsProgressLogger : IProgressLogger
    {
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
        }

        public IProgressLoggerBar CreateProgressLogger(int totalCount, string text)
        {
            return new EmptyProgressLoggerBar(text);
        }

        public void StopProgressBar(string solutionPath)
        {
        }
    }
}