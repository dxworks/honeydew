using Honeydew.ScriptBeePlugin.Models;

namespace Honeydew.DesignSmellsDetection.Metrics;

public static class CouplingExtensions
{
    public static IEnumerable<MemberModel> CoupledMethodsAndProperties(this MethodModel method)
    {
        var accessedProperties = method.InternalFieldAccesses
            .Where(fieldAccess => !fieldAccess.IsFrom(method.Entity) && fieldAccess.Field is PropertyModel)
            .Select(fa => fa.Field).ToHashSet();
        var accessedMethods = method.InternalOutgoingCalls.Where(methodCall => !methodCall.IsFrom(method.Entity))
            .Select(methodCall => methodCall.Called).ToHashSet();

        var accessedMethodsAndProperties = new List<MemberModel>();
        accessedMethodsAndProperties.AddRange(accessedProperties);
        accessedMethodsAndProperties.AddRange(accessedMethods);

        return accessedMethodsAndProperties;
    }

    public static IEnumerable<EntityModel> ProvidersForCoupledMethods(this MethodModel method)
    {
        return method.CoupledMethodsAndProperties().Select(m => m.Entity).ToHashSet();
    }

    public static IList<Tuple<EntityModel, IList<MemberModel>>> CouplingIntensityPerProvider(this MethodModel method)
    {
        var result = method.CoupledMethodsAndProperties().GroupBy(m => m.Entity, m => m,
            (key, g) => new Tuple<EntityModel, IList<MemberModel>>(key, g.ToList())).ToList();

        return result;
    }
}
