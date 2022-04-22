using System.Collections.Generic;

namespace Honeydew.Extractors.Visitors;

public interface ICompositeVisitor : ITypeVisitor
{
    void Add(ITypeVisitor visitor);

    IEnumerable<ITypeVisitor> GetContainedVisitors();
}
