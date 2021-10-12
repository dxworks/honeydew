using System.Collections.Generic;
using HoneydewExtractors.Core.Metrics.Iterators;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewModels.CSharp;

namespace HoneydewExtractors.CSharp.Metrics.Iterators
{
    public class RepositoryModelIterator : ModelIterator<RepositoryModel>
    {
        private readonly IList<ModelIterator<ProjectModel>> _iterators;

        public RepositoryModelIterator(IList<ModelIterator<ProjectModel>> iterators,
            IList<IModelVisitor<RepositoryModel>> modelVisitors) : base(modelVisitors)
        {
            _iterators = iterators;
        }

        public RepositoryModelIterator(IList<ModelIterator<ProjectModel>> iterators) : this(iterators,
            new List<IModelVisitor<RepositoryModel>>())
        {
            _iterators = iterators;
        }

        public override void Iterate(RepositoryModel repositoryModel)
        {
            foreach (var projectModel in repositoryModel.Projects)
            {
                foreach (var iterator in _iterators)
                {
                    iterator.Iterate(projectModel);
                }
            }

            base.Iterate(repositoryModel);
        }
    }
}
