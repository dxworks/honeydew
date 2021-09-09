using System.Collections.Generic;
using HoneydewExtractors.Core.Metrics.Iterators;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewModels.CSharp;
using HoneydewModels.Types;

namespace HoneydewExtractors.CSharp.Metrics.Iterators
{
    public class NamespaceModelIterator : ModelIterator<NamespaceModel>
    {
        private readonly IList<ModelIterator<IClassType>> _iterators;

        public NamespaceModelIterator(IList<ModelIterator<IClassType>> iterators,
            IList<IModelVisitor<NamespaceModel>> modelVisitors) : base(modelVisitors)
        {
            _iterators = iterators;
        }

        public NamespaceModelIterator(IList<ModelIterator<IClassType>> iterators) : this(iterators,
            new List<IModelVisitor<NamespaceModel>>())
        {
        }

        public override void Iterate(NamespaceModel namespaceModel)
        {
            foreach (var solutionModel in namespaceModel.ClassModels)
            {
                foreach (var iterator in _iterators)
                {
                    iterator.Iterate(solutionModel);
                }
            }

            base.Iterate(namespaceModel);
        }
    }
}
