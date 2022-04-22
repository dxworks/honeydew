using System.Collections.Generic;
using Honeydew.Extractors.CSharp.Visitors;
using Honeydew.Extractors.CSharp.Visitors.Concrete;
using Honeydew.Extractors.CSharp.Visitors.Setters;
using Honeydew.Extractors.Visitors;
using Honeydew.Models.CSharp;
using HoneydewCore.Logging;
using Moq;
using Xunit;

namespace Honeydew.Extractors.CSharp.Tests.LinesOfCodeCounter;

public class CSharpClassFactExtractorLinesOfCodeTests
{
    private readonly CSharpFactExtractor _sut;
    private readonly Mock<ILogger> _loggerMock = new();
    private readonly CSharpSyntacticModelCreator _syntacticModelCreator = new();
    private readonly CSharpSemanticModelCreator _semanticModelCreator = new(new CSharpCompilationMaker());

    public CSharpClassFactExtractorLinesOfCodeTests()
    {
        var linesOfCodeVisitor = new LinesOfCodeVisitor();
        var compositeVisitor = new CompositeVisitor(_loggerMock.Object);

        compositeVisitor.Add(new ClassSetterCompilationUnitVisitor(_loggerMock.Object, new List<ICSharpClassVisitor>
        {
            linesOfCodeVisitor,
            new ConstructorSetterClassVisitor(_loggerMock.Object, new List<ICSharpConstructorVisitor>
            {
                linesOfCodeVisitor
            }),
            new MethodSetterClassVisitor(_loggerMock.Object, new List<ICSharpMethodVisitor>
            {
                linesOfCodeVisitor,
                new LocalFunctionsSetterClassVisitor(_loggerMock.Object, new List<ICSharpLocalFunctionVisitor>
                {
                    linesOfCodeVisitor,
                    new LocalFunctionInfoVisitor(_loggerMock.Object, new List<ILocalFunctionVisitor>
                    {
                        linesOfCodeVisitor
                    }),
                })
            }),
            new PropertySetterClassVisitor(_loggerMock.Object, new List<ICSharpPropertyVisitor>
            {
                linesOfCodeVisitor,
                new MethodAccessorSetterPropertyVisitor(_loggerMock.Object, new List<IMethodVisitor>
                {
                    linesOfCodeVisitor
                })
            })
        }));
        compositeVisitor.Add(new EnumSetterCompilationUnitVisitor(_loggerMock.Object, new List<ICSharpEnumVisitor>
        {
            linesOfCodeVisitor,
        }));
        compositeVisitor.Add(new DelegateSetterCompilationUnitVisitor(_loggerMock.Object, new List<IDelegateVisitor>
        {
            linesOfCodeVisitor
        }));

        compositeVisitor.Add(linesOfCodeVisitor);

        _sut = new CSharpFactExtractor(compositeVisitor);
    }

    [Theory]
    [FileData("TestData/ClassWithCommentsWithPropertyAndMethod.txt")]
    public void Extract_ShouldHaveLinesOfCode_WhenProvidedWithClassWithMethodsAndProperties(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);

        var compilationUnit = _sut.Extract(syntaxTree, semanticModel);

        var classModels = compilationUnit.ClassTypes;

        Assert.Equal(11, compilationUnit.Loc.SourceLines);
        Assert.Equal(18, compilationUnit.Loc.EmptyLines);
        Assert.Equal(8, compilationUnit.Loc.CommentedLines);

        var classModel = (ClassModel)classModels[0];
        Assert.Equal(8, classModel.Loc.SourceLines);
        Assert.Equal(12, classModel.Loc.EmptyLines);
        Assert.Equal(5, classModel.Loc.CommentedLines);

        Assert.Equal(3, classModel.Methods[0].Loc.SourceLines);
        Assert.Equal(2, classModel.Methods[0].Loc.CommentedLines);
        Assert.Equal(2, classModel.Methods[0].Loc.EmptyLines);

