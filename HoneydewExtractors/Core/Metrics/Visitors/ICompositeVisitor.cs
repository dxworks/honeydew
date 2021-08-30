using System.Collections.Generic;

namespace HoneydewExtractors.Core.Metrics.Visitors
{
    public interface ICompositeVisitor : ITypeVisitor
    {
        void Add(ITypeVisitor visitor);

        IEnumerable<ITypeVisitor> GetContainedVisitors();
    }
}
