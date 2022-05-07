using Honeydew.Extractors.Visitors;
using Honeydew.Extractors.VisualBasic.Visitors.Concrete;
using Honeydew.Extractors.VisualBasic.Visitors.Setters;
using Honeydew.Logging;
using Honeydew.Models.Types;
using Honeydew.Models.VisualBasic;
using Moq;
using Xunit;

namespace Honeydew.Extractors.VisualBasic.Tests.Visitors.Method.GenericType;

public class VisualBasicGenericMethodTests
{
    private readonly VisualBasicFactExtractor _factExtractor;
    private readonly Mock<ILogger> _loggerMock = new();

    public VisualBasicGenericMethodTests()
    {
        var genericParameterSetterVisitor = new VisualBasicGenericParameterSetterVisitor(_loggerMock.Object,
            new List<ITypeVisitor<IGenericParameterType>>
            {
                new GenericParameterInfoVisitor()
            });
        var visualBasicMethodSetterVisitor = new VisualBasicMethodSetterVisitor(_loggerMock.Object,
            new List<ITypeVisitor<IMethodType>>
            {
                new MethodInfoVisitor(),
                genericParameterSetterVisitor,
            });
        var compositeVisitor = new VisualBasicCompilationUnitCompositeVisitor(_loggerMock.Object,
            new List<ITypeVisitor<ICompilationUnitType>>
            {
                new VisualBasicClassSetterVisitor(_loggerMock.Object, new List<ITypeVisitor<IMembersClassType>>
                {
                    new BaseInfoClassVisitor(),
                    visualBasicMethodSetterVisitor,
                }),
                new VisualBasicInterfaceSetterVisitor(_loggerMock.Object, new List<ITypeVisitor<IMembersClassType>>
                {
                    new BaseInfoClassVisitor(),
                    visualBasicMethodSetterVisitor,
                })
            });

        _factExtractor = new VisualBasicFactExtractor(compositeVisitor);
    }

    [Theory]
    [FilePath("TestData/GenericMethodWithPredefinedConstrains.txt")]
    public async Task Extract_ShouldHaveGenericTypesWithPredefinedConstrains_WhenProvidedWithClass(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());
        var classTypes = compilationUnitType.ClassTypes;

        var classModel = (VisualBasicClassModel)classTypes[0];
        var methodModel = classModel.Methods[0];

        Assert.Equal("Method", methodModel.Name);
        Assert.Equal(4, methodModel.GenericParameters.Count);
        Assert.Equal("tT", methodModel.GenericParameters[0].Name);
        Assert.Equal(1, methodModel.GenericParameters[0].Constraints.Count);
        Assert.Equal("Structure", methodModel.GenericParameters[0].Constraints[0].Name);
        Assert.False(methodModel.GenericParameters[0].Constraints[0].FullType.IsNullable);

        Assert.Equal("TK", methodModel.GenericParameters[1].Name);
        Assert.Equal(1, methodModel.GenericParameters[1].Constraints.Count);
        Assert.Equal("Class", methodModel.GenericParameters[1].Constraints[0].Name);
        Assert.Equal("Class", methodModel.GenericParameters[1].Constraints[0].FullType.Name);
        Assert.False(methodModel.GenericParameters[1].Constraints[0].FullType.IsNullable);

        Assert.Equal("tTR", methodModel.GenericParameters[2].Name);
        Assert.Equal(1, methodModel.GenericParameters[2].Constraints.Count);
        Assert.Equal("notnull", methodModel.GenericParameters[2].Constraints[0].Name);

        Assert.Equal("tTP", methodModel.GenericParameters[3].Name);
        Assert.Equal(1, methodModel.GenericParameters[3].Constraints.Count);
        Assert.Equal("Namespace1.IInterface2(Of tT As Structure, Namespace1.IInterface2(Of tT, TK))",
            methodModel.GenericParameters[3].Constraints[0].Name);
        Assert.Equal("Namespace1.IInterface2", methodModel.GenericParameters[3].Constraints[0].FullType.Name);
        Assert.Equal(2, methodModel.GenericParameters[3].Constraints[0].FullType.ContainedTypes.Count);
        Assert.Equal("tT As Structure", methodModel.GenericParameters[3].Constraints[0].FullType.ContainedTypes[0].Name);
        Assert.Equal("Namespace1.IInterface2",
            methodModel.GenericParameters[3].Constraints[0].FullType.ContainedTypes[1].Name);
        Assert.Equal(2,
            methodModel.GenericParameters[3].Constraints[0].FullType.ContainedTypes[1].ContainedTypes.Count);
        Assert.Equal("tT",
            methodModel.GenericParameters[3].Constraints[0].FullType.ContainedTypes[1].ContainedTypes[0].Name);
        Assert.Equal("TK",
            methodModel.GenericParameters[3].Constraints[0].FullType.ContainedTypes[1].ContainedTypes[1].Name);
    }

    [Theory]
    [FilePath("TestData/GenericMethodWithMultipleConstrains.txt")]
    public async Task Extract_ShouldHaveGenericTypesWithMultipleConstrains_WhenProvidedWithClass(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());
        var classTypes = compilationUnitType.ClassTypes;

        var classModel = (VisualBasicClassModel)classTypes[0];
        var methodModel = classModel.Methods[0];

        Assert.Equal("Method", methodModel.Name);
        Assert.Equal(3, methodModel.GenericParameters.Count);
        Assert.Equal("T", methodModel.GenericParameters[0].Name);
        Assert.Equal(2, methodModel.GenericParameters[0].Constraints.Count);
        Assert.Equal("Namespace1.IInterface", methodModel.GenericParameters[0].Constraints[0].Name);
        Assert.Equal("Namespace1.IInterface2(Of TK, TR)", methodModel.GenericParameters[0].Constraints[1].Name);
        Assert.Equal("Namespace1.IInterface2", methodModel.GenericParameters[0].Constraints[1].FullType.Name);
        Assert.Equal(2, methodModel.GenericParameters[0].Constraints[1].FullType.ContainedTypes.Count);
        Assert.Equal("TK", methodModel.GenericParameters[0].Constraints[1].FullType.ContainedTypes[0].Name);
        Assert.Equal("TR", methodModel.GenericParameters[0].Constraints[1].FullType.ContainedTypes[1].Name);
    }
}
