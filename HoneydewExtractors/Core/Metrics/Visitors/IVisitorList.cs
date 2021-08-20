using System.Collections.Generic;

namespace HoneydewExtractors.Core.Metrics.Visitors
{
    public interface IVisitorList
    {
        void Add(ITypeVisitor typeVisitor);

        IEnumerable<T> OfType<T>() where T : ITypeVisitor;

        void AddRange<T>(List<T> list) where T : ITypeVisitor;
    }
}
