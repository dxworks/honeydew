using DxWorks.ScriptBee.Plugins.Honeydew.Models;

namespace Honeydew.DesignSmellsDetection.Metrics;

public class WeightedMethodCount
{
    public static int Value(ClassModel type)
    {
        var wmc =
            type.Methods.Sum(m => m.CyclomaticComplexity) +
            type.Constructors.Sum(c => c.CyclomaticComplexity) +
            (type.Destructor?.CyclomaticComplexity ?? 0) +
            type.Properties.Sum(p => p.CyclomaticComplexity);

        return wmc;
    }
}
