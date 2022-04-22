using System.Collections.Generic;
using HoneydewCore.Logging;

namespace Honeydew.Extractors.Visitors;

public class CompositeVisitor : ICompositeVisitor
{
    protected readonly ILogger Logger;

    private readonly ISet<ITypeVisitor> _visitors = new HashSet<ITypeVisitor>();

    public CompositeVisitor(ILogger logger)
    {
        Logger = logger;
    }

    protected CompositeVisitor(IEnumerable<ITypeVisitor> visitors)
    {
        if (visitors == null)
        {
            return;
        }

        foreach (var visitor in visitors)
        {
            _visitors.Add(visitor);
        }
    }

    public void Add(ITypeVisitor visitor)
    {
        _visitors.Add(visitor);
    }

    public IEnumerable<ITypeVisitor> GetContainedVisitors()
    {
        return _visitors;
    }
}
