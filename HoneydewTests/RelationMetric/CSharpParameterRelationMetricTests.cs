using System.Collections.Generic;
using Honeydew.PostExtraction.ReferenceRelations;
using HoneydewCore.Logging;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewExtractors.Core.Metrics.Visitors.Constructors;
using HoneydewExtractors.Core.Metrics.Visitors.Methods;
using HoneydewExtractors.Core.Metrics.Visitors.Parameters;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewExtractors.CSharp.Metrics.Extraction.Class;
using HoneydewExtractors.CSharp.Metrics.Extraction.Common;
using HoneydewExtractors.CSharp.Metrics.Extraction.CompilationUnit;
using HoneydewExtractors.CSharp.Metrics.Extraction.Constructor;
using HoneydewExtractors.CSharp.Metrics.Extraction.Method;
using HoneydewExtractors.CSharp.Metrics.Extraction.Parameter;
using HoneydewModels.CSharp;
using HoneydewModels.Types;
using HoneydewScriptBeePlugin.Loaders;
using Moq;
using Xunit;

namespace HoneydewTests.RelationMetric;

public class CSharpParameterRelationMetricTests
{
    private readonly ParameterRelationVisitor _sut;
    private readonly CSharpFactExtractor _factExtractor;
    private readonly Mock<ILogger> _loggerMock = new();
    private readonly Mock<IProgressLogger> _progressLoggerMock = new();
    private readonly CSharpSyntacticModelCreator _syntacticModelCreator = new();
    private readonly CSharpSemanticModelCreator _semanticModelCreator = new(new CSharpCompilationMaker());

    public CSharpParameterRelationMetricTests()
    {
        _sut = new ParameterRelationVisitor(new AddNameStrategy());

        var compositeVisitor = new CompositeVisitor();

        var parameterSetterVisitor = new ParameterSetterVisitor(new List<IParameterVisitor>
        {
            new ParameterInfoVisitor()
        });
        compositeVisitor.Add(new ClassSetterCompilationUnitVisitor(new List<ICSharpClassVisitor>
        {
            new BaseInfoClassVisitor(),
            new MethodSetterClassVisitor(new List<IMethodVisitor>
            {
                new MethodInfoVisitor(),
                parameterSetterVisitor
            }),
            new ConstructorSetterClassVisitor(new List<IConstructorVisitor>
            {
                new ConstructorInfoVisitor(),
                parameterSetterVisitor
            })
        }));

        compositeVisitor.Accept(new LoggerSetterVisitor(_loggerMock.Object));

        _factExtractor = new CSharpFactExtractor(compositeVisitor);
    }

    [Theory]
    [FileData("TestData/CSharp/Metrics/Extraction/Relations/ClassWithTwoFunctions.txt")]
    public void Extract_ShouldHaveNoParameters_WhenClassHasMethodsWithNoParameters(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var compilationUnitType = _factExtractor.Extract(syntaxTree, semanticModel);
        RepositoryModelToReferenceRepositoryModelProcessor processor = new(_loggerMock.Object,
            _progressLoggerMock.Object);
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

        Assert.Empty(classModel.Metrics);
        Assert.Empty(dependencies);
    }

    [Theory]
    [FileData(
        "TestData/CSharp/Metrics/Extraction/Relations/ParameterRelations/ClassWithNoArgConstructor.txt")]
    public void Extract_ShouldHaveNoParameters_WhenClassHasConstructorWithNoParameters(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var compilationUnitType = _factExtractor.Extract(syntaxTree, semanticModel);
        RepositoryModelToReferenceRepositoryModelProcessor processor = new(_loggerMock.Object,
            _progressLoggerMock.Object);
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

        Assert.Empty(classModel.Metrics);
        Assert.Empty(dependencies);
    }

    [Theory]
    [FileData(
        "TestData/CSharp/Metrics/Extraction/Relations/ParameterRelations/ClassWithMethodsWithPrimitiveParams.txt")]
    public void Extract_ShouldHavePrimitiveParameters_WhenClassHasMethodsWithPrimitiveParameters(
        string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var compilationUnitType = _factExtractor.Extract(syntaxTree, semanticModel);
        RepositoryModelToReferenceRepositoryModelProcessor processor = new(_loggerMock.Object,
            _progressLoggerMock.Object);
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
        Assert.Equal(3, dependencies["int"]);
        Assert.Equal(2, dependencies["float"]);
        Assert.Equal(1, dependencies["string"]);
    }

    [Theory]
    [FileData(
        "TestData/CSharp/Metrics/Extraction/Relations/ParameterRelations/InterfaceWithMethodsWithPrimitiveParams.txt")]
    public void Extract_ShouldHavePrimitiveParameters_WhenInterfaceHasMethodsWithPrimitiveParameters(
        string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var compilationUnitType = _factExtractor.Extract(syntaxTree, semanticModel);
        RepositoryModelToReferenceRepositoryModelProcessor processor = new(_loggerMock.Object,
            _progressLoggerMock.Object);
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
        Assert.Equal(3, dependencies["int"]);
        Assert.Equal(2, dependencies["float"]);
        Assert.Equal(1, dependencies["string"]);
    }

    [Theory]
    [FileData(
        "TestData/CSharp/Metrics/Extraction/Relations/ParameterRelations/InterfaceWithMethodsWithNonPrimitiveParams.txt")]
    public void Extract_ShouldHaveDependenciesParameters_WhenInterfaceHasMethodsWithDependenciesParameters(
        string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var compilationUnitType = _factExtractor.Extract(syntaxTree, semanticModel);
        RepositoryModelToReferenceRepositoryModelProcessor processor = new(_loggerMock.Object,
            _progressLoggerMock.Object);
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
        Assert.Equal(2, dependencies["CSharpMetricExtractor"]);
        Assert.Equal(1, dependencies["IFactExtractor"]);
        Assert.Equal(1, dependencies["int"]);
    }

    [Theory]
    [FileData(
        "TestData/CSharp/Metrics/Extraction/Relations/ParameterRelations/ClassWithMethodsWithNonPrimitiveParams.txt")]
    public void Extract_ShouldHaveDependenciesParameters_WhenClassHasMethodsWithDependenciesParameters(
        string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var compilationUnitType = _factExtractor.Extract(syntaxTree, semanticModel);
        RepositoryModelToReferenceRepositoryModelProcessor processor = new(_loggerMock.Object,
            _progressLoggerMock.Object);
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
        Assert.Equal(2, dependencies["CSharpMetricExtractor"]);
        Assert.Equal(1, dependencies["IFactExtractor"]);
        Assert.Equal(2, dependencies["int"]);
        Assert.Equal(1, dependencies["string"]);
    }

    [Theory]
    [FileData(
        "TestData/CSharp/Metrics/Extraction/Relations/ParameterRelations/ClassWithConstructorsWithNonPrimitiveParams.txt")]
    public void Extract_ShouldHaveDependenciesParameters_WhenClassHasConstructorWithDependenciesParameters(
        string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var compilationUnitType = _factExtractor.Extract(syntaxTree, semanticModel);
        RepositoryModelToReferenceRepositoryModelProcessor processor = new(_loggerMock.Object,
            _progressLoggerMock.Object);
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
        Assert.Equal(2, dependencies["CSharpMetricExtractor"]);
        Assert.Equal(1, dependencies["IFactExtractor"]);
        Assert.Equal(2, dependencies["int"]);
        Assert.Equal(1, dependencies["string"]);
    }
}
