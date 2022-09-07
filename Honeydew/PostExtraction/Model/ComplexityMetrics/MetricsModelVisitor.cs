using Honeydew.DesignSmellsDetection.Metrics;
using Honeydew.ScriptBeePlugin.Models;

namespace Honeydew.PostExtraction.Model.ComplexityMetrics;

public class MetricsModelVisitor : IModelVisitor<EntityModel>
{
    public void Visit(EntityModel entityModel)
    {
        if (entityModel is not ClassModel classModel) return;


        //var atfd = classModel.ForeignData().Count();


        //var nopa = classModel.Fields.Count(f => f.AccessModifier == AccessModifier.Public);

        //woc += classModel.Properties.SelectMany(p => p.Accessors)
        //    .Count(a => a.AccessModifier == AccessModifier.Public);
        //nopa += classModel.Properties.Count(p => p.AccessModifier == AccessModifier.Public);

        var metrics = ClassMetrics.For(classModel);
        metrics.WeightOfAClass = WeightOfAClass.Value(classModel);
        metrics.AccessToForeignDataForType = AccessToForeignDataForType.Value(classModel);
        metrics.WeightedMethodCount = WeightedMethodCount.Value(classModel);
        metrics.TotalClassCohesion = TotalClassCohesion.Value(classModel);
    }
}