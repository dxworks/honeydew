﻿using System.Collections.Generic;
using HoneydewCore.Logging;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewExtractors.CSharp.Metrics.Extraction.CompilationUnit;
using HoneydewExtractors.CSharp.Metrics.Extraction.Enum;
using HoneydewModels.CSharp;
using Moq;
using Xunit;

namespace HoneydewExtractorsTests.CSharp.Metrics.Extraction.Enum;

public class CSharpEnumInfoTests
{
    private readonly CSharpFactExtractor _factExtractor;
    private readonly Mock<ILogger> _loggerMock = new();
    private readonly CSharpSyntacticModelCreator _syntacticModelCreator = new();
    private readonly CSharpSemanticModelCreator _semanticModelCreator = new(new CSharpCompilationMaker());

    public CSharpEnumInfoTests()
    {
        var compositeVisitor = new CompositeVisitor();

        compositeVisitor.Add(new EnumSetterCompilationUnitVisitor(new List<ICSharpEnumVisitor>
        {
            new BaseInfoEnumVisitor(),
            new EnumLabelsSetterVisitor(new List<IEnumLabelVisitor>
            {
                new BasicEnumLabelInfoVisitor(),
            }),
        }));

        compositeVisitor.Accept(new LoggerSetterVisitor(_loggerMock.Object));

        _factExtractor = new CSharpFactExtractor(compositeVisitor);
    }

    [Theory]
    [FileData("TestData/CSharp/Metrics/Extraction/Enum/BasicEnum.txt")]
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
    [FileData("TestData/CSharp/Metrics/Extraction/Enum/EnumWithOtherType.txt")]
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
    [FileData("TestData/CSharp/Metrics/Extraction/Enum/EnumInInnerClass.txt")]
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