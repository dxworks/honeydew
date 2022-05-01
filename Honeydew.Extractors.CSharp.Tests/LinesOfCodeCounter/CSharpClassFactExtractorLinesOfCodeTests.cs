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

namespace Honeydew.Extractors.CSharp.Tests.LinesOfCodeCounter;

public class CSharpClassFactExtractorLinesOfCodeTests
{
    private readonly CSharpFactExtractor _sut;
    private readonly Mock<ILogger> _loggerMock = new();
    private readonly CSharpSyntacticModelCreator _syntacticModelCreator = new();
    private readonly DotnetSemanticModelCreator _semanticModelCreator = new(new CSharpCompilationMaker());

    public CSharpClassFactExtractorLinesOfCodeTests()
    {
        var linesOfCodeVisitor = new LinesOfCodeVisitor();
        var compositeVisitor = new CSharpCompilationUnitCompositeVisitor(_loggerMock.Object,
            new List<ITypeVisitor<ICompilationUnitType>>
            {
                new CSharpClassSetterVisitor(_loggerMock.Object,
                    new List<ITypeVisitor<IMembersClassType>>
                    {
                        linesOfCodeVisitor,
                        new CSharpConstructorSetterVisitor(_loggerMock.Object,
                            new List<ITypeVisitor<IConstructorType>>
                            {
                                linesOfCodeVisitor
                            }),
                        new CSharpMethodSetterVisitor(_loggerMock.Object, new List<ITypeVisitor<IMethodType>>
                        {
                            linesOfCodeVisitor,
                            new CSharpLocalFunctionsSetterVisitor(_loggerMock.Object,
                                new List<ITypeVisitor<IMethodTypeWithLocalFunctions>>
                                {
                                    linesOfCodeVisitor,
                                    new LocalFunctionInfoVisitor(_loggerMock.Object,
                                        new List<ITypeVisitor<IMethodTypeWithLocalFunctions>>
                                        {
                                            linesOfCodeVisitor
                                        }),
                                })
                        }),
                        new CSharpPropertySetterClassVisitor(_loggerMock.Object, new List<ITypeVisitor<IPropertyType>>
                        {
                            linesOfCodeVisitor,
                            new CSharpAccessorMethodSetterVisitor(_loggerMock.Object,
                                new List<ITypeVisitor<IAccessorMethodType>>
                                {
                                    linesOfCodeVisitor
                                })
                        })
                    }),
                new CSharpEnumSetterVisitor(_loggerMock.Object, new List<ITypeVisitor<IEnumType>>
                {
                    linesOfCodeVisitor,
                }),
                new CSharpDelegateSetterVisitor(_loggerMock.Object, new List<ITypeVisitor<IDelegateType>>
                {
                    linesOfCodeVisitor
                }),
                linesOfCodeVisitor
            });

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

        var classModel = (CSharpClassModel)classModels[0];
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

        var classModel = (CSharpClassModel)classModels[0];
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

        var classModel = (CSharpEnumModel)classModels[0];
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

        var classModel = (CSharpDelegateModel)classModels[0];
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

        var classModel = (CSharpDelegateModel)classModels[0];
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

        var localFunction = ((CSharpMethodModel)((CSharpClassModel)classTypes[0]).Methods[0]).LocalFunctions[0];

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

        var accessor = ((CSharpClassModel)classTypes[0]).Properties[0].Accessors[0];

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

        var addAccessor = ((CSharpClassModel)classTypes[0]).Properties[0].Accessors[0];
        Assert.Equal(2, addAccessor.Loc.SourceLines);
        Assert.Equal(0, addAccessor.Loc.CommentedLines);
        Assert.Equal(2, addAccessor.Loc.EmptyLines);

        var removeAccessor = ((CSharpClassModel)classTypes[0]).Properties[0].Accessors[1];
        Assert.Equal(1, removeAccessor.Loc.SourceLines);
        Assert.Equal(0, removeAccessor.Loc.CommentedLines);
        Assert.Equal(0, removeAccessor.Loc.EmptyLines);
    }
}
