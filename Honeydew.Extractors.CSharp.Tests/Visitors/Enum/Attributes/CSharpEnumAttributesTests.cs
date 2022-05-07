using System.Collections.Generic;
using Honeydew.Extractors.CSharp.Visitors.Concrete;
using Honeydew.Extractors.CSharp.Visitors.Setters;
using Honeydew.Extractors.Dotnet;
using Honeydew.Extractors.Visitors;
using Honeydew.Logging;
using Honeydew.Models.CSharp;
using Honeydew.Models.Types;
using Moq;
using Xunit;

namespace Honeydew.Extractors.CSharp.Tests.Visitors.Enum.Attributes;

public class CSharpEnumAttributesTests
{
    private readonly CSharpFactExtractor _factExtractor;
    private readonly Mock<ILogger> _loggerMock = new();
    private readonly CSharpSyntacticModelCreator _syntacticModelCreator = new();
    private readonly DotnetSemanticModelCreator _semanticModelCreator = new(new CSharpCompilationMaker());

    public CSharpEnumAttributesTests()
    {
        var attributeSetterVisitor = new CSharpAttributeSetterVisitor(_loggerMock.Object,
            new List<ITypeVisitor<IAttributeType>>
            {
                new AttributeInfoVisitor()
            });

        var compositeVisitor = new CSharpCompilationUnitCompositeVisitor(_loggerMock.Object,
            new List<ITypeVisitor<ICompilationUnitType>>
            {
                new CSharpEnumSetterVisitor(_loggerMock.Object, new List<ITypeVisitor<IEnumType>>
                {
                    new BaseInfoEnumVisitor(),
                    new CSharpEnumLabelsSetterVisitor(_loggerMock.Object, new List<ITypeVisitor<IEnumLabelType>>
                    {
                        new BasicEnumLabelInfoVisitor(),
                        attributeSetterVisitor,
                    }),
                    attributeSetterVisitor,
                })
            });

        _factExtractor = new CSharpFactExtractor(compositeVisitor);
    }

    [Theory]
    [FileData("TestData/EnumWithAttributes.txt")]
    public void Extract_ShouldExtractAttribute_GivenEnum(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var enumModel = (CSharpEnumModel)classTypes[0];
        Assert.Equal(1, enumModel.Attributes.Count);
        Assert.Equal("type", enumModel.Attributes[0].Target);
        Assert.Equal("System.ObsoleteAttribute", enumModel.Attributes[0].Name);
        Assert.Equal(1, enumModel.Attributes[0].ParameterTypes.Count);
        Assert.Equal("string?", enumModel.Attributes[0].ParameterTypes[0].Type.Name);
        Assert.Equal("string", enumModel.Attributes[0].ParameterTypes[0].Type.FullType.Name);
        Assert.True(enumModel.Attributes[0].ParameterTypes[0].Type.FullType.IsNullable);
        foreach (var label in enumModel.Labels)
        {
            Assert.Empty(label.Attributes);
        }
    }

    [Theory]
    [FileData("TestData/EnumWithLabelAttributes.txt")]
    public void Extract_ShouldExtractLabelAttributes_GivenEnum(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var enumModel = (CSharpEnumModel)classTypes[0];
        Assert.Empty(enumModel.Attributes);

        var label1 = enumModel.Labels[0];
        Assert.Equal(1, label1.Attributes.Count);
        Assert.Equal("field", label1.Attributes[0].Target);
        Assert.Equal("System.ObsoleteAttribute", label1.Attributes[0].Name);
        Assert.Equal(1, label1.Attributes[0].ParameterTypes.Count);
        Assert.Equal("string?", label1.Attributes[0].ParameterTypes[0].Type.Name);
        Assert.Equal("string", label1.Attributes[0].ParameterTypes[0].Type.FullType.Name);
        Assert.True(label1.Attributes[0].ParameterTypes[0].Type.FullType.IsNullable);

        var label2 = enumModel.Labels[1];
        Assert.Equal(1, label2.Attributes.Count);
        Assert.Equal("field", label2.Attributes[0].Target);
        Assert.Equal("System.ObsoleteAttribute", label2.Attributes[0].Name);
        Assert.Empty(label2.Attributes[0].ParameterTypes);
    }
}
