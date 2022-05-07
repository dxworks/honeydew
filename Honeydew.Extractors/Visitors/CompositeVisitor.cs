using Honeydew.Logging;

namespace Honeydew.Extractors.Visitors;

public abstract class CompositeVisitor<TType>
{
    protected readonly ILogger CompositeLogger;

    private readonly HashSet<ITypeVisitor<TType>> _visitors = new();

    protected CompositeVisitor(ILogger compositeLogger, IEnumerable<ITypeVisitor<TType>> visitors)
    {
        CompositeLogger = compositeLogger;

        foreach (var visitor in visitors)
        {
            _visitors.Add(visitor);
        }
    }

    public IEnumerable<ITypeVisitor<TType>> GetContainedVisitors()
    {
        return _visitors;
    }
}
