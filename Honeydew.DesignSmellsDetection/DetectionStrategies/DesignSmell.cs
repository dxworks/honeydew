using Honeydew.ScriptBeePlugin.Models;

namespace Honeydew.DesignSmellsDetection.DetectionStrategies;

public class DesignSmell
{
    public string SourceFile { get; set; }

    public string Name { get; set; }

    public double Severity { get; set; }

    public EntityModel Source { get; set; }
}