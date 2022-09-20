namespace Honeydew.ScriptBeePlugin.Models;

public class MemberModel : ReferenceEntity
{
    public string Name { get; set; } = null!;
    public EntityModel Entity { get; set; } = null!;
    public string Modifier { get; set; } = "";
    public AccessModifier AccessModifier { get; set; }
    public IList<Modifier> Modifiers { get; set; } = new List<Modifier>();
    public IList<AttributeModel> Attributes { get; set; } = new List<AttributeModel>();

    public bool IsAbstract => Modifiers.Contains(Models.Modifier.Abstract);

    public bool IsPubliclyVisible =>
        AccessModifier == AccessModifier.Public && Entity.AccessModifier == AccessModifier.Public;

    public bool IsStatic => Modifiers.Contains(Models.Modifier.Static);

    public bool IsConst => Modifiers.Contains(Models.Modifier.Const);

    public bool IsReadonly => Modifiers.Contains(Models.Modifier.Readonly);

    public override string ToString()
    {
        return Name;
    }
}