using System.Collections.Generic;
using System.Linq;

namespace HoneydewModels.Reference;

public class PartialClassModel : ReferenceEntity
{
    public readonly IList<ClassModel> Classes = new List<ClassModel>();

    public string ClassType { get; init; }

    public string Name { get; init; }

    public IList<GenericParameterModel> GenericParameters { get; init; } = new List<GenericParameterModel>();

    public NamespaceModel Namespace { get; init; }

    public IEnumerable<ClassModel> BaseTypes => Classes.SelectMany(model => model.BaseTypes);

    public IEnumerable<ImportModel> Imports => Classes.SelectMany(model => model.Imports);

    public IEnumerable<FieldModel> Fields => Classes.SelectMany(model => model.Fields);

    public IEnumerable<PropertyModel> Properties => Classes.SelectMany(model => model.Properties);

    public IEnumerable<MethodModel> Constructors => Classes.SelectMany(model => model.Constructors);

    public IEnumerable<MethodModel> Methods => Classes.SelectMany(model => model.Methods);

    public MethodModel Destructor => Classes.FirstOrDefault(model => model.Destructor != null)?.Destructor;

    public IEnumerable<AttributeModel> Attributes => Classes.SelectMany(model => model.Attributes);

    public IEnumerable<MetricModel> Metrics => Classes.SelectMany(model => model.Metrics);
}
