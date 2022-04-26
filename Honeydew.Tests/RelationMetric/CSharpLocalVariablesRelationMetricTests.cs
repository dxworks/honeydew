using System.Collections.Generic;
using Honeydew.Extractors.CSharp;
using Honeydew.Extractors.CSharp.Visitors.Concrete;
using Honeydew.Extractors.CSharp.Visitors.Setters;
using Honeydew.Extractors.Dotnet;
using Honeydew.Extractors.Visitors;
using Honeydew.Logging;
using Honeydew.Models;
using Honeydew.Models.Types;
using Honeydew.PostExtraction.ReferenceRelations;
using Honeydew.ScriptBeePlugin.Loaders;
using Moq;
using Xunit;

namespace Honeydew.Tests.RelationMetric;

public class CSharpLocalVariablesRelationVisitorTests
{
    private readonly LocalVariablesRelationVisitor _sut;
    private readonly CSharpFactExtractor _factExtractor;
    private readonly Mock<ILogger> _loggerMock = new();
    private readonly Mock<IProgressLogger> _progressLoggerMock = new();
    private readonly CSharpSyntacticModelCreator _syntacticModelCreator = new();
    private readonly DotnetSemanticModelCreator _semanticModelCreator = new(new CSharpCompilationMaker());

    public CSharpLocalVariablesRelationVisitorTests()
    {
        _sut = new LocalVariablesRelationVisitor(new AddNameStrategy());

        var returnValueSetterVisitor = new CSharpReturnValueSetterVisitor(_loggerMock.Object,
            new List<ITypeVisitor<IReturnValueType>>
            {
                new ReturnValueInfoVisitor()
            });
        var localVariablesTypeSetterVisitor = new CSharpLocalVariablesTypeSetterVisitor(_loggerMock.Object,
            new List<ITypeVisitor<ILocalVariableType>>
            {
                new LocalVariableInfoVisitor()
            });
        var localFunctionsSetterClassVisitor = new CSharpLocalFunctionsSetterClassVisitor(_loggerMock.Object,
            new List<ITypeVisitor<IMethodTypeWithLocalFunctions>>
            {
                new LocalFunctionInfoVisitor(_loggerMock.Object, new List<ITypeVisitor<IMethodTypeWithLocalFunctions>>
                {
                    localVariablesTypeSetterVisitor,
                    returnValueSetterVisitor
                }),
                localVariablesTypeSetterVisitor,
                returnValueSetterVisitor
            });
        var methodInfoVisitor = new MethodInfoVisitor();

        var compositeVisitor = new CSharpCompilationUnitCompositeVisitor(_loggerMock.Object,
            new List<ITypeVisitor<ICompilationUnitType>>
            {
                new CSharpClassSetterVisitor(_loggerMock.Object,
                    new List<ITypeVisitor<IMembersClassType>>
                    {
                        new BaseInfoClassVisitor(),
                        new CSharpMethodSetterClassVisitor(_loggerMock.Object, new List<ITypeVisitor<IMethodType>>
                        {
                            methodInfoVisitor,
                            returnValueSetterVisitor,
                            localFunctionsSetterClassVisitor,
                            localVariablesTypeSetterVisitor
                        }),
                        new CSharpConstructorSetterClassVisitor(_loggerMock.Object,
                            new List<ITypeVisitor<IConstructorType>>
                            {
                                new ConstructorInfoVisitor(),
                                localFunctionsSetterClassVisitor,
                                localVariablesTypeSetterVisitor
                            }),
                        new CSharpPropertySetterClassVisitor(_loggerMock.Object, new List<ITypeVisitor<IPropertyType>>
                        {
                            new PropertyInfoVisitor(),
                            new CSharpAccessorMethodSetterPropertyVisitor(_loggerMock.Object,
                                new List<ITypeVisitor<IAccessorMethodType>>
                                {
                                    methodInfoVisitor,
                                    returnValueSetterVisitor,
                                    localFunctionsSetterClassVisitor,
                                    localVariablesTypeSetterVisitor
                                })
                        })
                    })
            });

        _factExtractor = new CSharpFactExtractor(compositeVisitor);
    }

