﻿namespace Honeydew.Models.Types;

public interface INamedType : IType
{
    public string Name { get; set; }
}
