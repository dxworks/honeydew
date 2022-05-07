using Honeydew.Extractors.Visitors;
using Honeydew.Extractors.VisualBasic.Visitors.Concrete;
using Honeydew.Extractors.VisualBasic.Visitors.Setters;
using Honeydew.Logging;
using Honeydew.Models.Types;
using Honeydew.Models.VisualBasic;
using Moq;
using Xunit;

namespace Honeydew.Extractors.VisualBasic.Tests.Visitors.Delegate.Info;

public class VisualBasicDelegateInfoTests
{
    private readonly VisualBasicFactExtractor _factExtractor;
    private readonly Mock<ILogger> _loggerMock = new();

    public VisualBasicDelegateInfoTests()
    {
        var compositeVisitor = new VisualBasicCompilationUnitCompositeVisitor(_loggerMock.Object,
            new List<ITypeVisitor<ICompilationUnitType>>
            {
                new VisualBasicClassSetterVisitor(_loggerMock.Object, new List<ITypeVisitor<IMembersClassType>>
                {
                    new BaseInfoClassVisitor()
                }),
                new VisualBasicDelegateSetterVisitor(_loggerMock.Object, new List<ITypeVisitor<IDelegateType>>
                {
                    new BaseInfoDelegateVisitor(),
                    new VisualBasicParameterSetterVisitor(_loggerMock.Object, new List<ITypeVisitor<IParameterType>>
                    {
                        new ParameterInfoVisitor()
                    }),
                    new VisualBasicReturnValueSetterVisitor(_loggerMock.Object, new List<ITypeVisitor<IReturnValueType>>
                    {
                        new ReturnValueInfoVisitor()
                    })
                }),
            });

        _factExtractor = new VisualBasicFactExtractor(compositeVisitor);
    }

    [Theory]
    [FilePath("TestData/BasicDelegate.txt")]
    public async Task Extract_ShouldHaveInfo_WhenProvidedWithDelegate(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());

        var delegateModel = (VisualBasicDelegateModel)compilationUnitType.ClassTypes[0];
        Assert.Single(compilationUnitType.ClassTypes);
        Assert.Equal("Module1.SampleDelegate", delegateModel.Name);
        Assert.Equal("delegate", delegateModel.ClassType);
        Assert.Equal("", delegateModel.ContainingClassName);
        Assert.Equal("Global", delegateModel.ContainingNamespaceName);
        Assert.Equal("Module1", delegateModel.ContainingModuleName);
        Assert.Equal("", delegateModel.Modifier);
        Assert.Equal("Public", delegateModel.AccessModifier);
        Assert.Single(delegateModel.BaseTypes);
        Assert.Equal("System.Delegate", delegateModel.BaseTypes[0].Type.Name);
        Assert.Equal("System.Delegate", delegateModel.BaseTypes[0].Type.FullType.Name);
        Assert.Equal("class", delegateModel.BaseTypes[0].Kind);
        Assert.Equal(2, delegateModel.ParameterTypes.Count);
        Assert.Equal("Integer", delegateModel.ParameterTypes[0].Type.Name);
        Assert.Equal("ByVal", delegateModel.ParameterTypes[0].Modifier);
        Assert.Equal("Integer", delegateModel.ParameterTypes[1].Type.Name);
        Assert.Equal("ByVal", delegateModel.ParameterTypes[1].Modifier);
    }

    [Theory]
    [FilePath("TestData/BasicDelegateWithReturnType.txt")]
    public async Task Extract_ShouldHaveInfo_WhenProvidedWithDelegateWithReturnType(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());

        var delegateModel = (VisualBasicDelegateModel)compilationUnitType.ClassTypes[0];
        Assert.Single(compilationUnitType.ClassTypes);
        Assert.Equal("Module1.SampleDelegate", delegateModel.Name);
        Assert.Equal("delegate", delegateModel.ClassType);
        Assert.Equal("Public", delegateModel.AccessModifier);
        Assert.Equal("Integer", delegateModel.ReturnValue.Type.Name);
    }

    [Theory]
    [FilePath("TestData/AccessModifier/PublicDelegate.txt", "Public")]
    [FilePath("TestData/AccessModifier/PrivateDelegate.txt", "Private")]
    [FilePath("TestData/AccessModifier/ProtectedDelegate.txt", "Protected")]
    [FilePath("TestData/AccessModifier/FriendDelegate.txt", "Friend")]
    [FilePath("TestData/AccessModifier/ProtectedFriendDelegate.txt", "Protected Friend")]
    [FilePath("TestData/AccessModifier/PrivateProtectedDelegate.txt", "Private Protected")]
    public async Task Extract_ShouldHaveAccessModifiers_WhenProvidedWithDelegateWithAccessModifier(string filePath,
        string accessModifier)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());

        var delegateModel = compilationUnitType.ClassTypes[0];
        Assert.Single(compilationUnitType.ClassTypes);
        Assert.Equal("Module1.Delegate1", delegateModel.Name);
        Assert.Equal(accessModifier, delegateModel.AccessModifier);
    }

    [Theory]
    [FilePath("TestData/NestedDelegate.txt")]
    public async Task Extract_ShouldHaveNestedClass_WhenProvidedWithClassWithinAnotherClass(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());

        Assert.Equal(2, compilationUnitType.ClassTypes.Count);

        var outerClass = compilationUnitType.ClassTypes[0];
        Assert.Equal("Namespace1.Module1.Class1", outerClass.Name);
        Assert.Equal("class", outerClass.ClassType);


        var delegateModel = (VisualBasicDelegateModel)compilationUnitType.ClassTypes[1];
        Assert.Equal("Namespace1.Module1.Class1.SampleDelegate", delegateModel.Name);
        Assert.Equal("delegate", delegateModel.ClassType);
        Assert.Equal("Namespace1.Module1.Class1", delegateModel.ContainingClassName);
        Assert.Equal("Namespace1", delegateModel.ContainingNamespaceName);
        Assert.Equal("Namespace1.Module1", delegateModel.ContainingModuleName);
        Assert.Equal("", delegateModel.Modifier);
        Assert.Equal("Friend", delegateModel.AccessModifier);
    }

    [Theory]
    [FilePath("TestData/DelegateInNamespace.txt")]
    public async Task Extract_ShouldHaveNamespace_WhenProvidedWithDelegateInNamespace(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());

        var delegateModel = (VisualBasicDelegateModel)compilationUnitType.ClassTypes[0];
        Assert.Single(compilationUnitType.ClassTypes);
        Assert.Equal("Namespace1.Module1.SampleDelegate", delegateModel.Name);
        Assert.Equal("Namespace1.Module1", delegateModel.ContainingModuleName);
        Assert.Equal("Namespace1", delegateModel.ContainingNamespaceName);
    }
}
