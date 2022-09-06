using Honeydew.ScriptBeePlugin.Models;

namespace Honeydew.PostExtraction.Model.DetectionStrategies;

public class DesignSmell
{
    public string SourceFile { get; set; }

    public string Name { get; set; }

    public double Severity { get; set; }

    public EntityModel Source { get; set; }
}