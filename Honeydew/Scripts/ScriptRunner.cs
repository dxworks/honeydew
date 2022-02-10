using System;
using System.Collections.Generic;
using HoneydewCore.Logging;

namespace Honeydew.Scripts
{
    internal class ScriptRunner
    {
        private readonly Dictionary<string, object> _defaultArguments;
        private readonly IProgressLogger _logger;

        public ScriptRunner(IProgressLogger logger, Dictionary<string, object> defaultArguments)
        {
            _logger = logger;
            _defaultArguments = defaultArguments;
        }

        public void Run(List<ScriptRuntime> scriptRuntimes, bool changeDefaultArguments = false)
        {
            var arguments = new Dictionary<string, object>();
            foreach (var (key, value) in _defaultArguments)
            {
                arguments.Add(key, value);
            }

            foreach (var (script, runtimeArguments) in scriptRuntimes)
            {
                try
                {
                    if (runtimeArguments != null)
                    {
                        foreach (var (key, value) in runtimeArguments)
                        {
                            if (arguments.ContainsKey(key))
                            {
                                arguments[key] = value;
                            }
                            else
                            {
                                arguments.Add(key, value);
                            }
                        }
                    }

                    script.Run(arguments);

                    if (!changeDefaultArguments)
                    {
                        continue;
                    }

                    foreach (var (key, value) in arguments)
                    {
                        if (_defaultArguments.ContainsKey(key))
                        {
                            _defaultArguments[key] = value;
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger.Log($"Could not run script because {e}");
                }
            }
        }
    }
}
