using HoneydewCore.ModelRepresentations;

namespace HoneydewCore.Processors
{
    public class
        RelationMetricHolderToClassRelationsProcessor : IProcessorFunction<IRelationMetricHolder,
            ClassRelationsRepresentation>
    {
        public ClassRelationsRepresentation Process(IRelationMetricHolder relationMetricHolder)
        {
            if (relationMetricHolder == null)
            {
                return new ClassRelationsRepresentation();
            }
            
            var classRelationsRepresentation = new ClassRelationsRepresentation();

            foreach (var fileRelation in relationMetricHolder.GetRelations())
            {
                classRelationsRepresentation.Add(fileRelation);
            }

            return classRelationsRepresentation;
        }
    }
}
