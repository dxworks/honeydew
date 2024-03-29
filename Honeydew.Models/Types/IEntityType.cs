﻿namespace Honeydew.Models.Types;

public interface IEntityType : INamedType
{
    public GenericType FullType { get; set; }
    
    public bool IsExtern { get; set; }
}
