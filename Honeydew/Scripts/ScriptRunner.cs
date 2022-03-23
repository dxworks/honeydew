using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HoneydewCore.Logging;

namespace Honeydew.Scripts;

internal class ScriptRunner
{
    private readonly Dictionary<string, object> _defaultArguments;
    private readonly IProgressLogger _logger;

    public ScriptRunner(IProgressLogger logger, Dictionary<string, object> defaultArguments)
    {
        _logger = logger;
        _defaultArguments = defaultArguments;
    }

    public void UpdateArgument(string key, object value)
    {
        if (_defaultArguments.ContainsKey(key))
        {
            _defaultArguments[key] = value;
        }
    }

    public object RunForResult(ScriptRuntime scriptRuntime)
    {
        var arguments = new Dictionary<string, object>();
        foreach (var (key, value) in _defaultArguments)
        {
            arguments.Add(key, value);
        }

        var (script, runtimeArguments) = scriptRuntime;

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

            return script.RunForResult(arguments);
        }
        catch (Exception e)
        {
            _logger.Log($"Could not run script {script.GetType().Name} because {e}");
        }

        return null;
    }

    public void Run(bool runInParallel, List<ScriptRuntime> scriptRuntimes)
    {
        var arguments = new Dictionary<string, object>();
        foreach (var (key, value) in _defaultArguments)
        {
            arguments.Add(key, value);
        }

        if (runInParallel)
        {
            Parallel.ForEach(scriptRuntimes, runtime => Run(arguments, runtime));
        }
        else
        {
            foreach (var runtime in scriptRuntimes)
            {
                Run(arguments, runtime);
            }
        }
    }

    private void Run(Dictionary<string, object> arguments, ScriptRuntime scriptRuntime)
    {
        var (script, runtimeArguments) = scriptRuntime;

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

            _logger.Log($"Running Script {script.GetType().Name}");

            script.Run(arguments);
        }
        catch (Exception e)
        {
            _logger.Log($"Could not run script {script.GetType().Name} because {e}");
        }
    }
}
