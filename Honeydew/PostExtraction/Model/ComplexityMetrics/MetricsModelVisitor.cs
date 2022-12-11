using Honeydew.DesignSmellsDetection.Metrics;
using DxWorks.ScriptBee.Plugins.Honeydew.Models;

namespace Honeydew.PostExtraction.Model.ComplexityMetrics;

public class MetricsModelVisitor : IModelVisitor<EntityModel>
{
    public void Visit(EntityModel entityModel)
    {
        if (entityModel is not ClassModel classModel) return;
        
        //var nopa = classModel.Fields.Count(f => f.AccessModifier == AccessModifier.Public);
        var metrics = Metrics.For(classModel);
        metrics.WeightOfAClass = WeightOfAClass.Value(classModel);
        metrics.AccessToForeignData = AccessToForeignData.Value(classModel);
        metrics.WeightedMethodCount = WeightedMethodCount.Value(classModel);
        metrics.AverageMethodWeight = AverageMethodWeight.Value(classModel, metrics.WeightedMethodCount);
        metrics.TotalClassCohesion = TotalClassCohesion.Value(classModel);

        if (classModel.BaseClass != null && classModel.BaseClass.Name != "object")
        {
            metrics.BaseClassUsageRatio = BaseClassUsageRatio.Value(classModel);
            metrics.BaseClassOverridingRatio = BaseClassOverridingRatio.Value(classModel);
            metrics.NumberOfProtectedMembers = NumberOfProtectedMembers.Value(classModel);
            metrics.NopOverridingMethods = NopOverridingMethods.Value(classModel);
        }

        foreach (var method in classModel.MethodsToConsiderForDesignSmells())
        {
            var methodMetrics = Metrics.For(method);
            methodMetrics.AccessToForeignData = AccessToForeignData.Value(method);
            methodMetrics.LocalityOfAttributeAccess = LocalityOfAttributeAccess.Value(method);
            methodMetrics.ForeignDataProviders = ForeignDataProviders.Value(method);
            methodMetrics.NumberOfAccessedVariables = NumberOfAccessedVariables.Value(method);
            methodMetrics.CouplingIntensity = CouplingIntensity.Value(method);
            methodMetrics.CouplingDispersion = CouplingDispersion.Value(method, methodMetrics.CouplingIntensity);
        }
    }
}
