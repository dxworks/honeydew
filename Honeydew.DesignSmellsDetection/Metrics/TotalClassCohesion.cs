using DxWorks.ScriptBee.Plugins.Honeydew.Models;

namespace Honeydew.DesignSmellsDetection.Metrics;

public class TotalClassCohesion
{
    public static double Value(ClassModel t)
    {
        var methodsToConsider = MethodsToConsiderFor(t).ToList();

        var cohesivePairs = CohesivePairsFrom(PairsFrom(methodsToConsider)).ToList();
        return (double)cohesivePairs.Count
               / NumberOfPairsFor(methodsToConsider.Count);
    }

    private static HashSet<FieldModel> FieldsUsedFromParentClass(MemberModel method)
    {
        var methods = method switch
        {
            MethodModel methodModel => new List<MethodModel> { methodModel },
            PropertyModel propertyModel => propertyModel.Accessors.ToList(),
            _ => new List<MethodModel>()
        };

        var fieldsUsed = methods.SelectMany(m => m.FieldAccesses).Where(fa => fa.IsFrom(method.Entity)).Select(fa => fa.Field)
            .ToHashSet();
        return fieldsUsed;
    }

    private static IEnumerable<MemberModel> MethodsToConsiderFor(ClassModel type)
    {
        return type.MethodsAndProperties.Where(m => !m.IsAbstract);
    }

    private static IEnumerable<HashSet<MemberModel>> PairsFrom(IEnumerable<MemberModel> methods)
    {
        var methodList = methods.ToList();
        return methodList.Select(
                (m1, i) => methodList.Where((m2, j) => j > i).Select(m2 => new HashSet<MemberModel> { m1, m2 }))
            .SelectMany(_ => _);
    }

    private static IEnumerable<HashSet<MemberModel>> CohesivePairsFrom(IEnumerable<HashSet<MemberModel>> pairs)
    {
        return pairs.Where(p => FieldsUsedFromParentClass(p.First()).Overlaps(FieldsUsedFromParentClass(p.Last())));
    }

    private static int NumberOfPairsFor(int n)
    {
        return n * (n - 1) / 2;
    }
}
