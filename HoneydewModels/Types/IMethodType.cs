﻿namespace HoneydewModels.Types;

public interface IMethodType : IMethodSkeletonType, ITypeWithGenericParameters, ITypeWithLocalFunctions
{

    public IReturnValueType ReturnValue { get; set; }
}
