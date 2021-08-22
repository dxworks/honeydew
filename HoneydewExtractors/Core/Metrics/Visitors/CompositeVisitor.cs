using System.Collections.Generic;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis.CSharp;

namespace HoneydewExtractors.Core.Metrics.Visitors
{
    public abstract class CompositeVisitor : ICompositeVisitor
    {
        private readonly ISet<ITypeVisitor> _visitors = new HashSet<ITypeVisitor>();

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

        public void Accept(IVisitor visitor)
        {
            foreach (var typeVisitor in _visitors)
            {
                visitor.Visit(typeVisitor);
            }
        }
    }

    public class CompositeVisitor<TType> : ICSharpCompositeVisitor<TType>
        where TType : IType
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

        public TType Visit(CSharpSyntaxNode syntaxNode, TType modelType)
        {
            foreach (var visitor in _visitors)
            {
                if (visitor is IExtractionVisitor<CSharpSyntaxNode, TType> extractionVisitor)
                {
                    modelType = extractionVisitor.Visit(syntaxNode, modelType);
                }
            }

            return modelType;
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

        public void Accept(IVisitor visitor)
        {
            foreach (var typeVisitor in _visitors)
            {
                visitor.Visit(typeVisitor);
            }
        }
    }
}
