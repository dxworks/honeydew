using Honeydew.ScriptBeePlugin.Models;

namespace Honeydew.DesignSmellsDetection.Metrics;

public class NopOverridingMethods
{
    public static int Value(ClassModel type)
    {
        // The method signature is counted as part of the lines of code.
        return type.Methods.Count(m => m.IsPubliclyVisible && m.IsOverride && m.LinesOfCode.SourceLines == 1);
    }
}