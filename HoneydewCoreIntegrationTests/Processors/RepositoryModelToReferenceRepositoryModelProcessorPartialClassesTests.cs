using System.Collections.Generic;
using System.Linq;
using HoneydewCore.Logging;
using HoneydewCore.Processors;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Attributes;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewExtractors.Core.Metrics.Visitors.Constructors;
using HoneydewExtractors.Core.Metrics.Visitors.Destructors;
using HoneydewExtractors.Core.Metrics.Visitors.Fields;
using HoneydewExtractors.Core.Metrics.Visitors.Methods;
using HoneydewExtractors.Core.Metrics.Visitors.Properties;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewExtractors.CSharp.Metrics.Extraction.Attribute;
using HoneydewExtractors.CSharp.Metrics.Extraction.Class;
using HoneydewExtractors.CSharp.Metrics.Extraction.Common;
using HoneydewExtractors.CSharp.Metrics.Extraction.CompilationUnit;
using HoneydewExtractors.CSharp.Metrics.Extraction.Constructor;
using HoneydewExtractors.CSharp.Metrics.Extraction.Destructor;
using HoneydewExtractors.CSharp.Metrics.Extraction.Field;
using HoneydewExtractors.CSharp.Metrics.Extraction.Method;
using HoneydewExtractors.CSharp.Metrics.Extraction.Property;
using HoneydewModels.CSharp;
using Moq;
using Xunit;

namespace HoneydewCoreIntegrationTests.Processors;

public class RepositoryModelToReferenceRepositoryModelProcessorPartialClassesTests
{
    private readonly RepositoryModelToReferenceRepositoryModelProcessor _sut;

    private readonly CSharpFactExtractor _extractor;
    private readonly Mock<ILogger> _loggerMock = new();
    private readonly CSharpSyntacticModelCreator _syntacticModelCreator = new();
    private readonly CSharpSemanticModelCreator _semanticModelCreator = new(new CSharpCompilationMaker());

    public RepositoryModelToReferenceRepositoryModelProcessorPartialClassesTests()
    {
        _sut = new RepositoryModelToReferenceRepositoryModelProcessor();

        var compositeVisitor = new CompositeVisitor();

        compositeVisitor.Add(new ClassSetterCompilationUnitVisitor(new List<ICSharpClassVisitor>
        {
            new BaseInfoClassVisitor(),
            new BaseTypesClassVisitor(),
            new ImportsVisitor(),
            new AttributeSetterVisitor(new List<IAttributeVisitor>
            {
                new AttributeInfoVisitor()
            }),
            new MethodSetterClassVisitor(new List<ICSharpMethodVisitor>
            {
                new MethodInfoVisitor(),
            }),
            new ConstructorSetterClassVisitor(new List<ICSharpConstructorVisitor>
            {
                new ConstructorInfoVisitor(),
            }),
            new FieldSetterClassVisitor(new List<ICSharpFieldVisitor>(
                new List<ICSharpFieldVisitor>
                {
                    new FieldInfoVisitor()
                })),
            new PropertySetterClassVisitor(new List<IPropertyVisitor>
            {
                new PropertyInfoVisitor(),
                new MethodAccessorSetterPropertyVisitor(new List<IMethodVisitor>
                {
                    new MethodInfoVisitor(),
                })
            }),
            new DestructorSetterClassVisitor(new List<IDestructorVisitor>
            {
                new DestructorInfoVisitor(),
            })
        }));

        compositeVisitor.Accept(new LoggerSetterVisitor(_loggerMock.Object));

        _extractor = new CSharpFactExtractor(compositeVisitor);
    }

    [Theory]
    [MultiFileData("TestData/Processors/ReferenceModel/PartialClass1.txt",
        "TestData/Processors/ReferenceModel/PartialClass2.txt")]
    public void Process_ShouldReferenceModelHavePartialClasses_WhenGivenTwoFilesContainingPartialClasses(
        string fileContent1, string fileContent2)
    {
        var repositoryModel = LoadPartialClassesInRepositoryModel(fileContent1, fileContent2);

        var referenceSolutionModel = _sut.Process(repositoryModel);

        var partialClassModel = referenceSolutionModel.Projects[0].Files[0].Classes[0].PartialClass;

        Assert.NotNull(partialClassModel);
        Assert.Equal(partialClassModel, referenceSolutionModel.Projects[0].Files[1].Classes[0].PartialClass);
        Assert.Equal(2, partialClassModel.Classes.Count);
        Assert.Equal("PartialClasses.C1", partialClassModel.Name);
        Assert.Empty(partialClassModel.GenericParameters);
        Assert.Equal("PartialClasses", partialClassModel.Namespace.Name);
        Assert.Equal("class", partialClassModel.ClassType);

        foreach (var classModel in partialClassModel.Classes)
        {
            Assert.Equal("PartialClasses.C1", classModel.Name);
            Assert.Equal("PartialClasses", classModel.Namespace.Name);
            Assert.Equal("partial", classModel.Modifier);
        }
    }

    [Theory]
    [MultiFileData("TestData/Processors/ReferenceModel/PartialClass1.txt",
        "TestData/Processors/ReferenceModel/PartialClass2.txt")]
    public void Process_ShouldPartialClassHaveAllTheMembers_WhenGivenTwoFilesContainingPartialClasses(
        string fileContent1, string fileContent2)
    {
        var repositoryModel = LoadPartialClassesInRepositoryModel(fileContent1, fileContent2);

        var referenceSolutionModel = _sut.Process(repositoryModel);

        var partialClassModel = referenceSolutionModel.Projects[0].Files[0].Classes[0].PartialClass;

        Assert.NotNull(partialClassModel);
        Assert.Equal(2, partialClassModel.Classes.Count);

        Assert.Equal(4, partialClassModel.Methods.Count());
        Assert.NotNull(partialClassModel.Destructor);
        Assert.Single(partialClassModel.Constructors);
        Assert.Single(partialClassModel.Attributes);
        Assert.Equal(8, partialClassModel.Fields.Count());
        Assert.Equal(4, partialClassModel.Properties.Count());
        Assert.Equal(2, partialClassModel.BaseTypes.Count());
        Assert.Equal(5, partialClassModel.Imports.Count());
    }

    private RepositoryModel LoadPartialClassesInRepositoryModel(string fileContent1, string fileContent2)
    {
        var syntaxTree1 = _syntacticModelCreator.Create(fileContent1);
        var semanticModel1 = _semanticModelCreator.Create(syntaxTree1);

        var classTypes1 = _extractor.Extract(syntaxTree1, semanticModel1).ClassTypes;

        var syntaxTree2 = _syntacticModelCreator.Create(fileContent2);
        var semanticModel2 = _semanticModelCreator.Create(syntaxTree2);

        var classTypes2 = _extractor.Extract(syntaxTree2, semanticModel2).ClassTypes;

        return new RepositoryModel
        {
            Projects =
            {
                new ProjectModel
                {
                    Name = "Project1",
                    CompilationUnits =
                    {
                        new CompilationUnitModel
                        {
                            ClassTypes = classTypes1
                        },
                        new CompilationUnitModel
                        {
                            ClassTypes = classTypes2
                        }
                    }
                }
            }
        };
    }
}
