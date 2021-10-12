using System.Collections.Generic;
using HoneydewExtractors.Core.Metrics.Iterators;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewModels.CSharp;
using HoneydewModels.Types;

namespace HoneydewExtractors.CSharp.Metrics.Iterators
{
    public class ProjectModelIterator : ModelIterator<ProjectModel>
    {
        private readonly IList<ModelIterator<ICompilationUnitType>> _iterators;

        public ProjectModelIterator(IList<ModelIterator<ICompilationUnitType>> iterators,
            IList<IModelVisitor<ProjectModel>> modelVisitors) : base(modelVisitors)
        {
            _iterators = iterators;
        }

        public ProjectModelIterator(IList<ModelIterator<ICompilationUnitType>> iterators) : this(iterators,
            new List<IModelVisitor<ProjectModel>>())
        {
        }

        public override void Iterate(ProjectModel projectModel)
        {
            foreach (var compilationUnitType in projectModel.CompilationUnits)
            {
                foreach (var iterator in _iterators)
                {
                    iterator.Iterate(compilationUnitType);
                }
            }

            base.Iterate(projectModel);
        }
    }
}
