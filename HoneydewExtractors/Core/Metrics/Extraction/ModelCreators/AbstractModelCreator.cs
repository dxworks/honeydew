using System.Collections.Generic;
using HoneydewExtractors.Core.Metrics.Visitors;

namespace HoneydewExtractors.Core.Metrics.Extraction.ModelCreators
{
    public abstract class AbstractModelCreator<TSyntaxNode, TModelType, TVisitorType>
        where TVisitorType : IVisitorType<TSyntaxNode, TModelType>
    {
        private readonly IEnumerable<TVisitorType> _visitors;

        protected AbstractModelCreator(IEnumerable<TVisitorType> visitors)
        {
            _visitors = visitors;
        }

        public TModelType Create(TSyntaxNode syntaxNode, TModelType modelType)
        {
            foreach (var visitor in _visitors)
            {
                modelType = visitor.Visit(syntaxNode, modelType);
            }

            return modelType;
        }

        public IEnumerable<TVisitorType> GetVisitors()
        {
            return _visitors;
        }
    }
}
