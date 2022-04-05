using System.Collections.Generic;
using Honeydew.PostExtraction.ReferenceRelations;
using HoneydewCore.Logging;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewExtractors.Core.Metrics.Visitors.Methods;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewExtractors.CSharp.Metrics.Extraction.Class;
using HoneydewExtractors.CSharp.Metrics.Extraction.CompilationUnit;
using HoneydewExtractors.CSharp.Metrics.Extraction.Method;
using HoneydewModels.CSharp;
using HoneydewModels.Types;
using HoneydewScriptBeePlugin.Loaders;
using Moq;
using Xunit;

namespace HoneydewTests.RelationMetric;

public class CSharpReturnValueRelationMetricTests
{
    private readonly ReturnValueRelationVisitor _sut;
    private readonly CSharpFactExtractor _factExtractor;
    private readonly Mock<ILogger> _loggerMock = new();
    private readonly Mock<IProgressLogger> _progressLoggerMock = new();
    private readonly CSharpSyntacticModelCreator _syntacticModelCreator = new();
    private readonly CSharpSemanticModelCreator _semanticModelCreator = new(new CSharpCompilationMaker());

    public CSharpReturnValueRelationMetricTests()
    {
        _sut = new ReturnValueRelationVisitor(new AddNameStrategy());

        var compositeVisitor = new CompositeVisitor();

        compositeVisitor.Add(new ClassSetterCompilationUnitVisitor(new List<ICSharpClassVisitor>
        {
            new BaseInfoClassVisitor(),
            new MethodSetterClassVisitor(new List<IMethodVisitor>
            {
                new MethodInfoVisitor(),
            })
        }));

        compositeVisitor.Accept(new LoggerSetterVisitor(_loggerMock.Object));

        _factExtractor = new CSharpFactExtractor(compositeVisitor);
    }

    [Theory]
    [FileData("TestData/CSharp/Metrics/Extraction/Relations/ClassWithTwoFunctions.txt")]
    public void Extract_ShouldHaveVoidReturnValues_WhenClassHasMethodsThatReturnVoid(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var compilationUnitType = _factExtractor.Extract(syntaxTree, semanticModel);
        RepositoryModelToReferenceRepositoryModelProcessor processor = new(_loggerMock.Object, _progressLoggerMock.Object);
        var repositoryModel = processor.Process(new RepositoryModel
        {
            Projects =
            {
                new ProjectModel("")
                {
                    CompilationUnits = new List<ICompilationUnitType>
                    {
                        compilationUnitType
                    }
                }
            }
        });
        var classModel = repositoryModel.Projects[0].Files[0].Entities[0];


        var dependencies = _sut.Visit(classModel);

        Assert.Single(dependencies);
        Assert.Equal(2, dependencies["void"]);
    }

    [Theory]
    [FileData(
        "TestData/CSharp/Metrics/Extraction/Relations/ReturnValueRelations/ClassWithMethodsWithPrimitiveReturnValue.txt")]
    public void Extract_ShouldHavePrimitiveReturnValues_WhenClassHasMethodsThatReturnPrimitiveValues(
        string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var compilationUnitType = _factExtractor.Extract(syntaxTree, semanticModel);
        RepositoryModelToReferenceRepositoryModelProcessor processor = new(_loggerMock.Object, _progressLoggerMock.Object);
        var repositoryModel = processor.Process(new RepositoryModel
        {
            Projects =
            {
                new ProjectModel("")
                {
                    CompilationUnits = new List<ICompilationUnitType>
                    {
                        compilationUnitType
                    }
                }
            }
        });
        var classModel = repositoryModel.Projects[0].Files[0].Entities[0];


        var dependencies = _sut.Visit(classModel);

        Assert.Equal(3, dependencies.Count);
        Assert.Equal(2, dependencies["int"]);
        Assert.Equal(1, dependencies["float"]);
        Assert.Equal(1, dependencies["string"]);
    }

    [Theory]
    [FileData(
        "TestData/CSharp/Metrics/Extraction/Relations/ReturnValueRelations/InterfaceWithMethodsWithPrimitiveReturnValue.txt")]
    public void Extract_ShouldHavePrimitiveReturnValues_WhenInterfaceHasMethodsWithPrimitiveReturnValues(
        string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var compilationUnitType = _factExtractor.Extract(syntaxTree, semanticModel);
        RepositoryModelToReferenceRepositoryModelProcessor processor = new(_loggerMock.Object, _progressLoggerMock.Object);
        var repositoryModel = processor.Process(new RepositoryModel
        {
            Projects =
            {
                new ProjectModel("")
                {
                    CompilationUnits = new List<ICompilationUnitType>
                    {
                        compilationUnitType
                    }
                }
            }
        });
        var classModel = repositoryModel.Projects[0].Files[0].Entities[0];


        var dependencies = _sut.Visit(classModel);

        Assert.Equal(4, dependencies.Count);
        Assert.Equal(1, dependencies["int"]);
        Assert.Equal(1, dependencies["float"]);
        Assert.Equal(1, dependencies["string"]);
        Assert.Equal(1, dependencies["void"]);
    }


    [Theory]
    [FileData(
        "TestData/CSharp/Metrics/Extraction/Relations/ReturnValueRelations/InterfaceWithMethodsWithNonPrimitiveReturnValue.txt")]
    public void Extract_ShouldHaveDependenciesReturnValues_WhenInterfaceHasMethodsWithDependenciesReturnValues(
        string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var compilationUnitType = _factExtractor.Extract(syntaxTree, semanticModel);
        RepositoryModelToReferenceRepositoryModelProcessor processor = new(_loggerMock.Object, _progressLoggerMock.Object);
        var repositoryModel = processor.Process(new RepositoryModel
        {
            Projects =
            {
                new ProjectModel("")
                {
                    CompilationUnits = new List<ICompilationUnitType>
                    {
                        compilationUnitType
                    }
                }
            }
        });
        var classModel = repositoryModel.Projects[0].Files[0].Entities[0];


        var dependencies = _sut.Visit(classModel);

        Assert.Equal(2, dependencies.Count);
        Assert.Equal(2, dependencies["CSharpMetricExtractor"]);
        Assert.Equal(1, dependencies["IFactExtractor"]);
    }

    [Theory]
    [FileData(
        "TestData/CSharp/Metrics/Extraction/Relations/ReturnValueRelations/ClassWithMethodsWithNonPrimitiveReturnValue.txt")]
    public void Extract_ShouldHaveDependenciesReturnValues_WhenClassHasMethodsWithDependenciesReturnValues(
        string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var compilationUnitType = _factExtractor.Extract(syntaxTree, semanticModel);
        RepositoryModelToReferenceRepositoryModelProcessor processor = new(_loggerMock.Object, _progressLoggerMock.Object);
        var repositoryModel = processor.Process(new RepositoryModel
        {
            Projects =
            {
                new ProjectModel("")
                {
                    CompilationUnits = new List<ICompilationUnitType>
                    {
                        compilationUnitType
                    }
                }
            }
        });
        var classModel = repositoryModel.Projects[0].Files[0].Entities[0];


        var dependencies = _sut.Visit(classModel);

        Assert.Equal(2, dependencies.Count);
        Assert.Equal(1, dependencies["CSharpMetricExtractor"]);
        Assert.Equal(2, dependencies["IFactExtractor"]);
    }
}
