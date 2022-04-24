using System.Collections.Generic;
using Honeydew.Extractors.CSharp.Visitors.Concrete;
using Honeydew.Extractors.CSharp.Visitors.Setters;
using Honeydew.Extractors.Visitors;
using Honeydew.Logging;
using Honeydew.Models.CSharp;
using Honeydew.Models.Types;
using Moq;
using Xunit;

namespace Honeydew.Extractors.CSharp.Tests.Visitors.Enum.Info;

public class CSharpEnumInfoTests
{
    private readonly CSharpFactExtractor _factExtractor;
    private readonly Mock<ILogger> _loggerMock = new();
    private readonly CSharpSyntacticModelCreator _syntacticModelCreator = new();
    private readonly CSharpSemanticModelCreator _semanticModelCreator = new(new CSharpCompilationMaker());

    public CSharpEnumInfoTests()
    {
        var compositeVisitor = new CSharpCompilationUnitCompositeVisitor(_loggerMock.Object,
            new List<ITypeVisitor<ICompilationUnitType>>
            {
                new CSharpEnumSetterCompilationUnitVisitor(_loggerMock.Object, new List<ITypeVisitor<IEnumType>>
                {
                    new BaseInfoEnumVisitor(),
                    new CSharpEnumLabelsSetterVisitor(_loggerMock.Object, new List<ITypeVisitor<IEnumLabelType>>
                    {
                        new BasicEnumLabelInfoVisitor()
                    })
                })
            });


        _factExtractor = new CSharpFactExtractor(compositeVisitor);
    }

    [Theory]
    [FileData("TestData/BasicEnum.txt")]
    public void Extract_ShouldBasicInfo_WhenProvidedWithEnum(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var enumModel = (EnumModel)classTypes[0];

        Assert.Equal("Enums.MyEnum", enumModel.Name);
        Assert.Equal("Enums", enumModel.ContainingNamespaceName);
        Assert.Equal("enum", enumModel.ClassType);
        Assert.Single(enumModel.BaseTypes);
        Assert.Equal("class", enumModel.BaseTypes[0].Kind);
        Assert.Equal("System.Enum", enumModel.BaseTypes[0].Type.Name);
        Assert.Equal("System.Enum", enumModel.BaseTypes[0].Type.Name);
        Assert.Equal("", enumModel.Modifier);
        Assert.Equal("public", enumModel.AccessModifier);
        Assert.Equal("int", enumModel.Type);
        Assert.Equal(6, enumModel.Labels.Count);
        Assert.Equal("A", enumModel.Labels[0].Name);
        Assert.Equal("B", enumModel.Labels[1].Name);
        Assert.Equal("C", enumModel.Labels[2].Name);
        Assert.Equal("D", enumModel.Labels[3].Name);
        Assert.Equal("E", enumModel.Labels[4].Name);
        Assert.Equal("F", enumModel.Labels[5].Name);
    }

    [Theory]
    [FileData("TestData/EnumWithOtherType.txt")]
    public void Extract_ShouldHaveOtherType_WhenProvidedWithEnumWithOtherType(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var enumModel = (EnumModel)classTypes[0];

        Assert.Equal("Enums.MyEnum", enumModel.Name);
        Assert.Equal("Enums", enumModel.ContainingNamespaceName);
        Assert.Equal("enum", enumModel.ClassType);
        Assert.Equal("", enumModel.Modifier);
        Assert.Equal("public", enumModel.AccessModifier);
        Assert.Equal("ushort", enumModel.Type);
        Assert.Equal(3, enumModel.Labels.Count);
        Assert.Equal("A", enumModel.Labels[0].Name);
        Assert.Equal("B", enumModel.Labels[1].Name);
        Assert.Equal("C", enumModel.Labels[2].Name);
    }

    [Theory]
    [FileData("TestData/EnumInInnerClass.txt")]
    public void Extract_ShouldContainingClass_WhenProvidedWithEnumInInnerClass(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(2, classTypes.Count);

        var enumModel1 = (EnumModel)classTypes[0];
        var enumModel2 = (EnumModel)classTypes[1];

        Assert.Equal("Enums.Class1.MyEnum", enumModel1.Name);
        Assert.Equal("Enums", enumModel1.ContainingNamespaceName);
        Assert.Equal("Enums.Class1", enumModel1.ContainingClassName);
        Assert.Equal("long", enumModel1.Type);

        Assert.Equal("Enums.Class1.Class2.Enum1", enumModel2.Name);
        Assert.Equal("Enums", enumModel2.ContainingNamespaceName);
        Assert.Equal("Enums.Class1.Class2", enumModel2.ContainingClassName);
        Assert.Equal("byte", enumModel2.Type);
    }
}
