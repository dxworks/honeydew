using System.Collections.Generic;
using HoneydewExtractors.Core.Metrics.Iterators;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewModels.CSharp;

namespace HoneydewExtractors.CSharp.Metrics.Iterators
{
    public class ProjectModelIterator : ModelIterator<ProjectModel>
    {
        private readonly IList<ModelIterator<NamespaceModel>> _iterators;

        public ProjectModelIterator(IList<ModelIterator<NamespaceModel>> iterators,
            IList<IModelVisitor<ProjectModel>> modelVisitors) : base(modelVisitors)
        {
            _iterators = iterators;
        }

        public ProjectModelIterator(IList<ModelIterator<NamespaceModel>> iterators) : this(iterators,
            new List<IModelVisitor<ProjectModel>>())
        {
        }

        public override void Iterate(ProjectModel projectModel)
        {
            foreach (var solutionModel in projectModel.Namespaces)
            {
                foreach (var iterator in _iterators)
                {
                    iterator.Iterate(solutionModel);
                }
            }

            base.Iterate(projectModel);
        }
    }
}
