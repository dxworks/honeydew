using System.Collections.Generic;
using Honeydew.Extractors.CSharp.Visitors.Concrete;
using Honeydew.Extractors.CSharp.Visitors.Setters;
using Honeydew.Extractors.Visitors;
using Honeydew.Logging;
using Honeydew.Models.Types;
using Moq;
using Xunit;

namespace Honeydew.Extractors.CSharp.Tests.Visitors.CompilationUnit.FileScopedNamespace;

public class CSharpFileScopedNamespaceTests
{
    private readonly CSharpFactExtractor _factExtractor;
    private readonly Mock<ILogger> _loggerMock = new();
    private readonly CSharpSyntacticModelCreator _syntacticModelCreator = new();
    private readonly CSharpSemanticModelCreator _semanticModelCreator = new(new CSharpCompilationMaker());

    public CSharpFileScopedNamespaceTests()
    {
        var compositeVisitor = new CSharpCompilationUnitCompositeVisitor(_loggerMock.Object,
            new List<ITypeVisitor<ICompilationUnitType>>
            {
                new CSharpClassSetterCompilationUnitVisitor(_loggerMock.Object,
                    new List<ITypeVisitor<IMembersClassType>>
                    {
                        new BaseInfoClassVisitor(),
                    })
            });

        _factExtractor = new CSharpFactExtractor(compositeVisitor);
    }

    [Theory]
    [InlineData("class")]
    [InlineData("record")]
    [InlineData("struct")]
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
