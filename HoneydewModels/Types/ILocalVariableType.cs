namespace HoneydewModels.Types
{
    public interface ILocalVariableType : INullableType
    {
        public IEntityType Type { get; set; }

        public string Modifier { get; set; }
    }
}
