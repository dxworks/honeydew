using Honeydew.Extractors.Visitors;
using Honeydew.Extractors.VisualBasic.Visitors.Concrete;
using Honeydew.Extractors.VisualBasic.Visitors.Setters;
using Honeydew.Logging;
using Honeydew.Models.Types;
using Honeydew.Models.VisualBasic;
using Moq;
using Xunit;

namespace Honeydew.Extractors.VisualBasic.Tests.Visitors.Class.Info;

public class VisualBasicClassInfoTests
{
    private readonly VisualBasicFactExtractor _factExtractor;
    private readonly Mock<ILogger> _loggerMock = new();

    public VisualBasicClassInfoTests()
    {
        var baseInfoClassVisitor = new BaseInfoClassVisitor();
        var compositeVisitor = new VisualBasicCompilationUnitCompositeVisitor(_loggerMock.Object,
            new List<ITypeVisitor<ICompilationUnitType>>
            {
                new VisualBasicClassSetterVisitor(_loggerMock.Object, new List<ITypeVisitor<IMembersClassType>>
                {
                    baseInfoClassVisitor
                }),
                new VisualBasicInterfaceSetterVisitor(_loggerMock.Object, new List<ITypeVisitor<IMembersClassType>>
                {
                    baseInfoClassVisitor
                }),
                new VisualBasicStructureSetterVisitor(_loggerMock.Object, new List<ITypeVisitor<IMembersClassType>>
                {
                    baseInfoClassVisitor
                })
            });

        _factExtractor = new VisualBasicFactExtractor(compositeVisitor);
    }

    [Theory]
    [FilePath("TestData/BasicClass.txt")]
    public async Task Extract_ShouldHaveInfo_WhenProvidedWithClass(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());

        var classType = (VisualBasicClassModel)compilationUnitType.ClassTypes[0];
        Assert.Single(compilationUnitType.ClassTypes);
        Assert.Equal("Users", classType.Name);
        Assert.Equal("class", classType.ClassType);
        Assert.Equal("", classType.ContainingClassName);
        Assert.Equal("Global", classType.ContainingNamespaceName);
        Assert.Equal("", classType.ContainingModuleName);
        Assert.Equal("", classType.Modifier);
        Assert.Equal("Public", classType.AccessModifier);
    }

    [Theory]
    [FilePath("TestData/BasicInterface.txt")]
    public async Task Extract_ShouldHaveInfo_WhenProvidedWithInterface(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());

        var classType = (VisualBasicClassModel)compilationUnitType.ClassTypes[0];
        Assert.Single(compilationUnitType.ClassTypes);
        Assert.Equal("Module1.IUser", classType.Name);
        Assert.Equal("interface", classType.ClassType);
        Assert.Equal("", classType.ContainingClassName);
        Assert.Equal("Global", classType.ContainingNamespaceName);
        Assert.Equal("Module1", classType.ContainingModuleName);
        Assert.Equal("", classType.Modifier);
        Assert.Equal("Friend", classType.AccessModifier);
    }

    [Theory]
    [FilePath("TestData/BasicStructure.txt")]
    public async Task Extract_ShouldHaveInfo_WhenProvidedWithStructure(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());

        var classType = (VisualBasicClassModel)compilationUnitType.ClassTypes[0];
        Assert.Single(compilationUnitType.ClassTypes);
        Assert.Equal("Module1.User", classType.Name);
        Assert.Equal("structure", classType.ClassType);
        Assert.Equal("", classType.ContainingClassName);
        Assert.Equal("Global", classType.ContainingNamespaceName);
        Assert.Equal("Module1", classType.ContainingModuleName);
        Assert.Equal("", classType.Modifier);
        Assert.Equal("Public", classType.AccessModifier);
    }

    [Theory]
    [FilePath("TestData/AccessModifier/PublicClass.txt", "Public")]
    [FilePath("TestData/AccessModifier/PrivateClass.txt", "Private")]
    [FilePath("TestData/AccessModifier/ProtectedClass.txt", "Protected")]
    [FilePath("TestData/AccessModifier/FriendClass.txt", "Friend")]
    [FilePath("TestData/AccessModifier/ProtectedFriendClass.txt", "Protected Friend")]
    [FilePath("TestData/AccessModifier/PrivateProtectedClass.txt", "Private Protected")]
    public async Task Extract_ShouldHaveAccessModifiers_WhenProvidedWithClassWithAccessModifier(string filePath,
        string accessModifier)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());

        var classType = compilationUnitType.ClassTypes[0];
        Assert.Single(compilationUnitType.ClassTypes);
        Assert.Equal("Module1.Class1", classType.Name);
        Assert.Equal(accessModifier, classType.AccessModifier);
    }

    [Theory]
    [FilePath("TestData/NestedClass.txt")]
    public async Task Extract_ShouldHaveNestedClass_WhenProvidedWithClassWithinAnotherClass(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());

        Assert.Equal(2, compilationUnitType.ClassTypes.Count);

        var outerClass = (VisualBasicClassModel)compilationUnitType.ClassTypes[0];
        Assert.Equal("Exercise.Outside", outerClass.Name);
        Assert.Equal("class", outerClass.ClassType);
        Assert.Equal("", outerClass.ContainingClassName);
        Assert.Equal("Global", outerClass.ContainingNamespaceName);
        Assert.Equal("Exercise", outerClass.ContainingModuleName);
        Assert.Equal("", outerClass.Modifier);
        Assert.Equal("Public", outerClass.AccessModifier);

        var innerClass = (VisualBasicClassModel)compilationUnitType.ClassTypes[1];
        Assert.Equal("Exercise.Outside.Inside", innerClass.Name);
        Assert.Equal("class", innerClass.ClassType);
        Assert.Equal("Exercise.Outside", innerClass.ContainingClassName);
        Assert.Equal("Global", innerClass.ContainingNamespaceName);
        Assert.Equal("Exercise", innerClass.ContainingModuleName);
        Assert.Equal("", innerClass.Modifier);
        Assert.Equal("Public", innerClass.AccessModifier);
    }

    [Theory]
    [FilePath("TestData/Namespace/ClassInNamespace.txt", "Class1")]
    [FilePath("TestData/Namespace/InterfaceInNamespace.txt", "Interface1")]
    [FilePath("TestData/Namespace/StructureInNamespace.txt", "Structure1")]
    public async Task Extract_ShouldHaveNamespace_WhenProvidedWithTypeInNamespace(string filePath,
        string type)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());

        var classType = (VisualBasicClassModel)compilationUnitType.ClassTypes[0];
        Assert.Single(compilationUnitType.ClassTypes);
        Assert.Equal($"SpecialSpace.System.Module1.{type}", classType.Name);
        Assert.Equal("SpecialSpace.System.Module1", classType.ContainingModuleName);
        Assert.Equal("SpecialSpace.System", classType.ContainingNamespaceName);
    }

    [Theory]
    [FilePath("TestData/Modifier/NonInheritableClass.txt")]
    public async Task Extract_ShouldHaveModifier_WhenProvidedClassWithModifier(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());

        var classType = (VisualBasicClassModel)compilationUnitType.ClassTypes[0];
        Assert.Single(compilationUnitType.ClassTypes);
        Assert.Equal("MyStaticClass", classType.Name);
        Assert.Equal("NotInheritable", classType.Modifier);
        Assert.Equal("Friend", classType.AccessModifier);
    }
}
