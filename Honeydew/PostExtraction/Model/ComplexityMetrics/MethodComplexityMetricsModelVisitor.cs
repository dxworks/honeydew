using Honeydew.ScriptBeePlugin.Models;

namespace Honeydew.PostExtraction.Model.ComplexityMetrics;

// AMW = Average Method Weight
// MWC = Weighted Method Count
// NOM = Number of Methods
public class MethodComplexityMetricsModelVisitor : IModelVisitor<EntityModel>
{
    public void Visit(EntityModel entityModel)
    {
        if (entityModel is not ClassModel classModel)
        {
            return;
        }

        var destructorCount = classModel.Destructor is null ? 0 : 1;
        var methodCount = classModel.Methods.Count + classModel.Constructors.Count + destructorCount;

        var wmc =
            classModel.Methods.Sum(m => m.CyclomaticComplexity) +
            classModel.Constructors.Sum(c => c.CyclomaticComplexity) +
            classModel.Destructor?.CyclomaticComplexity ?? 0;

        wmc += classModel.Properties.Sum(p => p.CyclomaticComplexity);
        methodCount += classModel.Properties.SelectMany(p => p.Accessors).Count();

        var amw = 0;
        if (methodCount > 0)
        {
            amw = wmc / methodCount;
        }

        classModel.Metrics["WMC"] = wmc;
        classModel.Metrics["AMW"] = amw;
        classModel.Metrics["NOM"] = methodCount;
    }
}
