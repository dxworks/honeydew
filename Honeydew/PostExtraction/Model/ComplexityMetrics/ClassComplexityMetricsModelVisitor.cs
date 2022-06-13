using Honeydew.ScriptBeePlugin.Models;

namespace Honeydew.PostExtraction.Model.ComplexityMetrics;

// WOC = Weight of a Class
// NOPA = Number of Public Attributes
public class ClassComplexityMetricsModelVisitor : IModelVisitor<EntityModel>
{
    public void Visit(EntityModel entityModel)
    {
        if (entityModel is not ClassModel classModel)
        {
            return;
        }

        var woc = classModel.Methods.Count(m => m.AccessModifier == AccessModifier.Public) +
                  classModel.Constructors.Count(m => m.AccessModifier == AccessModifier.Public);
        var nopa = classModel.Fields.Count(f => f.AccessModifier == AccessModifier.Public);

        woc += classModel.Properties.SelectMany(p => p.Accessors)
            .Count(a => a.AccessModifier == AccessModifier.Public);
        nopa += classModel.Properties.Count(p => p.AccessModifier == AccessModifier.Public);

        classModel.Metrics["WOC"] = woc;
        classModel.Metrics["NOPA"] = nopa;
    }
}
