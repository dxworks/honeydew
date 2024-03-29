﻿using Honeydew.Models.Types;

namespace Honeydew.Models.VisualBasic;

public record VisualBasicImportModel : IImportType
{
    public string Name { get; set; } = "";

    public bool IsStatic { get; init; }

    public string Alias { get; init; } = "";

    public string AliasType { get; set; } = nameof(EAliasType.None);
}
