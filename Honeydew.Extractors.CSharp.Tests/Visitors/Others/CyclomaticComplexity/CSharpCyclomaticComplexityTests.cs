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

namespace Honeydew.Extractors.CSharp.Tests.Visitors.Others.CyclomaticComplexity;

public class CSharpCyclomaticComplexityTests
{
    private readonly CSharpFactExtractor _factExtractor;
    private readonly Mock<ILogger> _loggerMock = new();
    private readonly CSharpSyntacticModelCreator _syntacticModelCreator = new();
    private readonly DotnetSemanticModelCreator _semanticModelCreator = new(new CSharpCompilationMaker());

    public CSharpCyclomaticComplexityTests()
    {
        var compositeVisitor = new CSharpCompilationUnitCompositeVisitor(_loggerMock.Object,
            new List<ITypeVisitor<ICompilationUnitType>>
            {
                new CSharpClassSetterVisitor(_loggerMock.Object,
                    new List<ITypeVisitor<IMembersClassType>>
                    {
                        new BaseInfoClassVisitor(),
                        new CSharpPropertySetterClassVisitor(_loggerMock.Object, new List<ITypeVisitor<IPropertyType>>
                        {
                            new PropertyInfoVisitor()
                        }),
                        new CSharpMethodSetterClassVisitor(_loggerMock.Object, new List<ITypeVisitor<IMethodType>>
                        {
                            new MethodInfoVisitor()
                        }),
                        new CSharpConstructorSetterClassVisitor(_loggerMock.Object,
                            new List<ITypeVisitor<IConstructorType>>
                            {
                                new ConstructorInfoVisitor()
                            })
                    }),
                new CSharpDelegateSetterCompilationUnitVisitor(_loggerMock.Object,
                    new List<ITypeVisitor<IDelegateType>>
                    {
                        new BaseInfoDelegateVisitor()
                    })
            });

        _factExtractor = new CSharpFactExtractor(compositeVisitor);
    }

    [Theory]
    [FileData("TestData/ClassWithDelegateFieldConstructorAndMethod.txt")]
    public void Extract_ShouldHave1CyclomaticComplexity_WhenGivenClassWithMethodsAndPropertiesAndDelegate(
        string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(2, classTypes.Count);

        var classModel = (CSharpClassModel)classTypes[0];
        Assert.Equal(1, classModel.Constructors[0].CyclomaticComplexity);
        Assert.Equal(1, classModel.Methods[0].CyclomaticComplexity);
        Assert.Equal(2, classModel.Properties[0].CyclomaticComplexity);
    }

    [Theory]
    [FileData("TestData/ClassWithDelegateFieldConstructorAndMethodsContainingWhile.txt")]
    public void
        Extract_ShouldCountCyclomaticComplexityFromWhiles_WhenGivenClassWithMethodsAndPropertiesAndDelegate(
            string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(2, classTypes.Count);

        var classModel = (CSharpClassModel)classTypes[0];
        Assert.Equal(2, classModel.Constructors[0].CyclomaticComplexity);
        Assert.Equal(4, classModel.Methods[0].CyclomaticComplexity);
        Assert.Equal(4, classModel.Properties[0].CyclomaticComplexity);
    }

    [Theory]
    [FileData("TestData/ClassWithDelegateFieldConstructorAndMethodsContainingIf.txt")]
    public void
        Extract_ShouldCountCyclomaticComplexityFromIfs_WhenGivenClassWithMethodsAndProperties(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(1, classTypes.Count);

        var classModel = (CSharpClassModel)classTypes[0];
        Assert.Equal(3, classModel.Constructors[0].CyclomaticComplexity);
        Assert.Equal(4, classModel.Methods[0].CyclomaticComplexity);
        Assert.Equal(4, classModel.Properties[0].CyclomaticComplexity);
    }

    [Theory]
    [FileData("TestData/ClassWithMethodContainingFor.txt")]
    public void
        Extract_ShouldCountCyclomaticComplexityFromFors_WhenGivenClassWithMethods(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(1, classTypes.Count);

        Assert.Equal(5, ((CSharpClassModel)classTypes[0]).Methods[0].CyclomaticComplexity);
    }

