namespace Honeydew.DesignSmellsDetection.DetectionStrategies;

public class DesignSmell
{
    public string SourceFile { get; set; } = null!;

    public string Name { get; set; } = null!;

    public double Severity { get; set; }

    public IDictionary<string, double> Metrics { get; set; } = new Dictionary<string, double>();
}
