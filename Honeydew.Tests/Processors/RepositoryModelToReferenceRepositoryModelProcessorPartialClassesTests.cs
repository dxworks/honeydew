using System.Collections.Generic;
using Honeydew.Extractors.CSharp;
using Honeydew.Extractors.CSharp.Visitors.Concrete;
using Honeydew.Extractors.CSharp.Visitors.Setters;
using Honeydew.Extractors.Visitors;
using Honeydew.Logging;
using Honeydew.Models.CSharp;
using Honeydew.Models.Types;
using Honeydew.ScriptBeePlugin.Loaders;
using Honeydew.ScriptBeePlugin.Models;
using Moq;
using Xunit;
using ClassModel = Honeydew.ScriptBeePlugin.Models.ClassModel;
using ProjectModel = Honeydew.Models.ProjectModel;
using RepositoryModel = Honeydew.Models.RepositoryModel;

namespace Honeydew.Tests.Processors;

public class RepositoryModelToReferenceRepositoryModelProcessorPartialClassesTests
{
    private readonly RepositoryModelToReferenceRepositoryModelProcessor _sut;

    private readonly CSharpFactExtractor _extractor;
    private readonly Mock<ILogger> _loggerMock = new();
    private readonly Mock<IProgressLogger> _progressLoggerMock = new();
    private readonly CSharpSyntacticModelCreator _syntacticModelCreator = new();
    private readonly CSharpSemanticModelCreator _semanticModelCreator = new(new CSharpCompilationMaker());

    public RepositoryModelToReferenceRepositoryModelProcessorPartialClassesTests()
    {
        _sut = new RepositoryModelToReferenceRepositoryModelProcessor(_loggerMock.Object, _progressLoggerMock.Object);

        var returnValueSetterVisitor = new CSharpReturnValueSetterVisitor(_loggerMock.Object,
            new List<ITypeVisitor<IReturnValueType>>
            {
                new ReturnValueInfoVisitor()
            });

        var compositeVisitor = new CSharpCompilationUnitCompositeVisitor(_loggerMock.Object,
            new List<ITypeVisitor<ICompilationUnitType>>
            {
                new CSharpClassSetterCompilationUnitVisitor(_loggerMock.Object,
                    new List<ITypeVisitor<IMembersClassType>>
                    {
                        new BaseInfoClassVisitor(),
                        new BaseTypesClassVisitor(),
                        new ImportsVisitor(),
                        new CSharpAttributeSetterVisitor(_loggerMock.Object, new List<ITypeVisitor<IAttributeType>>
                        {
                            new AttributeInfoVisitor()
                        }),
                        new CSharpMethodSetterClassVisitor(_loggerMock.Object, new List<ITypeVisitor<IMethodType>>
                        {
                            new MethodInfoVisitor(),
                            returnValueSetterVisitor,
                        }),
                        new CSharpConstructorSetterClassVisitor(_loggerMock.Object,
                            new List<ITypeVisitor<IConstructorType>>
                            {
                                new ConstructorInfoVisitor(),
                            }),
                        new CSharpFieldSetterClassVisitor(_loggerMock.Object, new List<ITypeVisitor<IFieldType>>
                        {
                            new FieldInfoVisitor()
                        }),
                        new CSharpPropertySetterClassVisitor(_loggerMock.Object, new List<ITypeVisitor<IPropertyType>>
                        {
                            new PropertyInfoVisitor(),
                            new CSharpAccessorMethodSetterPropertyVisitor(_loggerMock.Object,
                                new List<ITypeVisitor<IAccessorMethodType>>
                                {
                                    new MethodInfoVisitor(),
                                    returnValueSetterVisitor,
                                })
                        }),
                        new CSharpDestructorSetterClassVisitor(_loggerMock.Object,
                            new List<ITypeVisitor<IDestructorType>>
                            {
                                new DestructorInfoVisitor(),
                            })
                    })
            });


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
                        new CSharpCompilationUnitModel
                        {
                            ClassTypes = classTypes1
                        },
                        new CSharpCompilationUnitModel
                        {
                            ClassTypes = classTypes2
                        }
                    }
                }
            }
        };
    }
}
