using System.Collections.Generic;
using HoneydewCore.Logging;
using HoneydewCore.Processors;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewExtractors.Core.Metrics.Visitors.Constructors;
using HoneydewExtractors.Core.Metrics.Visitors.LocalVariables;
using HoneydewExtractors.Core.Metrics.Visitors.Methods;
using HoneydewExtractors.Core.Metrics.Visitors.Properties;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewExtractors.CSharp.Metrics.Extraction.Class;
using HoneydewExtractors.CSharp.Metrics.Extraction.Class.Relations;
using HoneydewExtractors.CSharp.Metrics.Extraction.Common;
using HoneydewExtractors.CSharp.Metrics.Extraction.CompilationUnit;
using HoneydewExtractors.CSharp.Metrics.Extraction.Constructor;
using HoneydewExtractors.CSharp.Metrics.Extraction.LocalVariables;
using HoneydewExtractors.CSharp.Metrics.Extraction.Method;
using HoneydewExtractors.CSharp.Metrics.Extraction.Property;
using HoneydewExtractors.CSharp.Metrics.Visitors.Method;
using HoneydewExtractors.CSharp.Metrics.Visitors.Method.LocalFunctions;
using HoneydewModels.CSharp;
using HoneydewModels.Types;
using Moq;
using Xunit;

namespace HoneydewExtractorsTests.CSharp.Metrics.Extraction.ClassLevel.RelationMetric;

public class CSharpLocalVariablesRelationVisitorTests
{
    private readonly LocalVariablesRelationVisitor _sut;
    private readonly CSharpFactExtractor _factExtractor;
    private readonly Mock<ILogger> _loggerMock = new();
    private readonly CSharpSyntacticModelCreator _syntacticModelCreator = new();
    private readonly CSharpSemanticModelCreator _semanticModelCreator = new(new CSharpCompilationMaker());

    public CSharpLocalVariablesRelationVisitorTests()
    {
        _sut = new LocalVariablesRelationVisitor();

        var compositeVisitor = new CompositeVisitor();
        var localVariablesTypeSetterVisitor = new LocalVariablesTypeSetterVisitor(new List<ILocalVariablesVisitor>
        {
            new LocalVariableInfoVisitor()
        });
        var localFunctionsSetterClassVisitor = new LocalFunctionsSetterClassVisitor(new List<ILocalFunctionVisitor>
        {
            new LocalFunctionInfoVisitor(new List<ILocalFunctionVisitor>
            {
                localVariablesTypeSetterVisitor
            }),
            localVariablesTypeSetterVisitor
        });
        var methodInfoVisitor = new MethodInfoVisitor();
        compositeVisitor.Add(new ClassSetterCompilationUnitVisitor(new List<ICSharpClassVisitor>
        {
            new BaseInfoClassVisitor(),
            new MethodSetterClassVisitor(new List<IMethodVisitor>
            {
                methodInfoVisitor,
                localFunctionsSetterClassVisitor,
                localVariablesTypeSetterVisitor
            }),
            new ConstructorSetterClassVisitor(new List<IConstructorVisitor>
            {
                new ConstructorInfoVisitor(),
                localFunctionsSetterClassVisitor,
                localVariablesTypeSetterVisitor
            }),
            new PropertySetterClassVisitor(new List<IPropertyVisitor>
            {
                new PropertyInfoVisitor(),
                new MethodAccessorSetterPropertyVisitor(new List<IMethodVisitor>
                {
                    methodInfoVisitor,
                    localFunctionsSetterClassVisitor,
                    localVariablesTypeSetterVisitor
                })
            })
        }));

        compositeVisitor.Accept(new LoggerSetterVisitor(_loggerMock.Object));

        _factExtractor = new CSharpFactExtractor(compositeVisitor);
    }

    [Fact]
    public void PrettyPrint_ShouldReturnLocalVariablesDependency()
    {
        Assert.Equal("Local Variables Dependency", _sut.PrettyPrint());
    }

    [Theory]
    [FileData("TestData/CSharp/Metrics/Extraction/Relations/ClassWithTwoFunctions.txt")]
    public void Extract_ShouldHaveNoLocalVariables_WhenClassHasMethodsThatDontUseLocalVariables(
        string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var compilationUnitType = _factExtractor.Extract(syntaxTree, semanticModel);
        RepositoryModelToReferenceRepositoryModelProcessor processor = new();
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
        var classModel = repositoryModel.Projects[0].Files[0].Classes[0];


        _sut.Visit(classModel);


        Assert.Equal(1, classModel.Metrics.Count);
        Assert.Equal("HoneydewExtractors.CSharp.Metrics.Extraction.Class.Relations.LocalVariablesRelationVisitor",
            classModel.Metrics[0].ExtractorName);
        Assert.Equal("System.Collections.Generic.Dictionary`2[System.String,System.Int32]",
            classModel.Metrics[0].ValueType);

        var dictionary = (Dictionary<string, int>)classModel.Metrics[0].Value;
        Assert.Empty(dictionary);
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
        RepositoryModelToReferenceRepositoryModelProcessor processor = new();
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
        var classModel = repositoryModel.Projects[0].Files[0].Classes[0];


        _sut.Visit(classModel);


        Assert.Equal(1, classModel.Metrics.Count);
        Assert.Equal("HoneydewExtractors.CSharp.Metrics.Extraction.Class.Relations.LocalVariablesRelationVisitor",
            classModel.Metrics[0].ExtractorName);
        Assert.Equal("System.Collections.Generic.Dictionary`2[System.String,System.Int32]",
            classModel.Metrics[0].ValueType);

        var dependencies = (Dictionary<string, int>)classModel.Metrics[0].Value;

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
        RepositoryModelToReferenceRepositoryModelProcessor processor = new();
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
        var classModel = repositoryModel.Projects[0].Files[0].Classes[0];


        _sut.Visit(classModel);


        Assert.Equal(1, classModel.Metrics.Count);
        Assert.Equal("HoneydewExtractors.CSharp.Metrics.Extraction.Class.Relations.LocalVariablesRelationVisitor",
            classModel.Metrics[0].ExtractorName);
        Assert.Equal("System.Collections.Generic.Dictionary`2[System.String,System.Int32]",
            classModel.Metrics[0].ValueType);

        var dictionary = (Dictionary<string, int>)classModel.Metrics[0].Value;
        Assert.Empty(dictionary);
    }

