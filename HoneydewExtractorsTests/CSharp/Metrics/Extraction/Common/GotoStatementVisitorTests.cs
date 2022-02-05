using System;
using System.Collections.Generic;
using HoneydewCore.Logging;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewExtractors.Core.Metrics.Visitors.Constructors;
using HoneydewExtractors.Core.Metrics.Visitors.Destructors;
using HoneydewExtractors.Core.Metrics.Visitors.Methods;
using HoneydewExtractors.Core.Metrics.Visitors.Properties;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewExtractors.CSharp.Metrics.Extraction.Class;
using HoneydewExtractors.CSharp.Metrics.Extraction.Common;
using HoneydewExtractors.CSharp.Metrics.Extraction.CompilationUnit;
using HoneydewExtractors.CSharp.Metrics.Extraction.Constructor;
using HoneydewExtractors.CSharp.Metrics.Extraction.Destructor;
using HoneydewExtractors.CSharp.Metrics.Extraction.Method;
using HoneydewExtractors.CSharp.Metrics.Extraction.Property;
using HoneydewExtractors.CSharp.Metrics.Visitors.Method;
using HoneydewExtractors.CSharp.Metrics.Visitors.Method.LocalFunctions;
using HoneydewModels.CSharp;
using Moq;
using Xunit;

namespace HoneydewExtractorsTests.CSharp.Metrics.Extraction.Common;

public class GotoStatementVisitorTests
{
    private readonly CSharpFactExtractor _factExtractor;
    private readonly Mock<ILogger> _loggerMock = new();
    private readonly CSharpSyntacticModelCreator _syntacticModelCreator = new();
    private readonly CSharpSemanticModelCreator _semanticModelCreator = new(new CSharpCompilationMaker());

    public GotoStatementVisitorTests()
    {
        var compositeVisitor = new CompositeVisitor();

        var gotoStatementVisitor = new GotoStatementVisitor();
        var localFunctionsSetterClassVisitor = new LocalFunctionsSetterClassVisitor(
            new List<ICSharpLocalFunctionVisitor>
            {
                new LocalFunctionInfoVisitor(new List<ICSharpLocalFunctionVisitor>
                {
                    gotoStatementVisitor
                }),
                gotoStatementVisitor
            });

        compositeVisitor.Add(new ClassSetterCompilationUnitVisitor(new List<IClassVisitor>
        {
            new BaseInfoClassVisitor(),
            new ConstructorSetterClassVisitor(new List<ICSharpConstructorVisitor>
            {
                new ConstructorInfoVisitor(),
                gotoStatementVisitor,
                localFunctionsSetterClassVisitor,
            }),
            new MethodSetterClassVisitor(new List<IMethodVisitor>
            {
                new MethodInfoVisitor(),
                gotoStatementVisitor,
                localFunctionsSetterClassVisitor,
            }),
            new DestructorSetterClassVisitor(new List<IDestructorVisitor>
            {
                new DestructorInfoVisitor(),
                gotoStatementVisitor,
            }),
            new PropertySetterClassVisitor(new List<IPropertyVisitor>
            {
                new PropertyInfoVisitor(),
                new MethodAccessorSetterPropertyVisitor(new List<ICSharpMethodAccessorVisitor>
                {
                    new MethodInfoVisitor(),
                    gotoStatementVisitor,
                    localFunctionsSetterClassVisitor,
                })
            })
        }));

        compositeVisitor.Accept(new LoggerSetterVisitor(_loggerMock.Object));

        _factExtractor = new CSharpFactExtractor(compositeVisitor);
    }

    [Theory]
    [FileData("TestData/CSharp/Metrics/Extraction/Common/GotoStatement/GotoInConstructorTests.txt")]
    public void Extract_ShouldExtractGotoStatements_WhenProvidedConstructor(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classModel = (ClassModel)classTypes[0];

        TestGotoStatementInMetrics(classModel.Constructors[0].Metrics);
    }

    [Theory]
    [FileData("TestData/CSharp/Metrics/Extraction/Common/GotoStatement/GotoInDestructorTests.txt")]
    public void Extract_ShouldExtractGotoStatements_WhenProvidedDestructor(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classModel = (ClassModel)classTypes[0];

        TestGotoStatementInMetrics(classModel.Destructor.Metrics);
    }

    [Theory]
    [FileData("TestData/CSharp/Metrics/Extraction/Common/GotoStatement/GotoInMethodTests.txt")]
    public void Extract_ShouldExtractGotoStatements_WhenProvidedMethod(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classModel = (ClassModel)classTypes[0];

        TestGotoStatementInMetrics(classModel.Methods[0].Metrics);
    }

    [Theory]
    [FileData("TestData/CSharp/Metrics/Extraction/Common/GotoStatement/GotoInPropertyAccessorTests.txt")]
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
    [FileData("TestData/CSharp/Metrics/Extraction/Common/GotoStatement/GotoInLocalFunctionTests.txt")]
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
        Assert.Equal(nameof(GotoStatementVisitor), metrics[0].ExtractorName);
        Assert.Equal(nameof(Int32), metrics[0].ValueType);
        Assert.Equal(3, (int)metrics[0].Value);
    }
}
