using HoneydewModels.Types;

namespace HoneydewModels.CSharp
{
    public record UsingModel : IModelEntity, IImportType
    {
        public string Name { get; set; }

        public bool IsStatic { get; init; }

        public string Alias { get; init; } = "";

        public string AliasType { get; set; } = nameof(EAliasType.None);
    }
}
