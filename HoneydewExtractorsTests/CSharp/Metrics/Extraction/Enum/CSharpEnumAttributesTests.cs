using System.Collections.Generic;
using HoneydewCore.Logging;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Attributes;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewExtractors.CSharp.Metrics.Extraction.Attribute;
using HoneydewExtractors.CSharp.Metrics.Extraction.Common;
using HoneydewExtractors.CSharp.Metrics.Extraction.CompilationUnit;
using HoneydewExtractors.CSharp.Metrics.Extraction.Enum;
using HoneydewModels.CSharp;
using Moq;
using Xunit;

namespace HoneydewExtractorsTests.CSharp.Metrics.Extraction.Enum;

public class CSharpEnumAttributesTests
{
    private readonly CSharpFactExtractor _factExtractor;
    private readonly Mock<ILogger> _loggerMock = new();
    private readonly CSharpSyntacticModelCreator _syntacticModelCreator = new();
    private readonly CSharpSemanticModelCreator _semanticModelCreator = new(new CSharpCompilationMaker());

    public CSharpEnumAttributesTests()
    {
        var compositeVisitor = new CompositeVisitor();

        var attributeSetterVisitor = new AttributeSetterVisitor(new List<IAttributeVisitor>
        {
            new AttributeInfoVisitor()
        });
        compositeVisitor.Add(new EnumSetterCompilationUnitVisitor(new List<ICSharpEnumVisitor>
        {
            new BaseInfoEnumVisitor(),
            new EnumLabelsSetterVisitor(new List<IEnumLabelVisitor>
            {
                new BasicEnumLabelInfoVisitor(),
                attributeSetterVisitor,
            }),
            attributeSetterVisitor,
        }));

        compositeVisitor.Accept(new LoggerSetterVisitor(_loggerMock.Object));

        _factExtractor = new CSharpFactExtractor(compositeVisitor);
    }

    [Theory]
    [FileData("TestData/CSharp/Metrics/Extraction/ClassLevel/Attributes/EnumWithAttributes.txt")]
    public void Extract_ShouldExtractAttribute_GivenEnum(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var enumModel = (EnumModel)classTypes[0];
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
    [FileData("TestData/CSharp/Metrics/Extraction/ClassLevel/Attributes/EnumWithLabelAttributes.txt")]
    public void Extract_ShouldExtractLabelAttributes_GivenEnum(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var enumModel = (EnumModel)classTypes[0];
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
