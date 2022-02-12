using System.Collections.Generic;
using HoneydewCore.Logging;
using HoneydewCore.Processors;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewExtractors.Core.Metrics.Visitors.Properties;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewExtractors.CSharp.Metrics.Extraction.Class;
using HoneydewExtractors.CSharp.Metrics.Extraction.Class.Relations;
using HoneydewExtractors.CSharp.Metrics.Extraction.CompilationUnit;
using HoneydewExtractors.CSharp.Metrics.Extraction.Property;
using HoneydewModels.CSharp;
using HoneydewModels.Types;
using Moq;
using Xunit;

namespace HoneydewExtractorsTests.CSharp.Metrics.Extraction.ClassLevel.RelationMetric;

public class CSharpPropertiesRelationMetricTests
{
    private readonly PropertiesRelationVisitor _sut;
    private readonly CSharpFactExtractor _factExtractor;
    private readonly Mock<ILogger> _loggerMock = new();
    private readonly CSharpSyntacticModelCreator _syntacticModelCreator = new();
    private readonly CSharpSemanticModelCreator _semanticModelCreator = new(new CSharpCompilationMaker());

    public CSharpPropertiesRelationMetricTests()
    {
        _sut = new PropertiesRelationVisitor();

        var compositeVisitor = new CompositeVisitor();

        compositeVisitor.Add(new ClassSetterCompilationUnitVisitor(new List<ICSharpClassVisitor>
        {
            new BaseInfoClassVisitor(),
            new PropertySetterClassVisitor(new List<IPropertyVisitor>
            {
                new PropertyInfoVisitor()
            })
        }));

        compositeVisitor.Accept(new LoggerSetterVisitor(_loggerMock.Object));

        _factExtractor = new CSharpFactExtractor(compositeVisitor);
    }

    [Fact]
    public void PrettyPrint_ShouldReturnReturnValueDependency()
    {
        Assert.Equal("Properties Dependency", _sut.PrettyPrint());
    }

    [Theory]
    [InlineData("class")]
    [InlineData("interface")]
    [InlineData("record")]
    [InlineData("struct")]
    public void Extract_ShouldHavePrimitiveProperties_WhenClassHasPropertiesOfPrimitiveTypes(string classType)
    {
        var fileContent = $@"using System;

                                     namespace App
                                     {{                                       
                                         {classType} MyClass
                                         {{                                           
                                             public int Foo {{get;set;}}

                                             public float Bar {{get;private set;}}

                                             public int Zoo {{set;}}

                                             public string Goo {{get;set;}}
                                         }}
                                     }}";

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
        Assert.Equal("HoneydewExtractors.CSharp.Metrics.Extraction.Class.Relations.PropertiesRelationVisitor",
            classModel.Metrics[0].ExtractorName);
        Assert.Equal("System.Collections.Generic.Dictionary`2[System.String,System.Int32]",
            classModel.Metrics[0].ValueType);

        var dependencies = (Dictionary<string, int>)classModel.Metrics[0].Value;

        Assert.Equal(3, dependencies.Count);
        Assert.Equal(2, dependencies["int"]);
        Assert.Equal(1, dependencies["float"]);
        Assert.Equal(1, dependencies["string"]);
    }

    [Theory]
    [InlineData("class")]
    [InlineData("interface")]
    [InlineData("record")]
    [InlineData("struct")]
    public void Extract_ShouldHavePrimitiveProperties_WhenClassHasEventPropertiesOfPrimitiveTypes(string classType)
    {
        var fileContent = $@"using System;
                                     namespace App
                                     {{                                       
                                        {classType} MyClass
                                        {{
                                            public event Func<int> Foo {{add{{}}remove{{}}}}
                                            
                                            public event Action<string> Bar {{add{{}}remove{{}}}}
                                        }}
                                     }}";

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
        Assert.Equal("HoneydewExtractors.CSharp.Metrics.Extraction.Class.Relations.PropertiesRelationVisitor",
            classModel.Metrics[0].ExtractorName);
        Assert.Equal("System.Collections.Generic.Dictionary`2[System.String,System.Int32]",
            classModel.Metrics[0].ValueType);

        var dependencies = (Dictionary<string, int>)classModel.Metrics[0].Value;

        Assert.Equal(2, dependencies.Count);
        Assert.Equal(1, dependencies["System.Func<int>"]);
        Assert.Equal(1, dependencies["System.Action<string>"]);
    }

    [Theory]
    [InlineData("class")]
    [InlineData("interface")]
    [InlineData("record")]
    [InlineData("struct")]
    public void Extract_ShouldHaveDependenciesProperties_WhenClassHasProperties(string classType)
    {
        var fileContent = $@"using System;
                                     using HoneydewCore.Extractors;
                                     using HoneydewCore.Extractors.Metrics;
                                     using HoneydewCore.Extractors.Metrics.SemanticMetrics;
                                     namespace App
                                     {{                                       
                                         public {classType} IInterface
                                         {{                                           
                                             public CSharpMetricExtractor Foo {{get;}}

                                             public CSharpMetricExtractor Foo2 {{get; private set;}}

                                             public IFactExtractor Bar {{get;}}
                                         }}
                                     }}";

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
        Assert.Equal("HoneydewExtractors.CSharp.Metrics.Extraction.Class.Relations.PropertiesRelationVisitor",
            classModel.Metrics[0].ExtractorName);
        Assert.Equal("System.Collections.Generic.Dictionary`2[System.String,System.Int32]",
            classModel.Metrics[0].ValueType);

        var dependencies = (Dictionary<string, int>)classModel.Metrics[0].Value;

        Assert.Equal(2, dependencies.Count);
        Assert.Equal(2, dependencies["CSharpMetricExtractor"]);
        Assert.Equal(1, dependencies["IFactExtractor"]);
    }

    [Theory]
    [InlineData("class")]
    [InlineData("interface")]
    [InlineData("record")]
    [InlineData("struct")]
    public void Extract_ShouldHaveDependenciesEventProperties_WhenClassHasEventProperties(string classType)
    {
        var fileContent = $@"using System;
                                     using HoneydewCore.Extractors;
                                     using HoneydewCore.Extractors.Metrics;
                                     namespace App
                                     {{                                       
                                         public {classType} IInterface
                                         {{                                           
                                             public event Func<CSharpMetricExtractor> Foo {{add{{}} remove{{}}}}

                                             public event Action<IFactExtractor> Bar {{add{{}} remove{{}}}}

                                             public event Func<IFactExtractor,CSharpMetricExtractor> Goo {{add{{}} remove{{}}}}
                                         }}
                                     }}";

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
        Assert.Equal("HoneydewExtractors.CSharp.Metrics.Extraction.Class.Relations.PropertiesRelationVisitor",
            classModel.Metrics[0].ExtractorName);
        Assert.Equal("System.Collections.Generic.Dictionary`2[System.String,System.Int32]",
            classModel.Metrics[0].ValueType);

        var dependencies = (Dictionary<string, int>)classModel.Metrics[0].Value;

        Assert.Equal(3, dependencies.Count);
        Assert.Equal(1, dependencies["System.Func<CSharpMetricExtractor>"]);
        Assert.Equal(1, dependencies["System.Action<IFactExtractor>"]);
        Assert.Equal(1, dependencies["System.Func<IFactExtractor, CSharpMetricExtractor>"]);
    }
}
