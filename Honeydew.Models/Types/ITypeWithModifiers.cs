﻿namespace Honeydew.Models.Types;

public interface ITypeWithModifiers : IType
{
    public string AccessModifier { get; set; }

    public string Modifier { get; set; }
}
