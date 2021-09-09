using System.Collections.Generic;
using HoneydewExtractors.Core.Metrics.Iterators;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewModels.Types;

namespace HoneydewExtractors.CSharp.Metrics.Iterators
{
    public class ClassTypePropertyIterator : ModelIterator<IClassType>
    {
        private readonly IList<ModelIterator<IPropertyType>> _propertyIterators;

        public ClassTypePropertyIterator(IList<ModelIterator<IPropertyType>> propertyIterators,
            IList<IModelVisitor<IClassType>> modelVisitors) : base(modelVisitors)
        {
            _propertyIterators = propertyIterators;
        }

        public ClassTypePropertyIterator(IList<IModelVisitor<IClassType>> modelVisitors) : this(
            new List<ModelIterator<IPropertyType>>(), modelVisitors)
        {
        }

        public override void Iterate(IClassType classType)
        {
            if (classType is not IPropertyMembersClassType propertyMembersClassType)
            {
                return;
            }

            foreach (var solutionModel in propertyMembersClassType.Properties)
            {
                foreach (var iterator in _propertyIterators)
                {
                    iterator.Iterate(solutionModel);
                }
            }

            base.Iterate(classType);
        }
    }
}
