using System.Collections.Generic;
using HoneydewExtractors.Core.Metrics.Iterators;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewModels.CSharp;

namespace HoneydewExtractors.CSharp.Metrics.Iterators
{
    public class RepositoryModelIterator : ModelIterator<RepositoryModel>
    {
        private readonly IList<ModelIterator<SolutionModel>> _iterators;

        public RepositoryModelIterator(IList<ModelIterator<SolutionModel>> iterators,
            IList<IModelVisitor<RepositoryModel>> modelVisitors) : base(modelVisitors)
        {
            _iterators = iterators;
        }

        public RepositoryModelIterator(IList<ModelIterator<SolutionModel>> iterators) : this(iterators,
            new List<IModelVisitor<RepositoryModel>>())
        {
            _iterators = iterators;
        }

        public override void Iterate(RepositoryModel repositoryModel)
        {
            foreach (var solutionModel in repositoryModel.Solutions)
            {
                foreach (var iterator in _iterators)
                {
                    iterator.Iterate(solutionModel);
                }
            }

            base.Iterate(repositoryModel);
        }
    }
}