    [Theory]
    [FileData(
        "TestData/CSharp/Metrics/Extraction/Relations/LocalVariablesRelations/InterfaceWithMethodsWithNonPrimitiveParams.txt")]
    public void Extract_ShouldHaveNoDependencies_WhenGivenAnInterface(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var compilationUnitType = _factExtractor.Extract(syntaxTree, semanticModel);
        RepositoryModelToReferenceRepositoryModelProcessor processor = new();
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
        var classModel = repositoryModel.Projects[0].Files[0].Classes[0];


        _sut.Visit(classModel);


        Assert.Equal(1, classModel.Metrics.Count);
        Assert.Equal("HoneydewExtractors.CSharp.Metrics.Extraction.Class.Relations.LocalVariablesRelationVisitor",
            classModel.Metrics[0].ExtractorName);
        Assert.Equal("System.Collections.Generic.Dictionary`2[System.String,System.Int32]",
            classModel.Metrics[0].ValueType);

        var dictionary = (Dictionary<string, int>)classModel.Metrics[0].Value;
        Assert.Empty(dictionary);
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
        RepositoryModelToReferenceRepositoryModelProcessor processor = new();
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
        var classModel = repositoryModel.Projects[0].Files[0].Classes[0];


        _sut.Visit(classModel);


        Assert.Equal(1, classModel.Metrics.Count);
        Assert.Equal("HoneydewExtractors.CSharp.Metrics.Extraction.Class.Relations.LocalVariablesRelationVisitor",
            classModel.Metrics[0].ExtractorName);
        Assert.Equal("System.Collections.Generic.Dictionary`2[System.String,System.Int32]",
            classModel.Metrics[0].ValueType);

        var dependencies = (Dictionary<string, int>)classModel.Metrics[0].Value;

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
        RepositoryModelToReferenceRepositoryModelProcessor processor = new();
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
        var classModel = repositoryModel.Projects[0].Files[0].Classes[0];


        _sut.Visit(classModel);


        Assert.Equal(1, classModel.Metrics.Count);
        Assert.Equal("HoneydewExtractors.CSharp.Metrics.Extraction.Class.Relations.LocalVariablesRelationVisitor",
            classModel.Metrics[0].ExtractorName);
        Assert.Equal("System.Collections.Generic.Dictionary`2[System.String,System.Int32]",
            classModel.Metrics[0].ValueType);

        var dependencies = (Dictionary<string, int>)classModel.Metrics[0].Value;

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
        RepositoryModelToReferenceRepositoryModelProcessor processor = new();
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
        var classModel = repositoryModel.Projects[0].Files[0].Classes[0];


        _sut.Visit(classModel);


        Assert.Equal(1, classModel.Metrics.Count);
        Assert.Equal("HoneydewExtractors.CSharp.Metrics.Extraction.Class.Relations.LocalVariablesRelationVisitor",
            classModel.Metrics[0].ExtractorName);
        Assert.Equal("System.Collections.Generic.Dictionary`2[System.String,System.Int32]",
            classModel.Metrics[0].ValueType);

        var dependencies = (Dictionary<string, int>)classModel.Metrics[0].Value;

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
        RepositoryModelToReferenceRepositoryModelProcessor processor = new();
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
        var classModel1 = repositoryModel.Projects[0].Files[0].Classes[0];
        var classModel2 = repositoryModel.Projects[0].Files[0].Classes[1];


        _sut.Visit(classModel1);
        _sut.Visit(classModel2);


        Assert.Equal(2, repositoryModel.Projects[0].Files[0].Classes.Count);

        Assert.Equal(1, classModel1.Metrics.Count);
        Assert.Equal("HoneydewExtractors.CSharp.Metrics.Extraction.Class.Relations.LocalVariablesRelationVisitor",
            classModel1.Metrics[0].ExtractorName);
        Assert.Equal("System.Collections.Generic.Dictionary`2[System.String,System.Int32]",
            classModel1.Metrics[0].ValueType);

        var dependencies1 = (Dictionary<string, int>)classModel1.Metrics[0].Value;

        Assert.Equal(2, dependencies1.Count);
        Assert.Equal(1, dependencies1["CSharpMetricExtractor"]);
        Assert.Equal(1, dependencies1["int"]);


        Assert.Equal(1, classModel2.Metrics.Count);
        Assert.Equal("HoneydewExtractors.CSharp.Metrics.Extraction.Class.Relations.LocalVariablesRelationVisitor",
            classModel2.Metrics[0].ExtractorName);
        Assert.Equal("System.Collections.Generic.Dictionary`2[System.String,System.Int32]",
            classModel2.Metrics[0].ValueType);

        var dependencies2 = (Dictionary<string, int>)classModel2.Metrics[0].Value;

        Assert.Equal(3, dependencies2.Count);
        Assert.Equal(1, dependencies2["CSharpMetricExtractor"]);
        Assert.Equal(5, dependencies2["int"]);
        Assert.Equal(1, dependencies2["double"]);
    }
}
