namespace Honeydew.ModelRepresentations;

public record Relation
{
    public string Source { get; set; } = "";

    public string Target { get; set; } = "";

    public string Type { get; set; }

    public int Strength { get; set; }
}
