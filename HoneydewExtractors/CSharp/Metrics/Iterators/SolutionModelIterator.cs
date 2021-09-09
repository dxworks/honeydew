using System.Collections.Generic;
using HoneydewExtractors.Core.Metrics.Iterators;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewModels.CSharp;

namespace HoneydewExtractors.CSharp.Metrics.Iterators
{
    public class SolutionModelIterator : ModelIterator<SolutionModel>
    {
        private readonly IList<ModelIterator<ProjectModel>> _iterators;

        public SolutionModelIterator(IList<ModelIterator<ProjectModel>> iterators,
            IList<IModelVisitor<SolutionModel>> modelVisitors) : base(modelVisitors)
        {
            _iterators = iterators;
        }
        
        public SolutionModelIterator(IList<ModelIterator<ProjectModel>> iterators) : this(iterators, new List<IModelVisitor<SolutionModel>>())
        {
        }

        public override void Iterate(SolutionModel solutionModel)
        {
            foreach (var projectModel in solutionModel.Projects)
            {
                foreach (var iterator in _iterators)
                {
                    iterator.Iterate(projectModel);
                }
            }

            base.Iterate(solutionModel);
        }
    }
}
