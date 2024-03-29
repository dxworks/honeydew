﻿using System.Collections.Generic;
using Honeydew.Extractors.CSharp;
using Honeydew.Extractors.CSharp.Visitors.Concrete;
using Honeydew.Extractors.CSharp.Visitors.Setters;
using Honeydew.Extractors.Dotnet;
using Honeydew.Extractors.Visitors;
using Honeydew.Logging;
using Honeydew.Models.CSharp;
using Honeydew.Models.Types;
using DxWorks.ScriptBee.Plugins.Honeydew.Loaders;
using DxWorks.ScriptBee.Plugins.Honeydew.Models;
using Moq;
using Xunit;
using ClassModel = DxWorks.ScriptBee.Plugins.Honeydew.Models.ClassModel;
using ProjectModel = Honeydew.Models.ProjectModel;
using RepositoryModel = Honeydew.Models.RepositoryModel;

namespace Honeydew.Tests.Processors;

public class RepositoryModelToReferenceRepositoryModelProcessorMethodTypesTests
{
    private readonly RepositoryModelToReferenceRepositoryModelProcessor _sut;

    private readonly CSharpFactExtractor _extractor;
    private readonly Mock<ILogger> _loggerMock = new();
    private readonly Mock<IProgressLogger> _progressLoggerMock = new();
    private readonly CSharpSyntacticModelCreator _syntacticModelCreator = new();
    private readonly DotnetSemanticModelCreator _semanticModelCreator = new(new CSharpCompilationMaker());

    public RepositoryModelToReferenceRepositoryModelProcessorMethodTypesTests()
    {
        _sut = new RepositoryModelToReferenceRepositoryModelProcessor(_loggerMock.Object, _progressLoggerMock.Object);

        var calledMethodSetterVisitor = new CSharpCalledMethodSetterVisitor(_loggerMock.Object,
            new List<ITypeVisitor<IMethodCallType>>
            {
                new MethodCallInfoVisitor()
            });
        var parameterSetterVisitor = new CSharpParameterSetterVisitor(_loggerMock.Object,
            new List<ITypeVisitor<IParameterType>>
            {
                new ParameterInfoVisitor()
            });
        var returnValueSetterVisitor = new CSharpReturnValueSetterVisitor(_loggerMock.Object,
            new List<ITypeVisitor<IReturnValueType>>
            {
                new ReturnValueInfoVisitor()
            });
        var localFunctionsSetterClassVisitor = new CSharpLocalFunctionsSetterVisitor(_loggerMock.Object,
            new List<ITypeVisitor<IMethodTypeWithLocalFunctions>>
            {
                new LocalFunctionInfoVisitor(_loggerMock.Object, new List<ITypeVisitor<IMethodTypeWithLocalFunctions>>
                {
                    calledMethodSetterVisitor,
                    returnValueSetterVisitor,
                }),
                calledMethodSetterVisitor,
                returnValueSetterVisitor,
            });

        var compositeVisitor = new CSharpCompilationUnitCompositeVisitor(_loggerMock.Object,
            new List<ITypeVisitor<ICompilationUnitType>>
            {
                new CSharpClassSetterVisitor(_loggerMock.Object,
                    new List<ITypeVisitor<IMembersClassType>>
                    {
                        new BaseInfoClassVisitor(),
                        new BaseTypesClassVisitor(),
                        new CSharpMethodSetterVisitor(_loggerMock.Object, new List<ITypeVisitor<IMethodType>>
                        {
                            new MethodInfoVisitor(),
                            calledMethodSetterVisitor,
                            parameterSetterVisitor,
                            returnValueSetterVisitor,
                            localFunctionsSetterClassVisitor
                        }),
                        new CSharpConstructorSetterVisitor(_loggerMock.Object,
                            new List<ITypeVisitor<IConstructorType>>
                            {
                                new ConstructorInfoVisitor(),
                                calledMethodSetterVisitor,
                                parameterSetterVisitor,
                                localFunctionsSetterClassVisitor
                            }),
                        new CSharpFieldSetterClassVisitor(_loggerMock.Object, new List<ITypeVisitor<IFieldType>>
                        {
                            new FieldInfoVisitor()
                        }),
                        new CSharpPropertySetterClassVisitor(_loggerMock.Object, new List<ITypeVisitor<IPropertyType>>
                        {
                            new PropertyInfoVisitor(),
                            new CSharpAccessorMethodSetterVisitor(_loggerMock.Object, new List<ITypeVisitor<IAccessorMethodType>>
                            {
                                new MethodInfoVisitor(),
                                calledMethodSetterVisitor,
                                returnValueSetterVisitor,
                                localFunctionsSetterClassVisitor
                            })
                        }),
                        new CSharpDestructorSetterVisitor(_loggerMock.Object, new List<ITypeVisitor<IDestructorType>>
                        {
                            new DestructorInfoVisitor(),
                            calledMethodSetterVisitor,
                            localFunctionsSetterClassVisitor,
                        })
                    })
            });

        _extractor = new CSharpFactExtractor(compositeVisitor);
    }

    [Theory]
    [FileData("TestData/Processors/ReferenceOfExtensionMethods.txt")]
    public void
        GetFunction_ShouldReturnReferenceSolutionModelWithExtensionMethodType_WhenGivenClassWithExtensionMethod(
            string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);

        var classTypes = _extractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var repositoryModel = new RepositoryModel
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
                            FilePath = "Project1.Class1",
                            ClassTypes = classTypes
                        }
                    }
                }
            }
        };


        var referenceSolutionModel = _sut.Process(repositoryModel);

        var classModel = referenceSolutionModel.Projects[0].Files[0].Entities[0] as ClassModel;
        Assert.NotNull(classModel);
        foreach (var modelMethod in classModel!.Methods)
        {
            Assert.Equal(MethodType.Extension, modelMethod.Type);
        }
    }

    [Theory]
    [FileData("TestData/Processors/ReferenceOfDestructorMethod.txt")]
    public void
        GetFunction_ShouldReturnReferenceSolutionModelWithDestructorMethodType_WhenGivenClassWithDestructor(
            string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);

        var classTypes = _extractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var repositoryModel = new RepositoryModel
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
                            FilePath = "Project1.Class1",
                            ClassTypes = classTypes
                        }
                    }
                }
            }
        };


        var referenceSolutionModel = _sut.Process(repositoryModel);


        var classModel = referenceSolutionModel.Projects[0].Files[0].Entities[0] as ClassModel;

        Assert.NotNull(classModel);
        Assert.Equal(MethodType.Destructor, classModel!.Destructor!.Type);
    }
}
