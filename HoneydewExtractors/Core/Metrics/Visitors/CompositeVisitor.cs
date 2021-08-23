using System.Collections.Generic;
using HoneydewModels.Types;

namespace HoneydewExtractors.Core.Metrics.Visitors
{
    public class CompositeVisitor : ICompositeVisitor, IModelVisitor
    {
        private readonly ISet<ITypeVisitor> _visitors = new HashSet<ITypeVisitor>();

        public CompositeVisitor()
        {
        }

        public CompositeVisitor(IEnumerable<ITypeVisitor> visitors)
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

        public void Accept(IVisitor visitor)
        {
            foreach (var typeVisitor in _visitors)
            {
                visitor.Visit(typeVisitor);
            }
        }

        public IType Visit(IType modelType)
        {
            foreach (var visitor in _visitors)
            {
                if (visitor is IModelVisitor modelVisitor)
                {
                    modelType = modelVisitor.Visit(modelType);
                }
            }

            return modelType;
        }
    }
}
