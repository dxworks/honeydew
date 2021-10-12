using System.Collections.Generic;
using HoneydewExtractors.Core.Metrics.Iterators;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewModels.Types;

namespace HoneydewExtractors.CSharp.Metrics.Iterators
{
    public class CompilationUnitModelIterator : ModelIterator<ICompilationUnitType>
    {
        private readonly IList<ModelIterator<IClassType>> _iterators;

        public CompilationUnitModelIterator(IList<ModelIterator<IClassType>> iterators,
            IList<IModelVisitor<ICompilationUnitType>> modelVisitors) : base(modelVisitors)
        {
            _iterators = iterators;
        }

        public CompilationUnitModelIterator(IList<ModelIterator<IClassType>> iterators) : this(iterators,
            new List<IModelVisitor<ICompilationUnitType>>())
        {
        }

        public override void Iterate(ICompilationUnitType compilationUnitModel)
        {
            foreach (var solutionModel in compilationUnitModel.ClassTypes)
            {
                foreach (var iterator in _iterators)
                {
                    iterator.Iterate(solutionModel);
                }
            }

            base.Iterate(compilationUnitModel);
        }
    }
}
