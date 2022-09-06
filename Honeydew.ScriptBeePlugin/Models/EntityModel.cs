namespace Honeydew.ScriptBeePlugin.Models;

public class EntityModel : ReferenceEntity
{
    private HashSet<EntityModel>? _hierarchy;
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

    public IDictionary<string, double> Metrics { get; set; } = new Dictionary<string, double>();

    public IEnumerable<EntityModel> Hierarchy
    {
        get
        {
            if (_hierarchy != null) return _hierarchy;

            var directBaseTypes = this switch
            {
                ClassModel classModel => new HashSet<EntityModel>(classModel.BaseTypes.Select(t => t.Entity)),
                InterfaceModel interfaceModel => new HashSet<EntityModel>(
                    interfaceModel.BaseTypes.Select(t => t.Entity)),
                _ => throw new InvalidOperationException("Can compute Hierarchy only for Class or Interface")
            };

            var indirectBaseTypes = directBaseTypes.SelectMany(t => t.Hierarchy);

            _hierarchy = directBaseTypes.Union(indirectBaseTypes).ToHashSet();
            _hierarchy.Add(this);
            return _hierarchy;
        }
    }

    public override string ToString()
    {
        return Name;
    }
}