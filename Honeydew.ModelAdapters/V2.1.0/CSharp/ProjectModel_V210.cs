namespace Honeydew.ModelAdapters.V2._1._0.CSharp;

public record ProjectModel_V210
{
    public string Name { get; set; } = null!;

    public string FilePath { get; set; } = null!;

    public IList<string> ProjectReferences { get; set; } = new List<string>();

    public IList<NamespaceModel_V210> Namespaces { get; set; } = new List<NamespaceModel_V210>();

    public IList<CompilationUnitModel_V210> CompilationUnits { get; set; } = new List<CompilationUnitModel_V210>();

    public ProjectModel_V210()
    {
    }

    public ProjectModel_V210(string name)
    {
        Name = name;
    }

    public void Add(CompilationUnitModel_V210 compilationUnitType)
    {
        CompilationUnits.Add(compilationUnitType);

        foreach (var classType in compilationUnitType.ClassTypes)
        {
            var namespaceModel = Namespaces.FirstOrDefault(model => model.Name == classType.ContainingTypeName);

            if (namespaceModel == null)
            {
                var model = new NamespaceModel_V210();
                model.Add(classType);
                Namespaces.Add(model);
            }
            else
            {
                namespaceModel.Add(classType);
            }
        }
    }
}
