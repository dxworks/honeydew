﻿using System.Collections.Generic;
using Honeydew.Extractors.CSharp.Visitors.Concrete;
using Honeydew.Extractors.CSharp.Visitors.Setters;
using Honeydew.Extractors.Dotnet;
using Honeydew.Extractors.Visitors;
using Honeydew.Logging;
using Honeydew.Models.Types;
using Moq;
using Xunit;

namespace Honeydew.Extractors.CSharp.Tests.Visitors.Delegate.Attributes;

public class CSharpDelegateAttributeMetricTests
{
    private readonly CSharpFactExtractor _factExtractor;
    private readonly Mock<ILogger> _loggerMock = new();
    private readonly CSharpSyntacticModelCreator _syntacticModelCreator = new();
    private readonly DotnetSemanticModelCreator _semanticModelCreator = new(new CSharpCompilationMaker());

    public CSharpDelegateAttributeMetricTests()
    {
        var compositeVisitor = new CSharpCompilationUnitCompositeVisitor(_loggerMock.Object,
            new List<ITypeVisitor<ICompilationUnitType>>
            {
                new CSharpDelegateSetterVisitor(_loggerMock.Object, new List<ITypeVisitor<IDelegateType>>
                {
                    new BaseInfoDelegateVisitor(),
                    new CSharpAttributeSetterVisitor(_loggerMock.Object, new List<ITypeVisitor<IAttributeType>>
                    {
                        new AttributeInfoVisitor()
                    })
                })
            });

        _factExtractor = new CSharpFactExtractor(compositeVisitor);
    }

    [Theory]
    [FileData("TestData/DelegateWithOneAttributeWithNoParams.txt")]
    public void Extract_ShouldExtractAttribute_WhenProvidedWithOneAttributeWithNoParams(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(1, classTypes[0].Attributes.Count);
        Assert.Equal("type", classTypes[0].Attributes[0].Target);
        Assert.Equal("System.SerializableAttribute", classTypes[0].Attributes[0].Name);
        Assert.Empty(classTypes[0].Attributes[0].ParameterTypes);
    }

    [Theory]
    [FileData("TestData/DelegateWithOneAttributeWithOneParam.txt")]
    public void Extract_ShouldExtractAttribute_WhenProvidedWithOneAttributeWithOneParams(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(1, classTypes[0].Attributes.Count);
        Assert.Equal("type", classTypes[0].Attributes[0].Target);
        Assert.Equal("System.ObsoleteAttribute", classTypes[0].Attributes[0].Name);
        Assert.Equal(1, classTypes[0].Attributes[0].ParameterTypes.Count);
        Assert.Equal("string?", classTypes[0].Attributes[0].ParameterTypes[0].Type.Name);
        Assert.Equal("string", classTypes[0].Attributes[0].ParameterTypes[0].Type.FullType.Name);
        Assert.True(classTypes[0].Attributes[0].ParameterTypes[0].Type.FullType.IsNullable);
    }

    [Theory]
    [FileData("TestData/DelegateWithMultipleAttributesWithMultipleParams.txt")]
    [FileData("TestData/DelegateWithMultipleAttributesWithMultipleParamsInDifferentSections.txt")]
    public void Extract_ShouldExtractAttribute_WhenProvidedWithMultipleAttributesWitMultipleParams(
        string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(3, classTypes[0].Attributes.Count);
        foreach (var attribute in classTypes[0].Attributes)
        {
            Assert.Equal("type", attribute.Target);
        }

        var attribute1 = classTypes[0].Attributes[0];
        Assert.Equal(2, attribute1.ParameterTypes.Count);
        Assert.Equal("System.ObsoleteAttribute", attribute1.Name);
        Assert.Equal("string?", attribute1.ParameterTypes[0].Type.Name);
        Assert.Equal("string", attribute1.ParameterTypes[0].Type.FullType.Name);
        Assert.True(attribute1.ParameterTypes[0].Type.FullType.IsNullable);
        Assert.Equal("bool", attribute1.ParameterTypes[1].Type.Name);

        var attribute2 = classTypes[0].Attributes[1];
        Assert.Equal("System.SerializableAttribute", attribute2.Name);
        Assert.Empty(attribute2.ParameterTypes);

        var attribute3 = classTypes[0].Attributes[2];
        Assert.Equal("System.AttributeUsageAttribute", attribute3.Name);
        Assert.Equal(1, attribute3.ParameterTypes.Count);
        Assert.Equal("System.AttributeTargets", attribute3.ParameterTypes[0].Type.Name);
    }

    [Theory]
    [FileData("TestData/DelegateWithCustomAttribute.txt")]
    public void Extract_ShouldExtractAttribute_WhenProvidedWithCustomAttribute(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classType = classTypes[0];

        Assert.Equal(4, classType.Attributes.Count);
        foreach (var attribute in classType.Attributes)
        {
            Assert.Equal("type", attribute.Target);
            Assert.Equal("MyNamespace.MyAttribute", attribute.Name);
        }

        var attribute1 = classType.Attributes[0];
        Assert.Equal(1, attribute1.ParameterTypes.Count);
        Assert.Equal("string", attribute1.ParameterTypes[0].Type.Name);

        var attribute2 = classType.Attributes[1];
        Assert.Empty(attribute2.ParameterTypes);

        var attribute3 = classType.Attributes[2];
        Assert.Equal(1, attribute3.ParameterTypes.Count);
        Assert.Equal("string", attribute3.ParameterTypes[0].Type.Name);

        var attribute4 = classType.Attributes[3];
        Assert.Empty(attribute4.ParameterTypes);
    }

    [Theory]
    [FileData("TestData/DelegateWithExternAttribute.txt")]
    public void Extract_ShouldExtractAttribute_WhenProvidedWithExternAttribute(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classType = classTypes[0];

        Assert.Equal(5, classType.Attributes.Count);
        foreach (var attribute in classType.Attributes)
        {
            Assert.Equal("type", attribute.Target);
        }

        var attribute1 = classType.Attributes[0];
        Assert.Equal("Extern", attribute1.Name);
        Assert.Equal(1, attribute1.ParameterTypes.Count);
        Assert.Equal("System.String", attribute1.ParameterTypes[0].Type.Name);

        var attribute2 = classType.Attributes[1];
        Assert.Equal("ExternAttribute", attribute2.Name);
        Assert.Empty(attribute2.ParameterTypes);

        var attribute3 = classType.Attributes[2];
        Assert.Equal("ExternAttribute", attribute3.Name);
        Assert.Equal(2, attribute3.ParameterTypes.Count);
        Assert.Equal("System.String", attribute3.ParameterTypes[0].Type.Name);
        Assert.Equal("System.Boolean", attribute3.ParameterTypes[1].Type.Name);

        var attribute4 = classType.Attributes[3];
        Assert.Equal("Extern", attribute4.Name);
        Assert.Equal(1, attribute4.ParameterTypes.Count);
        Assert.Equal("System.Int32", attribute4.ParameterTypes[0].Type.Name);

        var attribute5 = classType.Attributes[4];
        Assert.Equal("Extern", attribute5.Name);
        Assert.Equal(1, attribute5.ParameterTypes.Count);
        Assert.Equal("System.Object", attribute5.ParameterTypes[0].Type.Name);
    }
}
