using System.Globalization;
using Honeydew.Extractors.Exporters;

namespace Honeydew.DesignSmellsDetection.DetectionStrategies;

public static class DesignSmellsJsonWriter
{
    public static void Export(JsonModelExporter jsonModelExporter, IEnumerable<DesignSmell> designSmells,
        string designSmellsOutputFile)
    {
        var designFlaws = designSmells.Select(DesignFlaw.From).ToList();
        WriteToJson(jsonModelExporter, designFlaws, designSmellsOutputFile);
    }

    private static void WriteToJson(JsonModelExporter jsonModelExporter, IReadOnlyCollection<DesignFlaw> designFlaws,
        string output)
    {
        jsonModelExporter.Export(output, designFlaws);
    }
}

internal class DesignFlaw
{
    public DesignFlaw(string file, string name, string category, string value, IDictionary<string, double> metrics)
    {
        File = file;
        Name = name;
        Category = category;
        Value = value;
        Metrics = metrics;
    }

    public string File { get; }

    public string Name { get; }

    public string Category { get; }

    public string Value { get; }

    public IDictionary<string, double> Metrics { get; }

    public static DesignFlaw From(DesignSmell smell)
    {
        return new DesignFlaw(smell.SourceFile, smell.Name, GetCategory(smell.Name),
            RoundToInt(smell.Severity).ToString(CultureInfo.InvariantCulture), smell.Metrics);
    }

    private static int RoundToInt(double value)
    {
        return Convert.ToInt32(value);
    }

    private static string GetCategory(string name)
    {
        const string encapsulation = "Encapsulation";
        const string inheritance = "Inheritance Relations";
        const string coupling = "Coupling";
        var designSmellToCategory = new Dictionary<string, string>
        {
            { "God Class", encapsulation },
            { "Feature Envy", encapsulation },
            { "Data Class", encapsulation },
            { "Intensive Coupling", coupling },
            { "Dispersed Coupling", coupling },
            { "Shotgun Surgery", coupling },
            { "Refused Parent Bequest", inheritance },
            { "Tradition Breaker", inheritance },
            { "Blob Method", "Complexity" }
        };

        return designSmellToCategory.ContainsKey(name) ? designSmellToCategory[name] : name;
    }
}
