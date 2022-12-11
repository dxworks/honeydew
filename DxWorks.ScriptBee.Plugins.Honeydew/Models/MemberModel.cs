namespace DxWorks.ScriptBee.Plugins.Honeydew.Models;

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
    
    public bool IsOverride => Modifiers.Contains(Models.Modifier.Override);

    public bool IsConst => Modifiers.Contains(Models.Modifier.Const);

    public bool IsReadonly => Modifiers.Contains(Models.Modifier.Readonly);

    public bool IsVirtual => Modifiers.Contains(Models.Modifier.Virtual);

    public bool IsProtected => new List<AccessModifier>
            { AccessModifier.PrivateProtected, AccessModifier.Protected, AccessModifier.ProtectedInternal }
        .Contains(AccessModifier);

    public override string ToString()
    {
        return Name;
    }
}
