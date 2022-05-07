using Honeydew.Extractors.Visitors;
using Honeydew.Extractors.VisualBasic.Visitors.Concrete;
using Honeydew.Extractors.VisualBasic.Visitors.Setters;
using Honeydew.Logging;
using Honeydew.Models.Types;
using Honeydew.Models.VisualBasic;
using Moq;
using Xunit;

namespace Honeydew.Extractors.VisualBasic.Tests.Visitors.Enum.Info;

public class VisualBasicEnumInfoTests
{
    private readonly VisualBasicFactExtractor _factExtractor;
    private readonly Mock<ILogger> _loggerMock = new();

    public VisualBasicEnumInfoTests()
    {
        var compositeVisitor = new VisualBasicCompilationUnitCompositeVisitor(_loggerMock.Object,
            new List<ITypeVisitor<ICompilationUnitType>>
            {
                new VisualBasicClassSetterVisitor(_loggerMock.Object, new List<ITypeVisitor<IMembersClassType>>
                {
                    new BaseInfoClassVisitor()
                }),
                new VisualBasicEnumSetterVisitor(_loggerMock.Object, new List<ITypeVisitor<IEnumType>>
                {
                    new BaseInfoEnumVisitor()
                }),
            });

        _factExtractor = new VisualBasicFactExtractor(compositeVisitor);
    }

    [Theory]
    [FilePath("TestData/BasicEnum.txt")]
    public async Task Extract_ShouldHaveInfo_WhenProvidedWithEnum(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());

        var enumModel = (VisualBasicEnumModel)compilationUnitType.ClassTypes[0];
        Assert.Single(compilationUnitType.ClassTypes);
        Assert.Equal("SecurityLevel", enumModel.Name);
        Assert.Equal("enum", enumModel.ClassType);
        Assert.Equal("", enumModel.ContainingClassName);
        Assert.Equal("Global", enumModel.ContainingNamespaceName);
        Assert.Equal("", enumModel.ContainingModuleName);
        Assert.Equal("", enumModel.Modifier);
        Assert.Equal("Friend", enumModel.AccessModifier);
        Assert.Equal("Int", enumModel.Type);
        Assert.Single(enumModel.BaseTypes);
        Assert.Equal("System.Enum", enumModel.BaseTypes[0].Type.Name);
        Assert.Equal("System.Enum", enumModel.BaseTypes[0].Type.FullType.Name);
        Assert.Equal("class", enumModel.BaseTypes[0].Kind);
    }

    [Theory]
    [FilePath("TestData/AccessModifier/PublicEnum.txt", "Public")]
    [FilePath("TestData/AccessModifier/PrivateEnum.txt", "Private")]
    [FilePath("TestData/AccessModifier/ProtectedEnum.txt", "Protected")]
    [FilePath("TestData/AccessModifier/FriendEnum.txt", "Friend")]
    [FilePath("TestData/AccessModifier/ProtectedFriendEnum.txt", "Protected Friend")]
    [FilePath("TestData/AccessModifier/PrivateProtectedEnum.txt", "Private Protected")]
    public async Task Extract_ShouldHaveAccessModifiers_WhenProvidedWithEnumWithAccessModifier(string filePath,
        string accessModifier)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());

        var enumModel = compilationUnitType.ClassTypes[0];
        Assert.Single(compilationUnitType.ClassTypes);
        Assert.Equal("Module1.Enum1", enumModel.Name);
        Assert.Equal(accessModifier, enumModel.AccessModifier);
    }

    [Theory]
    [FilePath("TestData/NestedEnum.txt")]
    public async Task Extract_ShouldHaveNestedClass_WhenProvidedWithClassWithinAnotherClass(string filePath)
    {
        var enumModel = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());

        Assert.Equal(2, enumModel.ClassTypes.Count);

        var outerClass = enumModel.ClassTypes[0];
        Assert.Equal("Module1.Egg", outerClass.Name);
        Assert.Equal("class", outerClass.ClassType);


        var innerEnum = (VisualBasicEnumModel)enumModel.ClassTypes[1];
        Assert.Equal("Module1.Egg.EggSizeEnum", innerEnum.Name);
        Assert.Equal("enum", innerEnum.ClassType);
        Assert.Equal("Module1.Egg", innerEnum.ContainingClassName);
        Assert.Equal("Global", innerEnum.ContainingNamespaceName);
        Assert.Equal("Module1", innerEnum.ContainingModuleName);
        Assert.Equal("", innerEnum.Modifier);
        Assert.Equal("Friend", innerEnum.AccessModifier);
    }

    [Theory]
    [FilePath("TestData/EnumInNamespace.txt")]
    public async Task Extract_ShouldHaveNamespace_WhenProvidedWithEnumInNamespace(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());

        var enumModel = (VisualBasicEnumModel)compilationUnitType.ClassTypes[0];
        Assert.Single(compilationUnitType.ClassTypes);
        Assert.Equal("SpecialSpace.System.Module1.Enum1", enumModel.Name);
        Assert.Equal("SpecialSpace.System.Module1", enumModel.ContainingModuleName);
        Assert.Equal("SpecialSpace.System", enumModel.ContainingNamespaceName);
    }

    [Theory]
    [FilePath("TestData/EnumWithExplicitType.txt")]
    public async Task Extract_ShouldHaveExplicitType_WhenProvidedWithEnumWithExplicitType(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());

        var enumModel = (VisualBasicEnumModel)compilationUnitType.ClassTypes[0];
        Assert.Single(compilationUnitType.ClassTypes);
        Assert.Equal("Module1.MyEnum", enumModel.Name);
        Assert.Equal("Byte", enumModel.Type);
    }
}
