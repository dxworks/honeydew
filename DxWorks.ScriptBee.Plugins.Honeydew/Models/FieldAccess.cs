namespace DxWorks.ScriptBee.Plugins.Honeydew.Models;

public class FieldAccess : ReferenceEntity
{
    public FieldModel Field { get; set; }

    public MethodModel Caller { get; set; }

    public EntityType AccessEntityType { get; set; }
    
    public AccessKind AccessKind { get; set; }
}
