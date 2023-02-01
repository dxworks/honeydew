namespace Honeydew.ModelAdapters.V2._1._0.CSharp;

public record ClassModel_V210 : IClassType_V210
{
    public string ClassType { get; set; } = null!;

    public string Name { get; set; } = null!;

    public IList<GenericParameterModel_V210> GenericParameters { get; set; } = new List<GenericParameterModel_V210>();

    public string FilePath { get; set; } = null!;

    public LinesOfCode_V210 Loc { get; set; }

    public string AccessModifier { get; set; } = "";

    public string Modifier { get; set; } = "";


    private string _namespace = "";

    public string ContainingTypeName
    {
        get
        {
            if (!string.IsNullOrEmpty(_namespace) || string.IsNullOrEmpty(Name))
            {
                return _namespace;
            }

            var lastIndexOf = Name.LastIndexOf(".", StringComparison.Ordinal);
            if (lastIndexOf < 0)
            {
                return "";
            }

            _namespace = Name[..lastIndexOf];
            return _namespace;
        }
        set => _namespace = value;
    }

    public IList<BaseTypeModel_V210> BaseTypes { get; set; } = new List<BaseTypeModel_V210>();

    public IList<UsingModel_V210> Imports { get; set; } = new List<UsingModel_V210>();

    public IList<FieldModel_V210> Fields { get; init; } = new List<FieldModel_V210>();

    public IList<PropertyModel_V210> Properties { get; set; } = new List<PropertyModel_V210>();

    public IList<ConstructorModel_V210> Constructors { get; init; } = new List<ConstructorModel_V210>();

    public IList<MethodModel_V210> Methods { get; init; } = new List<MethodModel_V210>();

    public DestructorModel_V210? Destructor { get; set; }

    public IList<MetricModel_V210> Metrics { get; init; } = new List<MetricModel_V210>();

    public IList<AttributeModel_V210> Attributes { get; set; } = new List<AttributeModel_V210>();
}
