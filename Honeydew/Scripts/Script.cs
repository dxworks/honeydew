namespace Honeydew.Scripts;

public abstract class Script
{
    public virtual void Run(Dictionary<string, object?> arguments)
    {
    }

    public virtual object? RunForResult(Dictionary<string, object?> arguments)
    {
        return null;
    }

    protected static T? VerifyArgument<T>(Dictionary<string, object?> arguments, string argumentName)
    {
        if (!arguments.TryGetValue(argumentName, out var value))
        {
            throw new ArgumentException($"Argument {argumentName} not present!");
        }

        if (value == null)
        {
            return default;
        }

        if (value is not T tValue)
        {
            throw new ArgumentException($"Invalid type ({typeof(T).Name}) for {argumentName} !");
        }

        return tValue;
    }
}
