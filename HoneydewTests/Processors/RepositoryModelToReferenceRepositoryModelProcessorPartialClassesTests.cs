using System.Collections.Generic;
using HoneydewCore.Logging;
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
using HoneydewScriptBeePlugin.Loaders;
using HoneydewScriptBeePlugin.Models;
using Moq;
using Xunit;
using ClassModel = HoneydewScriptBeePlugin.Models.ClassModel;
using ProjectModel = HoneydewModels.CSharp.ProjectModel;
using RepositoryModel = HoneydewModels.CSharp.RepositoryModel;

namespace HoneydewTests.Processors;

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

        var partialClassModel1 = referenceSolutionModel.Projects[0].Files[0].Entities[0] as ClassModel;
        var partialClassModel2 = referenceSolutionModel.Projects[0].Files[1].Entities[0] as ClassModel;

        Assert.NotNull(partialClassModel1);
        Assert.NotNull(partialClassModel2);
        Assert.Equal(1, partialClassModel1!.Partials.Count);
        Assert.Equal(1, partialClassModel2!.Partials.Count);
        Assert.Equal(partialClassModel1.Partials[0], partialClassModel2);
        Assert.Equal(partialClassModel2.Partials[0], partialClassModel1);
        Assert.Equal("PartialClasses.C1", partialClassModel1.Name);
        Assert.Equal("PartialClasses.C1", partialClassModel2.Name);
        Assert.Empty(partialClassModel1.GenericParameters);
        Assert.Empty(partialClassModel2.GenericParameters);
        Assert.Equal("PartialClasses", partialClassModel1.Namespace.Name);
        Assert.Equal("PartialClasses", partialClassModel2.Namespace.Name);
        Assert.Equal(ClassType.Class, partialClassModel1.Type);
        Assert.Equal(ClassType.Class, partialClassModel2.Type);
        Assert.True(partialClassModel1.IsPartial);
        Assert.True(partialClassModel2.IsPartial);
        Assert.Contains(Modifier.Partial, partialClassModel1.Modifiers);
        Assert.Contains(Modifier.Partial, partialClassModel2.Modifiers);
        Assert.Equal("partial", partialClassModel1.Modifier);
        Assert.Equal("partial", partialClassModel2.Modifier);
    }

    [Theory]
    [MultiFileData("TestData/Processors/ReferenceModel/PartialClass1.txt",
        "TestData/Processors/ReferenceModel/PartialClass2.txt")]
    public void Process_ShouldPartialClassHaveAllTheMembers_WhenGivenTwoFilesContainingPartialClasses(
        string fileContent1, string fileContent2)
    {
        var repositoryModel = LoadPartialClassesInRepositoryModel(fileContent1, fileContent2);

        var referenceSolutionModel = _sut.Process(repositoryModel);

        var partialClassModel1 = referenceSolutionModel.Projects[0].Files[0].Entities[0] as ClassModel;
        var partialClassModel2 = referenceSolutionModel.Projects[0].Files[1].Entities[0] as ClassModel;

        Assert.NotNull(partialClassModel1);
        Assert.NotNull(partialClassModel2);

        Assert.Equal(2, partialClassModel1!.Methods.Count);
        Assert.NotNull(partialClassModel1.Destructor);
        Assert.Empty(partialClassModel1.Constructors);
        Assert.Single(partialClassModel1.Attributes);
        Assert.Equal(1, partialClassModel1.Fields.Count);
        Assert.Equal(2, partialClassModel1.Properties.Count);
        Assert.Single(partialClassModel1.BaseTypes);
        Assert.Equal(3, partialClassModel1.Imports.Count);

        Assert.Equal(3, partialClassModel2!.Methods.Count);
        Assert.Null(partialClassModel2.Destructor);
        Assert.Single(partialClassModel2.Constructors);
        Assert.Empty(partialClassModel2.Attributes);
        Assert.Equal(3, partialClassModel2.Fields.Count);
        Assert.Equal(2, partialClassModel2.Properties.Count);
        Assert.Single(partialClassModel1.BaseTypes);
        Assert.Equal(2, partialClassModel2.Imports.Count);
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
