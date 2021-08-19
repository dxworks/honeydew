using System.Collections.Generic;

namespace HoneydewExtractors.Core.Metrics.Visitors
{
    public abstract class CompositeTypeVisitor : ITypeVisitor
    {
        private readonly ISet<ITypeVisitor> _visitors = new HashSet<ITypeVisitor>();

        public void Add(ITypeVisitor visitor)
        {
            _visitors.Add(visitor);
        }

        public IEnumerable<ITypeVisitor> GetContainedVisitors()
        {
            return _visitors;
        }
    }
}
