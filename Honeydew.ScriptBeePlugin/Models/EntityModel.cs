namespace Honeydew.ScriptBeePlugin.Models;

public class EntityModel : ReferenceEntity
{
    public string Name { get; set; } = "";

    public string FilePath { get; set; } = "";

    public bool IsExternal { get; set; }

    public bool IsInternal { get; set; }

    public bool IsPrimitive { get; set; }

    public AccessModifier AccessModifier { get; set; }

    public string Modifier { get; set; }

    public IList<Modifier> Modifiers { get; set; } = new List<Modifier>();

    public NamespaceModel Namespace { get; set; }

    public FileModel File { get; set; }

    public IList<ImportModel> Imports { get; set; } = new List<ImportModel>();

    public IList<AttributeModel> Attributes { get; set; } = new List<AttributeModel>();

    public LinesOfCode LinesOfCode { get; set; }

    public IDictionary<string, int> Metrics { get; set; } = new Dictionary<string, int>();
}
