using System.Collections.Generic;
using System.Linq;
using Honeydew.Extractors.CSharp;
using Honeydew.Extractors.CSharp.Visitors.Concrete;
using Honeydew.Extractors.CSharp.Visitors.Setters;
using Honeydew.Extractors.Dotnet;
using Honeydew.Extractors.Visitors;
using Honeydew.Logging;
using Honeydew.Models.CSharp;
using Honeydew.Models.Types;
using Honeydew.ScriptBeePlugin.Loaders;
using Honeydew.ScriptBeePlugin.Models;
using Moq;
using Xunit;
using ClassModel = Honeydew.ScriptBeePlugin.Models.ClassModel;
using MethodModel = Honeydew.ScriptBeePlugin.Models.MethodModel;
using ProjectModel = Honeydew.Models.ProjectModel;
using RepositoryModel = Honeydew.Models.RepositoryModel;

namespace Honeydew.Tests.Processors;

public class RepositoryModelToReferenceRepositoryModelProcessorTestsWithMissingReferencesHandler
{
    private readonly RepositoryModelToReferenceRepositoryModelProcessor _sut;

    private readonly CSharpFactExtractor _extractor;
    private readonly Mock<ILogger> _loggerMock = new();
    private readonly Mock<IProgressLogger> _progressLoggerMock = new();
    private readonly CSharpSyntacticModelCreator _syntacticModelCreator = new();
    private readonly DotnetSemanticModelCreator _semanticModelCreator = new(new CSharpCompilationMaker());

    public RepositoryModelToReferenceRepositoryModelProcessorTestsWithMissingReferencesHandler()
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
        var localFunctionsSetterClassVisitor = new CSharpLocalFunctionsSetterClassVisitor(_loggerMock.Object,
            new List<ITypeVisitor<IMethodTypeWithLocalFunctions>>
            {
                new LocalFunctionInfoVisitor(_loggerMock.Object, new List<ITypeVisitor<IMethodTypeWithLocalFunctions>>
                {
                    calledMethodSetterVisitor,
                    returnValueSetterVisitor
                }),
                calledMethodSetterVisitor,
                returnValueSetterVisitor
            });

