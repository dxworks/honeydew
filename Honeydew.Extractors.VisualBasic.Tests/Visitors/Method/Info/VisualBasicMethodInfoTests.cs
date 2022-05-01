using Honeydew.Extractors.Visitors;
using Honeydew.Extractors.VisualBasic.Visitors.Concrete;
using Honeydew.Extractors.VisualBasic.Visitors.Setters;
using Honeydew.Logging;
using Honeydew.Models.Types;
using Honeydew.Models.VisualBasic;
using Moq;
using Xunit;

namespace Honeydew.Extractors.VisualBasic.Tests.Visitors.Method.Info;

public class VisualBasicMethodInfoTests
{
    private readonly VisualBasicFactExtractor _factExtractor;
    private readonly Mock<ILogger> _loggerMock = new();

    public VisualBasicMethodInfoTests()
    {
        var methodSetterVisitor = new VisualBasicMethodSetterVisitor(_loggerMock.Object,
            new List<ITypeVisitor<IMethodType>>
            {
                new MethodInfoVisitor(),
                new VisualBasicParameterSetterVisitor(_loggerMock.Object, new List<ITypeVisitor<IParameterType>>
                {
                    new ParameterInfoVisitor()
                }),
                new VisualBasicReturnValueSetterVisitor(_loggerMock.Object, new List<ITypeVisitor<IReturnValueType>>
                {
                    new ReturnValueInfoVisitor()
                })
            });
        var compositeVisitor = new VisualBasicCompilationUnitCompositeVisitor(_loggerMock.Object,
            new List<ITypeVisitor<ICompilationUnitType>>
            {
                new VisualBasicClassSetterVisitor(_loggerMock.Object, new List<ITypeVisitor<IMembersClassType>>
                {
                    new BaseInfoClassVisitor(),
                    methodSetterVisitor
                }),
                new VisualBasicInterfaceSetterVisitor(_loggerMock.Object, new List<ITypeVisitor<IMembersClassType>>
                {
                    new BaseInfoClassVisitor(),
                    methodSetterVisitor
                }),
                new VisualBasicStructureSetterVisitor(_loggerMock.Object, new List<ITypeVisitor<IMembersClassType>>
                {
                    new BaseInfoClassVisitor(),
                    methodSetterVisitor
                }),
            });

        _factExtractor = new VisualBasicFactExtractor(compositeVisitor);
    }


    [Theory]
    [FilePath("TestData/ClassWithMethod.txt")]
    [FilePath("TestData/StructureWithMethod.txt")]
    public async Task Extract_ShouldHaveMethodInfo_WhenProvidedWithMethods(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());

        var classType = (VisualBasicClassModel)compilationUnitType.ClassTypes[0];
        Assert.Single(compilationUnitType.ClassTypes);
        Assert.Equal("Customer", classType.Name);
        Assert.Equal("", classType.ContainingClassName);

        Assert.Equal(2, classType.Methods.Count);
        var method1 = classType.Methods[0];
        Assert.Equal("GetSalary", method1.Name);
        Assert.Equal("Public", method1.AccessModifier);
        Assert.Equal("", method1.Modifier);
        Assert.Empty(method1.ParameterTypes);
        Assert.Equal("Decimal", method1.ReturnValue.Type.Name);
        Assert.Empty(method1.LocalFunctions);

        var method2 = classType.Methods[1];
        Assert.Equal("giveRaise", method2.Name);
        Assert.Equal("Public", method2.AccessModifier);
        Assert.Equal("", method2.Modifier);
        Assert.Equal(1, method2.ParameterTypes.Count);
        Assert.Equal("Double", method2.ParameterTypes[0].Type.Name);
        Assert.Equal("Void", method2.ReturnValue.Type.Name);
        Assert.Empty(method2.LocalFunctions);
    }

    [Theory]
    [FilePath("TestData/InterfaceWithMethod.txt")]
    public async Task Extract_ShouldHaveMethodInfo_WhenProvidedWithInterfaceWithMethods(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());

        var classType = (VisualBasicClassModel)compilationUnitType.ClassTypes[0];
        Assert.Single(compilationUnitType.ClassTypes);
        Assert.Equal("IUser", classType.Name);

        Assert.Equal(2, classType.Methods.Count);
        var method1 = classType.Methods[0];
        Assert.Equal("GetDetails", method1.Name);
        Assert.Equal("Public", method1.AccessModifier);
        Assert.Equal("MustOverride", method1.Modifier);
        Assert.Empty(method1.ParameterTypes);
        Assert.Equal("Void", method1.ReturnValue.Type.Name);
        Assert.Empty(method1.LocalFunctions);

        var method2 = classType.Methods[1];
        Assert.Equal("ThisFunc", method2.Name);
        Assert.Equal("Public", method2.AccessModifier);
        Assert.Equal("MustOverride", method2.Modifier);
        Assert.Equal(1, method2.ParameterTypes.Count);
        Assert.Equal("Integer", method2.ParameterTypes[0].Type.Name);
        Assert.Equal("ByVal", method2.ParameterTypes[0].Modifier);
        Assert.Equal("Integer", method2.ReturnValue.Type.Name);
        Assert.Empty(method2.LocalFunctions);
    }

    [Theory]
    [FilePath("TestData/ClassWithMethodsForCyclomaticComplexity.txt")]
    public async Task Extract_ShouldHaveCyclomaticComplexity_WhenProvidedWithClassWithMethods(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());

        var classType = (VisualBasicClassModel)compilationUnitType.ClassTypes[0];
        Assert.Single(compilationUnitType.ClassTypes);
        Assert.Equal("Module1.User", classType.Name);
        Assert.Equal("class", classType.ClassType);
        Assert.Equal(2, classType.Methods.Count);
        Assert.Equal("F", classType.Methods[0].Name);
        Assert.Equal(4, classType.Methods[0].CyclomaticComplexity);
        Assert.Equal("F2", classType.Methods[1].Name);
        Assert.Equal(4, classType.Methods[1].CyclomaticComplexity);
    }

    [Theory]
    [FilePath("TestData/InheritedClassWithMethods.txt")]
    public async Task Extract_ShouldHaveModifiers_WhenProvidedWithMethodsFromClassInheritance(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());
        
        var personClass = (VisualBasicClassModel)compilationUnitType.ClassTypes[0];
        Assert.Equal("Person", personClass.Name);
        Assert.Equal("MustInherit", personClass.Modifier);
        Assert.Equal(2, personClass.Methods.Count);
        Assert.Equal("PrintName", personClass.Methods[0].Name);
        Assert.Equal("MustOverride", personClass.Methods[0].Modifier);
        
        var customerClass = (VisualBasicClassModel)compilationUnitType.ClassTypes[1];
        Assert.Equal("Customer", customerClass.Name);
        Assert.Equal("", customerClass.Modifier);
        Assert.Equal("PrintName", customerClass.Methods[0].Name);
        Assert.Equal("Overrides", customerClass.Methods[0].Modifier);
    }
}
