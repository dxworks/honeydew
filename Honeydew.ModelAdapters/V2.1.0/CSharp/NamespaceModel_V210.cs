namespace Honeydew.ModelAdapters.V2._1._0.CSharp;

public record NamespaceModel_V210
{
    public string Name { get; set; } = "";
    public IList<string> ClassNames { get; set; } = new List<string>();

    public void Add(IClassType_V210 classType)
    {
        if (!string.IsNullOrEmpty(Name) && classType.ContainingTypeName != Name)
        {
            return;
        }

        if (ClassNames.Any(name => name == classType.Name))
        {
            return;
        }

        Name = classType.ContainingTypeName;

        ClassNames.Add(classType.Name);
    }
}