        Assert.Equal(3, classModel.Properties[0].Loc.SourceLines);
        Assert.Equal(1, classModel.Properties[0].Loc.CommentedLines);
        Assert.Equal(5, classModel.Properties[0].Loc.EmptyLines);
    }

    [Theory]
    [FileData("TestData/ClassWithPropertyAndMethodAndDelegateWithComments.txt")]
    public void Extract_ShouldHaveLinesOfCode_WhenProvidedWithClassAndDelegate(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);

        var compilationUnit = _sut.Extract(syntaxTree, semanticModel);

        var classModels = compilationUnit.ClassTypes;

        Assert.Equal(12, compilationUnit.Loc.SourceLines);
        Assert.Equal(19, compilationUnit.Loc.EmptyLines);
        Assert.Equal(8, compilationUnit.Loc.CommentedLines);

        var classModel = (ClassModel)classModels[0];
        Assert.Equal(8, classModel.Loc.SourceLines);
        Assert.Equal(14, classModel.Loc.EmptyLines);
        Assert.Equal(5, classModel.Loc.CommentedLines);

        Assert.Equal(3, classModel.Methods[0].Loc.SourceLines);
        Assert.Equal(2, classModel.Methods[0].Loc.CommentedLines);
        Assert.Equal(4, classModel.Methods[0].Loc.EmptyLines);

        Assert.Equal(3, classModel.Properties[0].Loc.SourceLines);
        Assert.Equal(1, classModel.Properties[0].Loc.CommentedLines);
        Assert.Equal(5, classModel.Properties[0].Loc.EmptyLines);
    }

    [Theory]
    [FileData("TestData/EnumWithComments.txt")]
    public void Extract_ShouldHaveLinesOfCode_WhenProvidedWithEnum(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);

        var compilationUnit = _sut.Extract(syntaxTree, semanticModel);

        var classModels = compilationUnit.ClassTypes;

        Assert.Equal(8, compilationUnit.Loc.SourceLines);
        Assert.Equal(8, compilationUnit.Loc.EmptyLines);
        Assert.Equal(2, compilationUnit.Loc.CommentedLines);

        var classModel = (EnumModel)classModels[0];
        Assert.Equal(5, classModel.Loc.SourceLines);
        Assert.Equal(5, classModel.Loc.EmptyLines);
        Assert.Equal(1, classModel.Loc.CommentedLines);
    }

    [Theory]
    [FileData("TestData/DelegateOnOneLine.txt")]
    public void Extract_ShouldHaveLinesOfCode_WhenProvidedWithDelegateOnOneLine(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);

        var compilationUnit = _sut.Extract(syntaxTree, semanticModel);

        var classModels = compilationUnit.ClassTypes;

        Assert.Equal(2, compilationUnit.Loc.SourceLines);
        Assert.Equal(3, compilationUnit.Loc.EmptyLines);
        Assert.Equal(1, compilationUnit.Loc.CommentedLines);

        var classModel = (DelegateModel)classModels[0];
        Assert.Equal(1, classModel.Loc.SourceLines);
        Assert.Equal(0, classModel.Loc.EmptyLines);
        Assert.Equal(0, classModel.Loc.CommentedLines);
    }

    [Theory]
    [FileData("TestData/DelegateOnMultipleLines.txt")]
    public void Extract_ShouldHaveLinesOfCode_WhenProvidedWithDelegateOnMultipleLines(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);

        var compilationUnit = _sut.Extract(syntaxTree, semanticModel);

        var classModels = compilationUnit.ClassTypes;

        Assert.Equal(5, compilationUnit.Loc.SourceLines);
        Assert.Equal(3, compilationUnit.Loc.EmptyLines);
        Assert.Equal(1, compilationUnit.Loc.CommentedLines);

        var classModel = (DelegateModel)classModels[0];
        Assert.Equal(4, classModel.Loc.SourceLines);
        Assert.Equal(1, classModel.Loc.EmptyLines);
        Assert.Equal(0, classModel.Loc.CommentedLines);
    }

    [Theory]
    [FileData("TestData/LocalFunctionWithComments.txt")]
    public void Extract_ShouldHaveLinesOfCode_WhenMethodWithLocalFunction(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);

        var compilationUnit = _sut.Extract(syntaxTree, semanticModel);

        var classTypes = compilationUnit.ClassTypes;

        var localFunction = ((MethodModel)((ClassModel)classTypes[0]).Methods[0]).LocalFunctions[0];

        Assert.Equal(2, localFunction.Loc.SourceLines);
        Assert.Equal(1, localFunction.Loc.CommentedLines);
        Assert.Equal(3, localFunction.Loc.EmptyLines);
    }

    [Theory]
    [FileData("TestData/ClassWithCommentsWithPropertyAndMethod.txt")]
    public void Extract_ShouldHaveLinesOfCode_WhenGivenPropertyWithGetAccessor(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);

        var compilationUnit = _sut.Extract(syntaxTree, semanticModel);

        var classTypes = compilationUnit.ClassTypes;

        var accessor = ((ClassModel)classTypes[0]).Properties[0].Accessors[0];

        Assert.Equal(2, accessor.Loc.SourceLines);
        Assert.Equal(1, accessor.Loc.CommentedLines);
        Assert.Equal(3, accessor.Loc.EmptyLines);
    }

    [Theory]
    [FileData("TestData/ClassWithEventPropertyThatCallsMethodFromExternClass.txt")]
    public void Extract_ShouldHaveLinesOfCode_WhenGivenEventPropertyWithGetAccessor(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);

        var compilationUnit = _sut.Extract(syntaxTree, semanticModel);

        var classTypes = compilationUnit.ClassTypes;

        var addAccessor = ((ClassModel)classTypes[0]).Properties[0].Accessors[0];
        Assert.Equal(2, addAccessor.Loc.SourceLines);
        Assert.Equal(0, addAccessor.Loc.CommentedLines);
        Assert.Equal(2, addAccessor.Loc.EmptyLines);

        var removeAccessor = ((ClassModel)classTypes[0]).Properties[0].Accessors[1];
        Assert.Equal(1, removeAccessor.Loc.SourceLines);
        Assert.Equal(0, removeAccessor.Loc.CommentedLines);
        Assert.Equal(0, removeAccessor.Loc.EmptyLines);
    }
}
