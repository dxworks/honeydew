using Honeydew.DesignSmellsDetection.DetectionStrategies;
using Honeydew.Extractors.Exporters;

namespace Honeydew.DesignSmellsDetection.Runner;

public static class DesignSmellsJsonWriter
{
    public static void Export(JsonModelExporter jsonModelExporter, IEnumerable<DesignSmell> designSmells,
        string designSmellsOutputFile)
    {
        var designFlaws = designSmells.Select(DesignFlaw.From).ToList();
        {
            var output = new Output
            {
                file = new Concerns
                {
                    concerns = designFlaws.ToArray()
                }
            };

            WriteToJson(jsonModelExporter, output, designSmellsOutputFile);
        }
    }

    private static void WriteToJson(JsonModelExporter jsonModelExporter, Output designFlaws,
        string output)
    {
        jsonModelExporter.Export(output, designFlaws);
    }
}

internal class Output
{
    public Concerns file { get; set; }
}

internal class Concerns
{
    public DesignFlaw[] concerns { get; set; }
}

internal class DesignFlaw
{
    public DesignFlaw(string file, string tag, int strength)
    {
        entity = file;
        this.strength = strength;
        this.tag = tag;
    }

    public string entity { get; }

    public string tag { get; }

    public int strength { get; }

    public static DesignFlaw From(DesignSmell smell)
    {
        return new DesignFlaw(smell.SourceFile, GetTag(smell.Name),
            RoundToInt(smell.Severity));
    }

    private static int RoundToInt(double value)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
        {
            throw new InvalidOperationException($"Severity must be a finite number, got '{value}'.");
        }

        var clamped = Math.Clamp(value, 0d, 10d);
        return (int)Math.Round(clamped, MidpointRounding.AwayFromZero);
    }

    private static string GetTag(string name)
    {
        var designSmellToCategory = new Dictionary<string, string>
        {
            { "God Class", "anomaly.codesmell.encapsulation.GodClass" },
            { "Feature Envy", "anomaly.codesmell.encapsulation.FeatureEnvy" },
            { "Data Class", "anomaly.codesmell.encapsulation.DataClass" },
            { "Intensive Coupling", "anomaly.cohesion.tangling.IntensiveCoupling" },
            { "Dispersed Coupling", "anomaly.cohesion.tangling.DispersedCoupling" },
            { "Shotgun Surgery", "anomaly.codesmell.encapsulation.ShotgunSurgery" },
            { "Refused Parent Bequest", "anomaly.codesmell.inheritance.RefusedParentBequest" },
            { "Tradition Breaker", "anomaly.codesmell.inheritance.TraditionBreaker" },
            { "Blob Method", "anomaly.codesmell.complexity.BlobMethod" }
        };

        return designSmellToCategory.ContainsKey(name) ? designSmellToCategory[name] : name;
    }
}
