using System.Collections.Generic;
using Honeydew.Extractors.CSharp.Visitors;
using Honeydew.Extractors.CSharp.Visitors.Concrete;
using Honeydew.Extractors.CSharp.Visitors.Setters;
using Honeydew.Extractors.Visitors;
using Honeydew.Models.CSharp;
using Honeydew.Models.Types;
using HoneydewCore.Logging;
using Moq;
using Xunit;

namespace Honeydew.Extractors.CSharp.Tests.Visitors.Others.GotoStatement;

public class GotoStatementVisitorTests
{
    private readonly CSharpFactExtractor _factExtractor;
    private readonly Mock<ILogger> _loggerMock = new();
    private readonly CSharpSyntacticModelCreator _syntacticModelCreator = new();
    private readonly CSharpSemanticModelCreator _semanticModelCreator = new(new CSharpCompilationMaker());

    public GotoStatementVisitorTests()
    {
        var compositeVisitor = new CompositeVisitor(_loggerMock.Object);

        var gotoStatementVisitor = new GotoStatementVisitor();
        var localFunctionsSetterClassVisitor = new LocalFunctionsSetterClassVisitor(_loggerMock.Object,
            new List<ICSharpLocalFunctionVisitor>
            {
                new LocalFunctionInfoVisitor(_loggerMock.Object, new List<ICSharpLocalFunctionVisitor>
                {
                    gotoStatementVisitor
                }),
                gotoStatementVisitor
            });

        compositeVisitor.Add(new ClassSetterCompilationUnitVisitor(_loggerMock.Object, new List<IClassVisitor>
        {
            new BaseInfoClassVisitor(),
            new ConstructorSetterClassVisitor(_loggerMock.Object, new List<ICSharpConstructorVisitor>
            {
                new ConstructorInfoVisitor(),
                gotoStatementVisitor,
                localFunctionsSetterClassVisitor,
            }),
            new MethodSetterClassVisitor(_loggerMock.Object, new List<IMethodVisitor>
            {
                new MethodInfoVisitor(),
                gotoStatementVisitor,
                localFunctionsSetterClassVisitor,
            }),
            new DestructorSetterClassVisitor(_loggerMock.Object, new List<IDestructorVisitor>
            {
                new DestructorInfoVisitor(),
                gotoStatementVisitor,
            }),
            new PropertySetterClassVisitor(_loggerMock.Object, new List<IPropertyVisitor>
            {
                new PropertyInfoVisitor(),
                new MethodAccessorSetterPropertyVisitor(_loggerMock.Object, new List<ICSharpMethodAccessorVisitor>
                {
                    new MethodInfoVisitor(),
                    gotoStatementVisitor,
                    localFunctionsSetterClassVisitor,
                })
            })
        }));

        _factExtractor = new CSharpFactExtractor(compositeVisitor);
    }

    [Theory]
    [FileData("TestData/GotoInConstructorTests.txt")]
    public void Extract_ShouldExtractGotoStatements_WhenProvidedConstructor(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classModel = (ClassModel)classTypes[0];

        TestGotoStatementInMetrics(classModel.Constructors[0].Metrics);
    }

    [Theory]
    [FileData("TestData/GotoInDestructorTests.txt")]
    public void Extract_ShouldExtractGotoStatements_WhenProvidedDestructor(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classModel = (ClassModel)classTypes[0];

        TestGotoStatementInMetrics(classModel.Destructor.Metrics);
    }

    [Theory]
    [FileData("TestData/GotoInMethodTests.txt")]
    public void Extract_ShouldExtractGotoStatements_WhenProvidedMethod(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classModel = (ClassModel)classTypes[0];

        TestGotoStatementInMetrics(classModel.Methods[0].Metrics);
    }

    [Theory]
    [FileData("TestData/GotoInPropertyAccessorTests.txt")]
    public void Extract_ShouldExtractGotoStatements_WhenProvidedPropertyAccessors(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classModel = (ClassModel)classTypes[0];

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

        var classModel = (ClassModel)classTypes[0];
        var methodModel = (MethodModel)classModel.Methods[0];

        Assert.Single(methodModel.Metrics);
        Assert.Equal(0, (int)methodModel.Metrics[0].Value);

        TestGotoStatementInMetrics(methodModel.LocalFunctions[0].Metrics);
        TestGotoStatementInMetrics(methodModel.LocalFunctions[0].LocalFunctions[0].Metrics);
    }

    private static void TestGotoStatementInMetrics(IList<MetricModel> metrics)
    {
        Assert.Single(metrics);
        Assert.Equal(typeof(GotoStatementVisitor).FullName, metrics[0].ExtractorName);
        Assert.Equal(typeof(int).FullName, metrics[0].ValueType);
        Assert.Equal(3, (int)metrics[0].Value);
    }
}
