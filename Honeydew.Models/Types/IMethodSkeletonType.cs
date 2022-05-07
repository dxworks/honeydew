namespace Honeydew.Models.Types;

public interface IMethodSkeletonType : IMethodSignatureType, ICallingMethodsType, IContainedTypeWithAccessedFields,
    ITypeWithModifiers, ITypeWithCyclomaticComplexity, ITypeWithAttributes, ITypeWithMetrics, ITypeWithLinesOfCode,
    ITypeWithLocalVariables
{
}
