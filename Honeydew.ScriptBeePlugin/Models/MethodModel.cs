namespace Honeydew.ScriptBeePlugin.Models;

public class MethodModel : MemberModel
{
    public MethodType Type { get; set; }

    public MethodModel? ContainingMethod { get; set; }

    public PropertyModel? ContainingProperty { get; set; }

    public ReturnValueModel? ReturnValue { get; set; }

    public IList<ParameterModel> Parameters { get; set; } = new List<ParameterModel>();

    public IList<GenericParameterModel> GenericParameters { get; set; } = new List<GenericParameterModel>();

    public IList<MethodCall> OutgoingCalls { get; set; } = new List<MethodCall>();

    public IList<MethodCall> IncomingCalls { get; set; } = new List<MethodCall>();

    public IList<FieldAccess> FieldAccesses { get; set; } = new List<FieldAccess>();

    public IEnumerable<MethodCall> ExternalOutgoingCalls =>
        _externalOutgoingCalls ??= OutgoingCalls.Where(call => call.Called is { Entity.IsExternal: true });

    public IEnumerable<MethodCall> InternalOutgoingCalls =>
        _internalOutgoingCalls ??= OutgoingCalls.Where(call => call.Called is { Entity.IsInternal: true });

    public IEnumerable<FieldAccess> ExternalFieldAccesses =>
        _externalFieldAccesses ??= FieldAccesses.Where(call => call.Field is { Entity.IsExternal: true });

    public IEnumerable<FieldAccess> InternalFieldAccesses =>
        _internalFieldAccesses ??= FieldAccesses.Where(call => call.Field is { Entity.IsInternal: true });

    public IList<MethodModel> LocalFunctions { get; set; } = new List<MethodModel>();

    public IList<LocalVariableModel> LocalVariables { get; set; } = new List<LocalVariableModel>();

    public LinesOfCode LinesOfCode { get; set; }

    public int CyclomaticComplexity { get; set; }

    public IDictionary<string, double> Metrics { get; set; } = new Dictionary<string, double>();

    private IEnumerable<MethodCall>? _externalOutgoingCalls;

    private IEnumerable<MethodCall>? _internalOutgoingCalls;

    private IEnumerable<FieldAccess>? _externalFieldAccesses;

    private IEnumerable<FieldAccess>? _internalFieldAccesses;
}
