using Honeydew.Logging;

namespace Honeydew.Scripts;

internal class ScriptRunner
{
    private readonly IProgressLogger _logger;

    public ScriptRunner(IProgressLogger logger)
    {
        _logger = logger;
    }

    public object? RunForResult(ScriptRuntime scriptRuntime)
    {
        var (script, arguments) = scriptRuntime;
        arguments ??= new Dictionary<string, object?>();

        try
        {
            foreach (var (key, value) in arguments)
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
        if (runInParallel)
        {
            Parallel.ForEach(scriptRuntimes, Run);
        }
        else
        {
            foreach (var runtime in scriptRuntimes)
            {
                Run(runtime);
            }
        }
    }

    private void Run(ScriptRuntime scriptRuntime)
    {
        var (script, arguments) = scriptRuntime;
        arguments ??= new Dictionary<string, object?>();   

        try
        {
            foreach (var (key, value) in arguments)
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

            _logger.Log($"Running Script {script.GetType().Name}");

            script.Run(arguments);
        }
        catch (Exception e)
        {
            _logger.Log($"Could not run script {script.GetType().Name} because {e}");
        }
    }
}
