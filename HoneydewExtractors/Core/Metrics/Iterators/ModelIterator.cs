using System.Collections.Generic;
using HoneydewExtractors.Core.Metrics.Visitors;

namespace HoneydewExtractors.Core.Metrics.Iterators
{
    public abstract class ModelIterator<TModel>
    {
        private readonly IList<IModelVisitor<TModel>> _modelVisitors;

        protected ModelIterator(IList<IModelVisitor<TModel>> modelVisitors)
        {
            _modelVisitors = modelVisitors;
        }

        public virtual void Iterate(TModel model)
        {
            foreach (var visitor in _modelVisitors)
            {
                visitor.Visit(model);
            }
        }
    }
}