        var compositeVisitor = new CSharpCompilationUnitCompositeVisitor(_loggerMock.Object,
            new List<ITypeVisitor<ICompilationUnitType>>
            {
                new CSharpClassSetterVisitor(_loggerMock.Object,
                    new List<ITypeVisitor<IMembersClassType>>
                    {
                        new BaseInfoClassVisitor(),
                        new BaseTypesClassVisitor(),
                        new CSharpMethodSetterClassVisitor(_loggerMock.Object, new List<ITypeVisitor<IMethodType>>
                        {
                            new MethodInfoVisitor(),
                            calledMethodSetterVisitor,
                            parameterSetterVisitor,
                            returnValueSetterVisitor,
                            localFunctionsSetterClassVisitor
                        }),
                        new CSharpConstructorSetterClassVisitor(_loggerMock.Object,
                            new List<ITypeVisitor<IConstructorType>>
                            {
                                new ConstructorInfoVisitor(),
                                calledMethodSetterVisitor,
                                parameterSetterVisitor,
                                localFunctionsSetterClassVisitor
                            }),
                        new CSharpDestructorSetterVisitor(_loggerMock.Object,
                            new List<ITypeVisitor<IDestructorType>>
                            {
                                new DestructorInfoVisitor(),
                                calledMethodSetterVisitor,
                                localFunctionsSetterClassVisitor,
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
                                    calledMethodSetterVisitor,
                                    returnValueSetterVisitor,
                                    localFunctionsSetterClassVisitor
                                })
                        })
                    })
            });

        _extractor = new CSharpFactExtractor(compositeVisitor);
    }

    [Theory]
    [FileData("TestData/Processors/ReferenceOfClassWithMethodWithPrimitiveTypes.txt")]
    public void
        GetFunction_ShouldReturnReferenceSolutionModelWithAllMethodReferences_WhenGivenASolutionModelWithClassesWithMethodReferencesOnlyWithPrimitiveTypesAsParameters_UsingCSharpFactExtractor(
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
                            FilePath = "Project1.Services",
                            ClassTypes = classTypes
                        }
                    }
                }
            }
        };


        var referenceSolutionModel = _sut.Process(repositoryModel);

        Assert.Equal(1, referenceSolutionModel.Projects.Count);
        var allCreatedReferences = referenceSolutionModel.CreatedClasses;
        Assert.Equal(5, allCreatedReferences.Count);

        var objectClassModel =
            allCreatedReferences.SingleOrDefault(a => a.Name == "object");
        var intClassModel =
            allCreatedReferences.SingleOrDefault(a => a.Name == "int");
        var stringClassModel =
            allCreatedReferences.SingleOrDefault(a => a.Name == "string");
        var floatClassModel =
            allCreatedReferences.SingleOrDefault(a => a.Name == "float");
        var voidClassModel =
            allCreatedReferences.SingleOrDefault(a => a.Name == "void");

        Assert.NotNull(objectClassModel);
        Assert.NotNull(intClassModel);
        Assert.NotNull(stringClassModel);
        Assert.NotNull(floatClassModel);
        Assert.NotNull(voidClassModel);

        Assert.Equal("object", objectClassModel!.Name);
        Assert.Equal("int", intClassModel!.Name);
        Assert.Equal("string", stringClassModel!.Name);
        Assert.Equal("float", floatClassModel!.Name);
        Assert.Equal("void", voidClassModel!.Name);


        Assert.Equal(2, intClassModel.Methods.Count);

        var toStringMethodModel = intClassModel.Methods.First(model => model.Name == "ToString");
        var parseMethodModel = intClassModel.Methods.First(model => model.Name == "Parse");

        Assert.Equal("ToString", toStringMethodModel.Name);
        Assert.Equal(intClassModel, toStringMethodModel.Entity);
        Assert.Empty(toStringMethodModel.Parameters);

        Assert.Equal("Parse", parseMethodModel.Name);
        Assert.Equal(intClassModel, parseMethodModel.Entity);
        Assert.Equal(1, parseMethodModel.Parameters.Count);
        Assert.Equal(stringClassModel, parseMethodModel.Parameters[0].Type.Entity);
        Assert.Equal(ParameterModifier.None, parseMethodModel.Parameters[0].Modifier);
        Assert.Null(parseMethodModel.Parameters[0].DefaultValue);

        var compilationUnitModelServices = referenceSolutionModel.Projects[0].Files[0];
        var referenceMyClass = compilationUnitModelServices.Entities[0] as ClassModel;

        Assert.Equal(1, compilationUnitModelServices.Entities.Count);

        Assert.Equal("Project1.Services.MyClass", referenceMyClass!.Name);
        Assert.Equal(compilationUnitModelServices, referenceMyClass.File);
        Assert.Empty(referenceMyClass.Fields);
        Assert.Equal(5, referenceMyClass.Methods.Count);

        var methodFunction1 = referenceMyClass.Methods[0];
        var methodFunction2 = referenceMyClass.Methods[1];
        var methodFunction3 = referenceMyClass.Methods[2];
        var methodPrint1 = referenceMyClass.Methods[3];
        var methodPrint2 = referenceMyClass.Methods[4];

        Assert.Equal("Function1", methodFunction1.Name);
        Assert.Equal(referenceMyClass, methodFunction1.Entity);
        Assert.Equal("", methodFunction1.Modifier);
        Assert.Equal(AccessModifier.Public, methodFunction1.AccessModifier);
        Assert.Equal(floatClassModel, methodFunction1.ReturnValue!.Type.Entity);
        Assert.Equal(2, methodFunction1.Parameters.Count);
        Assert.Equal(intClassModel, methodFunction1.Parameters[0].Type.Entity);
        Assert.Equal(ParameterModifier.None, methodFunction1.Parameters[0].Modifier);
        Assert.Null(methodFunction1.Parameters[0].DefaultValue);
        Assert.Equal(intClassModel, methodFunction1.Parameters[1].Type.Entity);
        Assert.Equal(ParameterModifier.None, methodFunction1.Parameters[1].Modifier);
        Assert.Null(methodFunction1.Parameters[1].DefaultValue);
        Assert.Equal(5, methodFunction1.OutgoingCalls.Count);
        Assert.Equal(methodFunction3, methodFunction1.OutgoingCalls[0].Called);
        Assert.Equal(methodFunction3, methodFunction1.OutgoingCalls[1].Called);
        Assert.Equal(methodFunction2, methodFunction1.OutgoingCalls[2].Called);
        Assert.Equal(methodFunction2, methodFunction1.OutgoingCalls[3].Called);
        Assert.Equal(methodPrint2, methodFunction1.OutgoingCalls[4].Called);
        foreach (var methodCall in methodFunction1.OutgoingCalls)
        {
            Assert.Equal(methodFunction1, methodCall.Caller);
        }

        Assert.Equal("Function2", methodFunction2.Name);
        Assert.Equal(referenceMyClass, methodFunction2.Entity);
        Assert.Equal("", methodFunction2.Modifier);
        Assert.Equal(AccessModifier.Public, methodFunction2.AccessModifier);
        Assert.Equal(intClassModel, methodFunction2.ReturnValue!.Type.Entity);
        Assert.Equal(1, methodFunction2.Parameters.Count);
        Assert.Equal(stringClassModel, methodFunction2.Parameters[0].Type.Entity);
        Assert.Equal(ParameterModifier.None, methodFunction2.Parameters[0].Modifier);
        Assert.Null(methodFunction2.Parameters[0].DefaultValue);
        Assert.Equal(1, methodFunction2.OutgoingCalls.Count);
        Assert.Equal(parseMethodModel, methodFunction2.OutgoingCalls[0].Called);
        Assert.Equal(methodFunction2, methodFunction2.OutgoingCalls[0].Caller);

        Assert.Equal("Function3", methodFunction3.Name);
        Assert.Equal(referenceMyClass, methodFunction3.Entity);
        Assert.Equal("", methodFunction3.Modifier);
        Assert.Equal(AccessModifier.Public, methodFunction3.AccessModifier);
        Assert.Equal(stringClassModel, methodFunction3.ReturnValue!.Type.Entity);
        Assert.Equal(1, methodFunction3.Parameters.Count);
        Assert.Equal(intClassModel, methodFunction3.Parameters[0].Type.Entity);
        Assert.Equal(ParameterModifier.None, methodFunction3.Parameters[0].Modifier);
        Assert.Null(methodFunction3.Parameters[0].DefaultValue);
        Assert.Equal(1, methodFunction3.OutgoingCalls.Count);
        Assert.Equal(toStringMethodModel, methodFunction3.OutgoingCalls[0].Called);
        Assert.Equal(methodFunction3, methodFunction3.OutgoingCalls[0].Caller);

        Assert.Equal("Print", methodPrint1.Name);
        Assert.Equal(referenceMyClass, methodPrint1.Entity);
        Assert.Equal("static", methodPrint1.Modifier);
        Assert.Equal(AccessModifier.Private, methodPrint1.AccessModifier);
        Assert.Equal(voidClassModel, methodPrint1.ReturnValue!.Type.Entity);
        Assert.Equal(1, methodPrint1.Parameters.Count);
        Assert.Equal(floatClassModel, methodPrint1.Parameters[0].Type.Entity);
        Assert.Equal(ParameterModifier.None, methodPrint1.Parameters[0].Modifier);
        Assert.Null(methodPrint1.Parameters[0].DefaultValue);
        Assert.Empty(methodPrint1.OutgoingCalls);

        Assert.Equal("Print", methodPrint2.Name);
        Assert.Equal(referenceMyClass, methodPrint2.Entity);
        Assert.Equal("", methodPrint2.Modifier);
        Assert.Equal(AccessModifier.Private, methodPrint2.AccessModifier);
        Assert.Equal(voidClassModel, methodPrint2.ReturnValue!.Type.Entity);
        Assert.Equal(1, methodPrint2.Parameters.Count);
        Assert.Equal(intClassModel, methodPrint2.Parameters[0].Type.Entity);
        Assert.Equal(ParameterModifier.None, methodPrint2.Parameters[0].Modifier);
        Assert.Null(methodPrint2.Parameters[0].DefaultValue);
        Assert.Equal(1, methodPrint2.OutgoingCalls.Count);
        Assert.Equal(methodPrint2, methodPrint2.OutgoingCalls[0].Called);
        Assert.Equal(methodPrint2, methodPrint2.OutgoingCalls[0].Caller);
    }

    [Theory]
    [FileData("TestData/Processors/ReferenceOfClassWithMethodWithNumericValuesAsParameters.txt")]
    public void
        GetFunction_ShouldReturnReferenceSolutionModelWithAllMethodReferences_WhenGivenASolutionModelWithClassesWithMethodReferencesOnlyWithNumericValesAsParameters_UsingCSharpFactExtractor(
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
                            FilePath = "Project1.Services",
                            ClassTypes = classTypes,
                        }
                    }
                }
            }
        };

        var referenceSolutionModel = _sut.Process(repositoryModel);

        Assert.Equal(1, referenceSolutionModel.Projects.Count);
        var allCreatedReferences = referenceSolutionModel.CreatedClasses;
        Assert.Equal(6, allCreatedReferences.Count);

        var objectClassModel =
            allCreatedReferences.SingleOrDefault(a => a.Name == "object");
        var intClassModel =
            allCreatedReferences.SingleOrDefault(a => a.Name == "int");
        var shortClassModel =
            allCreatedReferences.SingleOrDefault(a => a.Name == "short");
        var longClassModel =
            allCreatedReferences.SingleOrDefault(a => a.Name == "long");
        var byteClassModel =
            allCreatedReferences.SingleOrDefault(a => a.Name == "byte");
        var voidClassModel =
            allCreatedReferences.SingleOrDefault(a => a.Name == "void");

        Assert.NotNull(objectClassModel);
        Assert.NotNull(intClassModel);
        Assert.NotNull(shortClassModel);
        Assert.NotNull(longClassModel);
        Assert.NotNull(byteClassModel);
        Assert.NotNull(voidClassModel);

        Assert.Equal("object", objectClassModel!.Name);
        Assert.Equal("int", intClassModel!.Name);
        Assert.Equal("short", shortClassModel!.Name);
        Assert.Equal("long", longClassModel!.Name);
        Assert.Equal("byte", byteClassModel!.Name);
        Assert.Equal("void", voidClassModel!.Name);

        var referenceCompilationUnitServices = referenceSolutionModel.Projects[0].Files[0];
        var referenceMyClass = referenceCompilationUnitServices.Entities[0] as ClassModel;

        Assert.Equal(1, referenceCompilationUnitServices.Entities.Count);

        Assert.Equal("Project1.Services.MyClass", referenceMyClass!.Name);
        Assert.Equal(referenceCompilationUnitServices, referenceMyClass.File);
        Assert.Empty(referenceMyClass.Fields);
        Assert.Equal(5, referenceMyClass.Methods.Count);

        var printNoArg = referenceMyClass.Methods[0];
        var printInt = referenceMyClass.Methods[1];
        var printShort = referenceMyClass.Methods[2];
        var printLong = referenceMyClass.Methods[3];
        var printByte = referenceMyClass.Methods[4];

        Assert.Equal("Print", printNoArg.Name);
        Assert.Equal(referenceMyClass, printNoArg.Entity);
        Assert.Equal("", printNoArg.Modifier);
        Assert.Equal(AccessModifier.Public, printNoArg.AccessModifier);
        Assert.Equal(voidClassModel, printNoArg.ReturnValue!.Type.Entity);
        Assert.Empty(printNoArg.Parameters);
        Assert.Equal(4, printNoArg.OutgoingCalls.Count);
        Assert.Equal(printInt, printNoArg.OutgoingCalls[0].Called);
        Assert.Equal(printLong, printNoArg.OutgoingCalls[1].Called);
        Assert.Equal(printShort, printNoArg.OutgoingCalls[2].Called);
        Assert.Equal(printByte, printNoArg.OutgoingCalls[3].Called);
        foreach (var methodCall in printNoArg.OutgoingCalls)
        {
            Assert.Equal(printNoArg, methodCall.Caller);
        }

        Assert.Equal("Print", printInt.Name);
        Assert.Equal(referenceMyClass, printInt.Entity);
        Assert.Equal("", printInt.Modifier);
        Assert.Equal(AccessModifier.Private, printInt.AccessModifier);
        Assert.Equal(voidClassModel, printInt.ReturnValue!.Type.Entity);
        Assert.Equal(1, printInt.Parameters.Count);
        Assert.Equal(intClassModel, printInt.Parameters[0].Type.Entity);
        Assert.Equal(ParameterModifier.None, printInt.Parameters[0].Modifier);
        Assert.Null(printInt.Parameters[0].DefaultValue);
        Assert.Empty(printInt.OutgoingCalls);

        Assert.Equal("Print", printShort.Name);
        Assert.Equal(referenceMyClass, printShort.Entity);
        Assert.Equal("", printShort.Modifier);
        Assert.Equal(AccessModifier.Private, printShort.AccessModifier);
        Assert.Equal(voidClassModel, printShort.ReturnValue!.Type.Entity);
        Assert.Equal(1, printShort.Parameters.Count);
        Assert.Equal(shortClassModel, printShort.Parameters[0].Type.Entity);
        Assert.Equal(ParameterModifier.None, printShort.Parameters[0].Modifier);
        Assert.Null(printShort.Parameters[0].DefaultValue);
        Assert.Empty(printShort.OutgoingCalls);

        Assert.Equal("Print", printLong.Name);
        Assert.Equal(referenceMyClass, printLong.Entity);
        Assert.Equal("", printLong.Modifier);
        Assert.Equal(AccessModifier.Private, printLong.AccessModifier);
        Assert.Equal(voidClassModel, printLong.ReturnValue!.Type.Entity);
        Assert.Equal(1, printLong.Parameters.Count);
        Assert.Equal(longClassModel, printLong.Parameters[0].Type.Entity);
        Assert.Equal(ParameterModifier.None, printLong.Parameters[0].Modifier);
        Assert.Null(printLong.Parameters[0].DefaultValue);
        Assert.Empty(printLong.OutgoingCalls);

        Assert.Equal("Print", printByte.Name);
        Assert.Equal(referenceMyClass, printByte.Entity);
        Assert.Equal("", printByte.Modifier);
        Assert.Equal(AccessModifier.Private, printByte.AccessModifier);
        Assert.Equal(voidClassModel, printByte.ReturnValue!.Type.Entity);
        Assert.Equal(1, printByte.Parameters.Count);
        Assert.Equal(byteClassModel, printByte.Parameters[0].Type.Entity);
        Assert.Equal(ParameterModifier.None, printByte.Parameters[0].Modifier);
        Assert.Null(printByte.Parameters[0].DefaultValue);
        Assert.Empty(printByte.OutgoingCalls);
    }

    [Theory]
    [FileData("TestData/Processors/ReferenceOfNamespaceWithMultipleClasses.txt")]
    public void
        GetFunction_ShouldReturnReferenceSolutionModelWithAllClassReferences_WhenGivenASolutionModelWithClassHierarchy_UsingCSharpFactExtractor(
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
                            FilePath = "Project1.Services",
                            ClassTypes = classTypes,
                        }
                    }
                }
            }
        };

        var referenceSolutionModel = _sut.Process(repositoryModel);

        Assert.Equal(1, referenceSolutionModel.Projects.Count);
        var allCreatedReferences = referenceSolutionModel.CreatedClasses;
        Assert.Equal(1, allCreatedReferences.Count);

        var objectClassModel =
            allCreatedReferences.SingleOrDefault(a => a.Name == "object");

        Assert.NotNull(objectClassModel);
        Assert.Equal("object", objectClassModel!.Name);

        var referenceNamespaceServices = referenceSolutionModel.Projects[0].Files[0];

        var referenceIInterface = referenceNamespaceServices.Entities[0] as InterfaceModel;
        var referenceMyInterface = referenceNamespaceServices.Entities[1] as InterfaceModel;
        var referenceOtherInterface = referenceNamespaceServices.Entities[2] as InterfaceModel;
        var referenceBaseClass = referenceNamespaceServices.Entities[3] as ClassModel;
        var referenceChildClass1 = referenceNamespaceServices.Entities[4] as ClassModel;
        var referenceChildClass2 = referenceNamespaceServices.Entities[5] as ClassModel;
        var referenceModel = referenceNamespaceServices.Entities[6] as ClassModel;
        var referenceChildClass3 = referenceNamespaceServices.Entities[7] as ClassModel;

        Assert.Equal(8, referenceNamespaceServices.Entities.Count);

        Assert.Equal("Project1.MyNamespace.IInterface", referenceIInterface!.Name);
        Assert.Equal(referenceNamespaceServices, referenceIInterface.File);
        Assert.Empty(referenceIInterface.BaseTypes);

        Assert.Equal("Project1.MyNamespace.MyInterface", referenceMyInterface!.Name);
        Assert.Equal(referenceNamespaceServices, referenceMyInterface.File);
        Assert.Equal(AccessModifier.Public, referenceMyInterface.AccessModifier);
        Assert.Equal("", referenceMyInterface.Modifier);
        Assert.Equal(1, referenceMyInterface.BaseTypes.Count);
        Assert.Equal(referenceIInterface, referenceMyInterface.BaseTypes[0].Entity);

        Assert.Equal("Project1.MyNamespace.OtherInterface", referenceOtherInterface!.Name);
        Assert.Equal(referenceNamespaceServices, referenceOtherInterface.File);
        Assert.Equal(AccessModifier.Public, referenceOtherInterface.AccessModifier);
        Assert.Equal("", referenceOtherInterface.Modifier);
        Assert.Empty(referenceOtherInterface.BaseTypes);

        Assert.Equal("Project1.MyNamespace.BaseClass", referenceBaseClass!.Name);
        Assert.Equal(referenceNamespaceServices, referenceBaseClass.File);
        Assert.Equal(ClassType.Class, referenceBaseClass.Type);
        Assert.Equal(AccessModifier.Public, referenceBaseClass.AccessModifier);
        Assert.Equal("", referenceBaseClass.Modifier);
        Assert.Equal(1, referenceBaseClass.BaseTypes.Count);
        Assert.Equal(objectClassModel, referenceBaseClass.BaseTypes[0].Entity);

        Assert.Equal("Project1.MyNamespace.ChildClass1", referenceChildClass1!.Name);
        Assert.Equal(referenceNamespaceServices, referenceChildClass1.File);
        Assert.Equal(ClassType.Class, referenceChildClass1.Type);
        Assert.Equal(AccessModifier.Public, referenceChildClass1.AccessModifier);
        Assert.Equal("", referenceChildClass1.Modifier);
        Assert.Equal(2, referenceChildClass1.BaseTypes.Count);
        Assert.Equal(referenceBaseClass, referenceChildClass1.BaseTypes[0].Entity);
        Assert.Equal(referenceIInterface, referenceChildClass1.BaseTypes[1].Entity);

        Assert.Equal("Project1.MyNamespace.ChildClass2", referenceChildClass2!.Name);
        Assert.Equal(referenceNamespaceServices, referenceChildClass2.File);
        Assert.Equal(ClassType.Class, referenceChildClass2.Type);
        Assert.Equal(AccessModifier.Public, referenceChildClass2.AccessModifier);
        Assert.Equal("", referenceChildClass2.Modifier);
        Assert.Equal(3, referenceChildClass2.BaseTypes.Count);
        Assert.Equal(referenceBaseClass, referenceChildClass2.BaseTypes[0].Entity);
        Assert.Equal(referenceMyInterface, referenceChildClass2.BaseTypes[1].Entity);
        Assert.Equal(referenceOtherInterface, referenceChildClass2.BaseTypes[2].Entity);

        Assert.Equal("Project1.MyNamespace.Model", referenceModel!.Name);
        Assert.Equal(referenceNamespaceServices, referenceModel.File);
        Assert.Equal(ClassType.Class, referenceModel.Type);
        Assert.Equal(AccessModifier.Public, referenceModel.AccessModifier);
        Assert.Equal("", referenceModel.Modifier);
        Assert.Equal(2, referenceModel.BaseTypes.Count);
        Assert.Equal(objectClassModel, referenceModel.BaseTypes[0].Entity);
        Assert.Equal(referenceOtherInterface, referenceModel.BaseTypes[1].Entity);

        Assert.Equal("Project1.MyNamespace.ChildClass3", referenceChildClass3!.Name);
        Assert.Equal(referenceNamespaceServices, referenceChildClass3.File);
        Assert.Equal(ClassType.Class, referenceChildClass3.Type);
        Assert.Equal(AccessModifier.Public, referenceChildClass3.AccessModifier);
        Assert.Equal("", referenceChildClass3.Modifier);
        Assert.Equal(1, referenceChildClass3.BaseTypes.Count);
        Assert.Equal(referenceChildClass1, referenceChildClass3.BaseTypes[0].Entity);
    }

    [Theory]
    [FileData("TestData/Processors/ReferenceWithClassHierarchy.txt")]
    public void
        GetFunction_ShouldReturnReferenceSolutionModelWithAllMethodReferences_WhenGivenASolutionModelWithClassesWithMethodReferencesWithClassHierarchyAsParameter_UsingCSharpFactExtractor(
            string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);

        var classTypes = _extractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var solutionModel = new RepositoryModel
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
                            FilePath = "Project1.MyNamespace",
                            ClassTypes = classTypes,
                        }
                    }
                }
            }
        };

        var referenceSolutionModel = _sut.Process(solutionModel);

        Assert.Equal(1, referenceSolutionModel.Projects.Count);
        var allCreatedReferences = referenceSolutionModel.CreatedClasses;
        Assert.Equal(4, allCreatedReferences.Count);

        var objectClassModel =
            allCreatedReferences.SingleOrDefault(a => a.Name == "object");
        var intClassModel =
            allCreatedReferences.SingleOrDefault(a => a.Name == "int");
        var floatClassModel =
            allCreatedReferences.SingleOrDefault(a => a.Name == "float");

        var voidClassModel =
            allCreatedReferences.SingleOrDefault(a => a.Name == "void");

        Assert.NotNull(objectClassModel);
        Assert.NotNull(intClassModel);
        Assert.NotNull(floatClassModel);
        Assert.NotNull(voidClassModel);

        Assert.Equal("object", objectClassModel!.Name);
        Assert.Equal("int", intClassModel!.Name);
        Assert.Equal("float", floatClassModel!.Name);
        Assert.Equal("void", voidClassModel!.Name);

        var referenceNamespaceServices = referenceSolutionModel.Projects[0].Files[0];
        var baseClass = referenceNamespaceServices.Entities[0] as ClassModel;
        var childClass1 = referenceNamespaceServices.Entities[1] as ClassModel;
        var childClass2 = referenceNamespaceServices.Entities[2] as ClassModel;
        var modelClass = referenceNamespaceServices.Entities[3] as ClassModel;
        var childClass3 = referenceNamespaceServices.Entities[4] as ClassModel;
        var callerClass = referenceNamespaceServices.Entities[5] as ClassModel;

        Assert.Equal(6, referenceNamespaceServices.Entities.Count);

        Assert.Equal("Project1.MyNamespace.BaseClass", baseClass!.Name);
        Assert.Equal(referenceNamespaceServices, baseClass.File);
        Assert.Equal(1, baseClass.Constructors.Count);
        Assert.Equal("BaseClass", baseClass.Constructors[0].Name);
        Assert.Equal("", baseClass.Constructors[0].Modifier);
        Assert.Equal(AccessModifier.Public, baseClass.Constructors[0].AccessModifier);
        Assert.Empty(baseClass.Constructors[0].OutgoingCalls);
        Assert.Empty(baseClass.Constructors[0].IncomingCalls);
        Assert.Equal(baseClass, baseClass.Constructors[0].Entity);
        Assert.Empty(baseClass.Constructors[0].Parameters);
        Assert.Empty(baseClass.Methods);
        Assert.Equal(1, baseClass.Constructors.Count);
        Assert.Equal(MethodType.Constructor, baseClass.Constructors[0].Type);
        Assert.Empty(baseClass.Metrics);
        Assert.Equal(1, baseClass.Fields.Count);

        var baseClassFieldX = baseClass.Fields[0];
        Assert.Equal("X", baseClassFieldX.Name);
        Assert.Equal(baseClass, baseClassFieldX.Entity);
        Assert.Equal(intClassModel, baseClassFieldX.Type.Entity);
        Assert.Equal("", baseClassFieldX.Modifier);
        Assert.Equal(AccessModifier.Public, baseClassFieldX.AccessModifier);
        Assert.False(baseClassFieldX.IsEvent);


        Assert.Equal("Project1.MyNamespace.ChildClass1", childClass1!.Name);
        Assert.Equal(referenceNamespaceServices, childClass1.File);
        Assert.Empty(childClass1.Methods);
        Assert.Empty(childClass1.Metrics);
        Assert.Empty(childClass1.Fields);


        Assert.Equal("Project1.MyNamespace.ChildClass2", childClass2!.Name);
        Assert.Equal(referenceNamespaceServices, childClass2.File);
        Assert.Empty(childClass2.Methods);
        Assert.Empty(childClass2.Metrics);
        Assert.Equal(1, childClass2.Fields.Count);

        var childClass2FieldZ = childClass2.Fields[0];
        Assert.Equal("Z", childClass2FieldZ.Name);
        Assert.Equal(childClass2, childClass2FieldZ.Entity);
        Assert.Equal(floatClassModel, childClass2FieldZ.Type.Entity);
        Assert.Equal("", childClass2FieldZ.Modifier);
        Assert.Equal(AccessModifier.Public, childClass2FieldZ.AccessModifier);
        Assert.False(childClass2FieldZ.IsEvent);


        Assert.Equal("Project1.MyNamespace.Model", modelClass!.Name);
        Assert.Equal(referenceNamespaceServices, modelClass.File);
        Assert.Empty(modelClass.Methods);
        Assert.Empty(modelClass.Metrics);
        Assert.Empty(modelClass.Fields);


        Assert.Equal("Project1.MyNamespace.ChildClass3", childClass3!.Name);
        Assert.Equal(referenceNamespaceServices, childClass3.File);
        Assert.Empty(childClass3.Methods);
        Assert.Empty(childClass3.Metrics);
        Assert.Equal(1, childClass3.Fields.Count);

        var childClass3ModelField = childClass3.Fields[0];
        Assert.Equal("_model", childClass3ModelField.Name);
        Assert.Equal(childClass3, childClass3ModelField.Entity);
        Assert.Equal(modelClass, childClass3ModelField.Type.Entity);
        Assert.Equal("readonly", childClass3ModelField.Modifier);
        Assert.Equal(AccessModifier.Private, childClass3ModelField.AccessModifier);
        Assert.False(childClass3ModelField.IsEvent);


        Assert.Equal("Project1.MyNamespace.Caller", callerClass!.Name);
        Assert.Equal(referenceNamespaceServices, callerClass.File);
        Assert.Empty(callerClass.Fields);
        Assert.Empty(callerClass.Metrics);
        Assert.Equal(2, callerClass.Methods.Count);

        var callMethod0 = callerClass.Methods[0];
        Assert.Equal("Call", callMethod0.Name);
        Assert.Equal(callerClass, callMethod0.Entity);
        Assert.Equal("", callMethod0.Modifier);
        Assert.Equal(AccessModifier.Public, callMethod0.AccessModifier);
        Assert.Equal(voidClassModel, callMethod0.ReturnValue!.Type.Entity);
        Assert.Equal(1, callMethod0.Parameters.Count);
        Assert.Equal(baseClass, callMethod0.Parameters[0].Type.Entity);
        Assert.Equal(ParameterModifier.None, callMethod0.Parameters[0].Modifier);
        Assert.Null(callMethod0.Parameters[0].DefaultValue);
        Assert.Empty(callMethod0.OutgoingCalls);

        var callMethod1 = callerClass.Methods[1];
        Assert.Equal("Call", callMethod1.Name);
        Assert.Equal(callerClass, callMethod1.Entity);
        Assert.Equal("static", callMethod1.Modifier);
        Assert.Equal(AccessModifier.Public, callMethod1.AccessModifier);
        Assert.Equal(voidClassModel, callMethod1.ReturnValue!.Type.Entity);
        Assert.Equal(MethodType.Method, callMethod1.Type);
        Assert.Empty(callMethod1.Parameters);
        Assert.Equal(6, callMethod1.OutgoingCalls.Count);

        foreach (var calledMethod in callMethod1.OutgoingCalls)
        {
            Assert.Equal(callMethod0, calledMethod.Called);
            Assert.Equal(callMethod1, calledMethod.Caller);
        }
    }

    [Theory]
    [FileData("TestData/Processors/ReferenceOfDeepNestedLocalFunctions.txt")]
    public void Process_ShouldHaveLocalFunctionReferences_WhenGivenMethodWithDeepNestedLocalFunctions(
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
                            FilePath = "Project1/class1.cs",
                            ClassTypes = classTypes,
                        }
                    }
                }
            }
        };

        var referenceSolutionModel = _sut.Process(repositoryModel);

        var classModel = referenceSolutionModel.Projects[0].Files[0].Entities[0] as ClassModel;
        var methodModel = classModel!.Methods[0];
        var methodModelLocalFunction = methodModel.LocalFunctions[0];

        Assert.Equal(classModel, methodModel.Entity);
        Assert.Equal(1, methodModel.OutgoingCalls.Count);
        Assert.Equal(methodModelLocalFunction, methodModel.OutgoingCalls[0].Called);
        Assert.Equal(methodModel, methodModel.OutgoingCalls[0].Caller);
        Assert.Equal(methodModel, methodModelLocalFunction.ContainingMethod);
        Assert.Equal(1, methodModel.LocalFunctions.Count);
        AssertLocalFunctions(methodModelLocalFunction);

        var constructorModel = classModel.Constructors[0];
        var constructorModelLocalFunction = constructorModel.LocalFunctions[0];
        Assert.Equal(classModel, constructorModel.Entity);
        Assert.Equal(1, constructorModel.OutgoingCalls.Count);
        Assert.Equal(1, constructorModel.LocalFunctions.Count);
        Assert.Equal(constructorModel, constructorModel.OutgoingCalls[0].Caller);
        Assert.Equal(constructorModelLocalFunction, constructorModel.OutgoingCalls[0].Called);
        Assert.Equal(constructorModel, constructorModelLocalFunction.ContainingMethod);
        Assert.Equal(1, constructorModel.LocalFunctions.Count);
        AssertLocalFunctions(constructorModelLocalFunction);

        var destructorModel = classModel.Destructor;
        var destructorModelLocalFunction = destructorModel!.LocalFunctions[0];
        Assert.Equal(classModel, destructorModel.Entity);
        Assert.Equal(1, destructorModel.OutgoingCalls.Count);
        Assert.Equal(destructorModelLocalFunction, destructorModel.OutgoingCalls[0].Called);
        Assert.Equal(destructorModel, destructorModel.OutgoingCalls[0].Caller);
        Assert.Equal(destructorModel, destructorModelLocalFunction.ContainingMethod);
        Assert.Equal(1, destructorModel.LocalFunctions.Count);
        AssertLocalFunctions(destructorModelLocalFunction);

        var propertyModel = classModel.Properties[0];
        var getAccessor = propertyModel.Accessors[0];
        var getAccessorModelLocalFunction = getAccessor.LocalFunctions[0];
        Assert.Equal(propertyModel, getAccessor.ContainingProperty);
        Assert.Equal(1, getAccessor.OutgoingCalls.Count);
        Assert.Equal(getAccessorModelLocalFunction, getAccessor.OutgoingCalls[0].Called);
        Assert.Equal(getAccessor, getAccessor.OutgoingCalls[0].Caller);
        Assert.Equal(getAccessor, getAccessorModelLocalFunction.ContainingMethod);
        Assert.Equal(1, getAccessor.LocalFunctions.Count);
        AssertLocalFunctions(getAccessorModelLocalFunction);

        var setAccessor = propertyModel.Accessors[1];
        var setAccessorModelLocalFunction = setAccessor.LocalFunctions[0];
        Assert.Equal(propertyModel, setAccessor.ContainingProperty);
        Assert.Equal(1, destructorModel.OutgoingCalls.Count);
        Assert.Equal(destructorModelLocalFunction, destructorModel.OutgoingCalls[0].Called);
        Assert.Equal(destructorModel, destructorModel.OutgoingCalls[0].Caller);
        Assert.Equal(setAccessor, setAccessorModelLocalFunction.ContainingMethod);
        Assert.Equal(1, destructorModel.LocalFunctions.Count);
        AssertLocalFunctions(setAccessorModelLocalFunction);

        void AssertLocalFunctions(MethodModel localFunction1)
        {
            Assert.Equal("LocalFunction1", localFunction1.Name);
            Assert.Equal(1, localFunction1.OutgoingCalls.Count);

            var localFunction2 = localFunction1.LocalFunctions[0];
            Assert.Equal(localFunction2, localFunction1.OutgoingCalls[0].Called);
            Assert.Equal(localFunction1, localFunction1.OutgoingCalls[0].Caller);
            Assert.Equal(1, localFunction1.LocalFunctions.Count);
            Assert.Equal("LocalFunction2", localFunction2.Name);
            Assert.Equal(localFunction1, localFunction2.ContainingMethod);
            Assert.Equal(1, localFunction2.OutgoingCalls.Count);

            var localFunction3 = localFunction2.LocalFunctions[0];
            Assert.Equal(localFunction3, localFunction2.OutgoingCalls[0].Called);
            Assert.Equal(localFunction2, localFunction2.OutgoingCalls[0].Caller);
            Assert.Equal(1, localFunction2.LocalFunctions.Count);
            Assert.Equal("LocalFunction3", localFunction3.Name);
            Assert.Equal(localFunction2, localFunction3.ContainingMethod);
            Assert.Empty(localFunction3.OutgoingCalls);
        }
    }
}
