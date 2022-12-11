namespace DxWorks.ScriptBee.Plugins.Honeydew.Models;

public class NamespaceModel : ReferenceEntity
{
    public string FullName { get; set; } = "";

    public string Name { get; set; } = "";

    public NamespaceModel? Parent { get; set; }

    public IList<NamespaceModel> ChildNamespaces { get; set; } = new List<NamespaceModel>();

    public IList<EntityModel> Entities { get; set; } = new List<EntityModel>();
}
