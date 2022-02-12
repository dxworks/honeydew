namespace HoneydewModels.Reference;

public abstract record ClassOption
{
    public record Class(ClassModel ClassModel) : ClassOption;

    public record Delegate(DelegateModel DelegateModel) : ClassOption;
}
