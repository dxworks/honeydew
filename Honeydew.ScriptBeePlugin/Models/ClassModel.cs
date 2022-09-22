namespace Honeydew.ScriptBeePlugin.Models;

public class ClassModel : EntityModel
{
    public ClassType Type { get; set; }

    public bool IsPartial => Modifiers.Contains(Models.Modifier.Partial);

    public IList<ClassModel> Partials { get; set; } = new List<ClassModel>();

    public IList<GenericParameterModel> GenericParameters { get; set; } = new List<GenericParameterModel>();

    public IList<EntityType> BaseTypes { get; set; } = new List<EntityType>();

    public IList<FieldModel> Fields { get; set; } = new List<FieldModel>();

    public new IList<PropertyModel> Properties { get; set; } = new List<PropertyModel>();

    public IList<MethodModel> Methods { get; set; } = new List<MethodModel>();

    public IList<MethodModel> Constructors { get; set; } = new List<MethodModel>();

    public MethodModel? Destructor { get; set; }

    public IList<MemberModel> MethodsAndProperties
    {
        get
        {
            var methodsAndProperties = new List<MemberModel>();
            methodsAndProperties.AddRange(Methods);
            methodsAndProperties.AddRange(Properties);
            return methodsAndProperties;
        }
    }

    public IList<MemberModel> MethodsPropertiesAndConstructors
    {
        get
        {
            var methodsPropertiesAndConstructors = new List<MemberModel>();
            methodsPropertiesAndConstructors.AddRange(MethodsAndProperties);
            methodsPropertiesAndConstructors.AddRange(Constructors);
            methodsPropertiesAndConstructors.AddRange(Constructors);
            if (Destructor != null)
            {
                methodsPropertiesAndConstructors.Add(Destructor);
            }
            return methodsPropertiesAndConstructors;
        }
    }
    public IList<MemberModel> Members
    {
        get
        {
            var allMembers = new List<MemberModel>();
            allMembers.AddRange(MethodsPropertiesAndConstructors);
            allMembers.AddRange(Fields);

            return allMembers;
        }
    }
    public EntityType? BaseClass
    {
        get
        {
            // TODO: currently we are returning only internal base classes
            var baseClass = BaseTypes.Where(baseType => !baseType.IsExtern && baseType.Entity is ClassModel);
          
            return baseClass.SingleOrDefault();
        }
    }

    public bool Uses(MemberModel member)
    {
        var allMethods = new List<MethodModel>();
        allMethods.AddRange(Methods);
        allMethods.AddRange(Properties.SelectMany(p => p.Accessors));
        allMethods.AddRange(Constructors);
        if (Destructor != null)
        {
            allMethods.Add(Destructor);
        }

        foreach (var method in allMethods)
        {
            if (method.OutgoingCalls.Any(call => call.Called == member) ||
                method.FieldAccesses.Any(fa => fa.Field == member))
            {
                return true;
            }
        }

        return false;
    }
}
