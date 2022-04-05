using System.Collections.Generic;
using Honeydew.PostExtraction.ReferenceRelations;
using HoneydewCore.Logging;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewExtractors.Core.Metrics.Visitors.Fields;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewExtractors.CSharp.Metrics.Extraction.Class;
using HoneydewExtractors.CSharp.Metrics.Extraction.CompilationUnit;
using HoneydewExtractors.CSharp.Metrics.Extraction.Field;
using HoneydewModels.CSharp;
using HoneydewModels.Types;
using HoneydewScriptBeePlugin.Loaders;
using Moq;
using Xunit;

namespace HoneydewTests.RelationMetric;

public class CSharpFieldsRelationVisitorTests
{
    private readonly FieldsRelationVisitor _sut;
    private readonly CSharpFactExtractor _factExtractor;
    private readonly Mock<ILogger> _loggerMock = new();
    private readonly Mock<IProgressLogger> _progressLoggerMock = new();
    private readonly CSharpSyntacticModelCreator _syntacticModelCreator = new();
    private readonly CSharpSemanticModelCreator _semanticModelCreator = new(new CSharpCompilationMaker());

    public CSharpFieldsRelationVisitorTests()
    {
        _sut = new FieldsRelationVisitor(new AddNameStrategy());

        var compositeVisitor = new CompositeVisitor();

        compositeVisitor.Add(new ClassSetterCompilationUnitVisitor(new List<ICSharpClassVisitor>
        {
            new BaseInfoClassVisitor(),
            new FieldSetterClassVisitor(new List<IFieldVisitor>
            {
                new FieldInfoVisitor()
            })
        }));

        compositeVisitor.Accept(new LoggerSetterVisitor(_loggerMock.Object));

        _factExtractor = new CSharpFactExtractor(compositeVisitor);
    }

    [Theory]
    [InlineData("class")]
    [InlineData("record")]
    [InlineData("struct")]
    public void Extract_ShouldHavePrimitiveFields_WhenClassHasFieldsOfPrimitiveTypes(string classType)
    {
        var fileContent = $@"using System;

                                     namespace App
                                     {{                                       
                                         {classType} MyClass
                                         {{                                           
                                             public int Foo;

                                             private float Bar;

                                             protected int Zoo;

                                             internal string Goo;
                                         }}
                                     }}";

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
        Assert.Equal(2, dependencies["int"]);
        Assert.Equal(1, dependencies["float"]);
        Assert.Equal(1, dependencies["string"]);
    }

    [Theory]
    [InlineData("class")]
    [InlineData("record")]
    [InlineData("struct")]
    public void Extract_ShouldHavePrimitiveFields_WhenClassHasEventFieldsOfPrimitiveTypes(string classType)
    {
        var fileContent = $@"using System;
                                     namespace App
                                     {{                                       
                                        {classType} MyClass
                                        {{
                                            public event Func<int> Foo;
                                            
                                            public event Action<string> Bar;
                                        }}
                                     }}";

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

        Assert.Equal(2, dependencies.Count);
        Assert.Equal(1, dependencies["System.Func<int>"]);
        Assert.Equal(1, dependencies["System.Action<string>"]);
    }

    [Theory]
    [InlineData("class")]
    [InlineData("record")]
    [InlineData("struct")]
    public void Extract_ShouldHaveDependenciesFields_WhenClassHasFields(string classType)
    {
        var fileContent = $@"using System;
                                     using HoneydewCore.Extractors;
                                     using HoneydewCore.Extractors.Metrics;
                                     using HoneydewCore.Extractors.Metrics.SemanticMetrics;
                                     namespace App
                                     {{                                       
                                         public {classType} IInterface
                                         {{                                           
                                             public CSharpMetricExtractor Foo;

                                             private CSharpMetricExtractor Foo2 ;

                                             protected IFactExtractor Bar;
                                         }}
                                     }}";

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

        Assert.Equal(2, dependencies.Count);
        Assert.Equal(2, dependencies["CSharpMetricExtractor"]);
        Assert.Equal(1, dependencies["IFactExtractor"]);
    }

    [Theory]
    [InlineData("class")]
    [InlineData("record")]
    [InlineData("struct")]
    public void Extract_ShouldHaveDependenciesEventFields_WhenClassHasEventFields(string classType)
    {
        var fileContent = $@"using System;
                                     using HoneydewCore.Extractors;
                                     using HoneydewCore.Extractors.Metrics;
                                     namespace App
                                     {{                                       
                                         public {classType} IInterface
                                         {{                                           
                                             internal event Func<CSharpMetricExtractor> Foo;

                                             public event Action<IFactExtractor> Bar;

                                             private event Func<IFactExtractor,CSharpMetricExtractor> Goo;
                                         }}
                                     }}";

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
        Assert.Equal(1, dependencies["System.Func<CSharpMetricExtractor>"]);
        Assert.Equal(1, dependencies["System.Action<IFactExtractor>"]);
        Assert.Equal(1, dependencies["System.Func<IFactExtractor, CSharpMetricExtractor>"]);
    }
}
