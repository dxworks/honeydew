using System.Collections.Generic;
using HoneydewCore.Logging;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewExtractors.CSharp.Metrics.Extraction.Class;
using HoneydewExtractors.CSharp.Metrics.Extraction.CompilationUnit;
using Moq;
using Xunit;

namespace HoneydewExtractorsTests.CSharp.Metrics.Extraction.CompilationUnitLevel;

public class CSharpFileScopedNamespaceTests
{
    private readonly CSharpFactExtractor _factExtractor;
    private readonly Mock<ILogger> _loggerMock = new();
    private readonly CSharpSyntacticModelCreator _syntacticModelCreator = new();
    private readonly CSharpSemanticModelCreator _semanticModelCreator = new(new CSharpCompilationMaker());

    public CSharpFileScopedNamespaceTests()
    {
        var compositeVisitor = new CompositeVisitor();

        compositeVisitor.Add(new ClassSetterCompilationUnitVisitor(new List<IClassVisitor>
        {
            new BaseInfoClassVisitor(),
        }));

        compositeVisitor.Accept(new LoggerSetterVisitor(_loggerMock.Object));

        _factExtractor = new CSharpFactExtractor(compositeVisitor);
    }

    [Theory]
    [InlineData("class")]
    [InlineData("record")]
    [InlineData("struct")]
    [InlineData("enum")]
    [InlineData("interface")]
    public void Extract_ShouldHaveClassType_WhenGivenFileScopedNamespace(string classType)
    {
        var fileContent = $@"using System;

namespace Namespace1;

public {classType} Foo {{ }}";

        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);

        var compilationUnit = _factExtractor.Extract(syntaxTree, semanticModel);
        var classModel = compilationUnit.ClassTypes[0];

        Assert.Single(compilationUnit.ClassTypes);
        Assert.Equal("Namespace1.Foo", classModel.Name);
        Assert.Equal(classType, classModel.ClassType);
        Assert.Equal("Namespace1", classModel.ContainingNamespaceName);
        Assert.Equal("", classModel.ContainingClassName);
    }
}
