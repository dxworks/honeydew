using Honeydew.Models.Types;
using Honeydew.PostExtraction.Model.Metrics;
using Honeydew.ScriptBeePlugin.Models;
using Honeydew.Scripts;

namespace Honeydew.PostExtraction.Model.ComplexityMetrics;

public class MetricsModelVisitor : IModelVisitor<EntityModel>
{
    public void Visit(EntityModel entityModel)
    {
        if (entityModel is not ClassModel classModel)
        {
            return;
        }



        //var atfd = classModel.ForeignData().Count();


        //var nopa = classModel.Fields.Count(f => f.AccessModifier == AccessModifier.Public);

        //woc += classModel.Properties.SelectMany(p => p.Accessors)
        //    .Count(a => a.AccessModifier == AccessModifier.Public);
        //nopa += classModel.Properties.Count(p => p.AccessModifier == AccessModifier.Public);

        classModel.Metrics["WOC"] = WeightOfAClass.Value(classModel);
        classModel.Metrics["ATFD"] = AccessToForeignDataForType.Value(classModel);
        classModel.Metrics["WMC"] = WeightedMethodCount.Value(classModel);
        classModel.Metrics["TCC"] = TotalClassCohesion.Value(classModel);

        if (classModel.Name.Contains("TccTestCase"))
        {

        }
    }

}