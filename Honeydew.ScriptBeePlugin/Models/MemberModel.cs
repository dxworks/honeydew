namespace Honeydew.ScriptBeePlugin.Models;

public class MemberModel : ReferenceEntity
{
    public string Name { get; set; }
    public EntityModel Entity { get; set; }
    public string Modifier { get; set; } = "";
    public AccessModifier AccessModifier { get; set; }
    public IList<Modifier> Modifiers { get; set; } = new List<Modifier>();
    public IList<AttributeModel> Attributes { get; set; } = new List<AttributeModel>();

    public bool IsAbstract => Modifiers.Contains(Models.Modifier.Abstract);

    public bool IsPubliclyVisible =>
        AccessModifier == AccessModifier.Public && Entity.AccessModifier == AccessModifier.Public;

    public override string ToString()
    {
        return Name;
    }
}