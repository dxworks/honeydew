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

namespace Honeydew.Extractors.CSharp.Tests.Visitors.Others.GotoStatement;

public class GotoStatementVisitorTests
{
    private readonly CSharpFactExtractor _factExtractor;
    private readonly Mock<ILogger> _loggerMock = new();
    private readonly CSharpSyntacticModelCreator _syntacticModelCreator = new();
    private readonly DotnetSemanticModelCreator _semanticModelCreator = new(new CSharpCompilationMaker());

    public GotoStatementVisitorTests()
    {
        var gotoStatementVisitor = new GotoStatementVisitor();
        var localFunctionsSetterClassVisitor = new CSharpLocalFunctionsSetterVisitor(_loggerMock.Object,
            new List<ITypeVisitor<IMethodTypeWithLocalFunctions>>
            {
                new LocalFunctionInfoVisitor(_loggerMock.Object, new List<ITypeVisitor<IMethodTypeWithLocalFunctions>>
                {
                    gotoStatementVisitor
                }),
                gotoStatementVisitor
            });

        var compositeVisitor = new CSharpCompilationUnitCompositeVisitor(_loggerMock.Object,
            new List<ITypeVisitor<ICompilationUnitType>>
            {
                new CSharpClassSetterVisitor(_loggerMock.Object,
                    new List<ITypeVisitor<IMembersClassType>>
                    {
                        new BaseInfoClassVisitor(),
                        new CSharpConstructorSetterVisitor(_loggerMock.Object,
                            new List<ITypeVisitor<IConstructorType>>
                            {
                                new ConstructorInfoVisitor(),
                                gotoStatementVisitor,
                                localFunctionsSetterClassVisitor,
                            }),
                        new CSharpMethodSetterVisitor(_loggerMock.Object, new List<ITypeVisitor<IMethodType>>
                        {
                            new MethodInfoVisitor(),
                            gotoStatementVisitor,
                            localFunctionsSetterClassVisitor,
                        }),
                        new CSharpDestructorSetterVisitor(_loggerMock.Object,
                            new List<ITypeVisitor<IDestructorType>>
                            {
                                new DestructorInfoVisitor(),
                                gotoStatementVisitor,
                            }),
                        new CSharpPropertySetterClassVisitor(_loggerMock.Object, new List<ITypeVisitor<IPropertyType>>
                        {
                            new PropertyInfoVisitor(),
                            new CSharpAccessorMethodSetterVisitor(_loggerMock.Object,
                                new List<ITypeVisitor<IAccessorMethodType>>
                                {
                                    new MethodInfoVisitor(),
                                    gotoStatementVisitor,
                                    localFunctionsSetterClassVisitor,
                                })
                        })
                    })
            });

        _factExtractor = new CSharpFactExtractor(compositeVisitor);
    }

    [Theory]
    [FileData("TestData/GotoInConstructorTests.txt")]
    public void Extract_ShouldExtractGotoStatements_WhenProvidedConstructor(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classModel = (CSharpClassModel)classTypes[0];

        TestGotoStatementInMetrics(classModel.Constructors[0].Metrics);
    }

    [Theory]
    [FileData("TestData/GotoInDestructorTests.txt")]
    public void Extract_ShouldExtractGotoStatements_WhenProvidedDestructor(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classModel = (CSharpClassModel)classTypes[0];

        TestGotoStatementInMetrics(classModel.Destructor.Metrics);
    }

    [Theory]
    [FileData("TestData/GotoInMethodTests.txt")]
    public void Extract_ShouldExtractGotoStatements_WhenProvidedMethod(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classModel = (CSharpClassModel)classTypes[0];

        TestGotoStatementInMetrics(classModel.Methods[0].Metrics);
    }

    [Theory]
    [FileData("TestData/GotoInPropertyAccessorTests.txt")]
    public void Extract_ShouldExtractGotoStatements_WhenProvidedPropertyAccessors(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classModel = (CSharpClassModel)classTypes[0];

        TestGotoStatementInMetrics(classModel.Properties[0].Accessors[0].Metrics);
        TestGotoStatementInMetrics(classModel.Properties[0].Accessors[1].Metrics);

        TestGotoStatementInMetrics(classModel.Properties[1].Accessors[0].Metrics);
        TestGotoStatementInMetrics(classModel.Properties[1].Accessors[1].Metrics);
    }

    [Theory]
    [FileData("TestData/GotoInLocalFunctionTests.txt")]
    public void Extract_ShouldExtractGotoStatements_WhenProvidedMethodWithLocalFunctions(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classModel = (CSharpClassModel)classTypes[0];
        var methodModel = (CSharpMethodModel)classModel.Methods[0];

        Assert.Single(methodModel.Metrics);
        Assert.Equal(0, (int)methodModel.Metrics[0].Value!);

        TestGotoStatementInMetrics(methodModel.LocalFunctions[0].Metrics);
        TestGotoStatementInMetrics(methodModel.LocalFunctions[0].LocalFunctions[0].Metrics);
    }

    private static void TestGotoStatementInMetrics(IList<MetricModel> metrics)
    {
        Assert.Single(metrics);
        Assert.Equal(typeof(GotoStatementVisitor).FullName, metrics[0].ExtractorName);
        Assert.Equal(typeof(int).FullName, metrics[0].ValueType);
        Assert.Equal(3, (int)metrics[0].Value!);
    }
}