    [Theory]
    [FileData("TestData/CSharp/Metrics/Extraction/Relations/ClassWithTwoFunctions.txt")]
    public void Extract_ShouldHaveNoLocalVariables_WhenClassHasMethodsThatDontUseLocalVariables(
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
                new ProjectModel
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

        Assert.Empty(dependencies);
    }

    [Theory]
    [FileData(
        "TestData/CSharp/Metrics/Extraction/Relations/LocalVariablesRelations/ClassWithPrimitiveLocalVariables.txt")]
    public void Extract_ShouldHavePrimitiveLocalValues_WhenClassHasMethodsThatHaveLocalVariables(
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
                new ProjectModel
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
        Assert.Equal(1, dependencies["float"]);
        Assert.Equal(1, dependencies["string"]);
    }

    [Theory]
    [FileData(
        "TestData/CSharp/Metrics/Extraction/Relations/LocalVariablesRelations/InterfaceWithMethods.txt")]
    public void Extract_ShouldHaveNoPrimitiveLocalVariables_WhenGivenAnInterface(string fileContent)
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
                new ProjectModel
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

        Assert.Empty(dependencies);
    }

    [Theory]
    [FileData(
        "TestData/CSharp/Metrics/Extraction/Relations/LocalVariablesRelations/InterfaceWithMethodsWithNonPrimitiveParams.txt")]
    public void Extract_ShouldHaveNoDependencies_WhenGivenAnInterface(string fileContent)
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
                new ProjectModel
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

        Assert.Empty(dependencies);
    }

    [Theory]
    [FileData(
        "TestData/CSharp/Metrics/Extraction/Relations/LocalVariablesRelations/ClassWithNonPrimitiveLocalVariables.txt")]
    public void Extract_ShouldHaveLocalVariablesDependencies_WhenClassHasMethodsWithNonPrimitiveLocalVariables(
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
                new ProjectModel
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
        Assert.Equal(3, dependencies["IFactExtractor"]);
    }

    [Theory]
    [FileData(
        "TestData/CSharp/Metrics/Extraction/Relations/LocalVariablesRelations/ClassWithConstructorsWithNonPrimitiveLocalVariables.txt")]
    public void Extract_ShouldHaveLocalVariablesDependencies_WhenClassHasConstructorLocalVariables(
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
                new ProjectModel
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
        Assert.Equal(1, dependencies["CSharpMetricExtractor"]);
        Assert.Equal(5, dependencies["int"]);
        Assert.Equal(1, dependencies["double"]);
    }

    [Theory]
    [FileData(
        "TestData/CSharp/Metrics/Extraction/Relations/LocalVariablesRelations/ClassWithNonPrimitiveLocalVariablesInAForLoop.txt")]
    public void
        Extract_ShouldHaveLocalVariablesDependencies_WhenClassHasMethodsWithNonPrimitiveLocalVariablesInAForLoop(
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
                new ProjectModel
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
        Assert.Equal(1, dependencies["int"]);
    }

    [Theory]
    [FileData(
        "TestData/CSharp/Metrics/Extraction/Relations/LocalVariablesRelations/ClassWithNonPrimitiveLocalVariablesOfClassesFromTheSameNamespace.txt")]
    public void
        Extract_ShouldHaveLocalVariablesDependencies_WhenNamespaceHasMultipleClasses(string fileContent)
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
                new ProjectModel
                {
                    CompilationUnits = new List<ICompilationUnitType>
                    {
                        compilationUnitType
                    }
                }
            }
        });
        var classModel1 = repositoryModel.Projects[0].Files[0].Entities[0];
        var classModel2 = repositoryModel.Projects[0].Files[0].Entities[1];


        var dependencies1 = _sut.Visit(classModel1);
        Assert.Equal(2, dependencies1.Count);
        Assert.Equal(1, dependencies1["CSharpMetricExtractor"]);
        Assert.Equal(1, dependencies1["int"]);


        var dependencies2 = _sut.Visit(classModel2);
        Assert.Equal(3, dependencies2.Count);
        Assert.Equal(1, dependencies2["CSharpMetricExtractor"]);
        Assert.Equal(5, dependencies2["int"]);
        Assert.Equal(1, dependencies2["double"]);
    }
}
