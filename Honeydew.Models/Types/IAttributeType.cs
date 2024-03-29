﻿namespace Honeydew.Models.Types;

public interface IAttributeType : IMethodSignatureType
{
    public IEntityType Type { get; set; }

    public string Target { get; set; }
}
