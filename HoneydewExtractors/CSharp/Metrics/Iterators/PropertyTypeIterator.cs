// using System.Collections.Generic;
// using HoneydewExtractors.Core.Metrics.Iterators;
// using HoneydewModels.Types;
//
// namespace HoneydewExtractors.CSharp.Metrics.Iterators
// {
//     public class PropertyTypeIterator : ModelIterator<IPropertyType>
//     {
//         private readonly IList<ModelIterator<IPropertyType>> _propertyIterators;
//
//         public PropertyTypeIterator(IList<ModelIterator<IPropertyType>> propertyIterators)
//         {
//             _propertyIterators = propertyIterators;
//         }
//
//         public override void Iterate(IPropertyType propertyType)
//         {
//             foreach (var solutionModel in propertyType.)
//             {
//                 foreach (var iterator in _propertyIterators)
//                 {
//                     iterator.Iterate(solutionModel);
//                 }
//             }
//         }
//     }
// }
