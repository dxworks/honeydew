namespace HoneydewModels.Reference;

public class FieldAccess : ReferenceEntity
{
    public FieldModel Field { get; set; }

    public MethodModel Caller { get; set; }

    public AccessKind AccessKind { get; set; }
}