    [Theory]
    [FileData("TestData/ClassWithMethodContainingUnaryExpressionsInConditions.txt")]
    public void
        Extract_ShouldCountCyclomaticComplexityFromUnaryExpression_WhenGivenClassWithMethods(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(1, classTypes.Count);

        Assert.Equal(7, ((CSharpClassModel)classTypes[0]).Methods[0].CyclomaticComplexity);
    }

    [Theory]
    [FileData("TestData/ClassWithComplexBinaryExpression.txt")]
    public void
        Extract_ShouldCountCyclomaticComplexityFromComplexBinaryExpression_WhenGivenClassWithMethods(
            string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(1, classTypes.Count);

        Assert.Equal(7, ((CSharpClassModel)classTypes[0]).Methods[0].CyclomaticComplexity);
    }

    [Theory]
    [FileData("TestData/ClassWithMethodContainingBinaryExpressionWithIsAndOrNot.txt")]
    public void
        Extract_ShouldCountCyclomaticComplexityFromComplexBinaryExpressionWithIsAndOr_WhenGivenClassWithMethods(
            string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(1, classTypes.Count);

        Assert.Equal(6, ((CSharpClassModel)classTypes[0]).Methods[0].CyclomaticComplexity);
    }

    [Theory]
    [FileData("TestData/ClassWithMethodContainingDoWhile.txt")]
    public void
        Extract_ShouldCountCyclomaticComplexityFromDoWhile_WhenGivenClassWithMethods(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(1, classTypes.Count);

        Assert.Equal(2, ((CSharpClassModel)classTypes[0]).Methods[0].CyclomaticComplexity);
    }

    [Theory]
    [FileData("TestData/ClassWithMethodContainingForeach.txt")]
    public void
        Extract_ShouldCountCyclomaticComplexityFromForeach_WhenGivenClassWithMethods(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(1, classTypes.Count);

        Assert.Equal(2, ((CSharpClassModel)classTypes[0]).Methods[0].CyclomaticComplexity);
    }

    [Theory]
    [FileData("TestData/ClassWithMethodContainingSwitch.txt")]
    public void
        Extract_ShouldCountCyclomaticComplexityFromSwitch_WhenGivenClassWithMethods(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(1, classTypes.Count);

        Assert.Equal(7, ((CSharpClassModel)classTypes[0]).Methods[0].CyclomaticComplexity);
    }

    [Theory]
    [FileData("TestData/ClassWithMethodContainingSwitchWithOperators.txt")]
    public void
        Extract_ShouldCountCyclomaticComplexityFromPatternSwitchWithOperators_WhenGivenClassWithMethods(
            string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(1, classTypes.Count);

        Assert.Equal(6, ((CSharpClassModel)classTypes[0]).Methods[0].CyclomaticComplexity);
    }

    [Theory]
    [FileData("TestData/ClassWithMethodThatReturnsAStringWithSwitch.txt")]
    public void
        Extract_ShouldCountCyclomaticComplexityFromReturnSwitchWithStrings_WhenGivenClassWithMethods(
            string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(1, classTypes.Count);

        Assert.Equal(1, ((CSharpClassModel)classTypes[0]).Methods[0].CyclomaticComplexity);
    }

    [Theory]
    [FileData("TestData/ClassWithMethodContainingPatternSwitch.txt")]
    public void
        Extract_ShouldCountCyclomaticComplexityFromPatternSwitchWithClassHierarchy_WhenGivenClassWithMethods(
            string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(5, classTypes.Count);

        Assert.Equal(5, ((CSharpClassModel)classTypes[0]).Methods[0].CyclomaticComplexity);
    }

    [Theory]
    [FileData("TestData/ClassWithMethodContainingConditionalOperators.txt")]
    public void
        Extract_ShouldCountCyclomaticComplexityForConditionalOperators_WhenGivenClassWithMethods(
            string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(1, classTypes.Count);

        Assert.Equal(5, ((CSharpClassModel)classTypes[0]).Methods[0].CyclomaticComplexity);
    }
}
