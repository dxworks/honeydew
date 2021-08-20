using System.Collections.Generic;
using System.Linq;

namespace HoneydewExtractors.Core.Metrics.Visitors
{
    public class VisitorList : IVisitorList
    {
        private readonly ISet<ITypeVisitor> _visitors = new HashSet<ITypeVisitor>();

        public void Add(ITypeVisitor typeVisitor)
        {
            if (typeVisitor is CompositeTypeVisitor compositeTypeVisitor)
            {
                foreach (var visitor in compositeTypeVisitor.GetContainedVisitors())
                {
                    Add(visitor);
                }
            }

            _visitors.Add(typeVisitor);
        }

        public IEnumerable<T> OfType<T>() where T : ITypeVisitor
        {
            return _visitors.OfType<T>();
        }

        public void AddRange<T>(List<T> list) where T : ITypeVisitor
        {
            foreach (var visitor in list)
            {
                Add(visitor);
            }
        }
    }
}
