using System.Collections.Generic;
using HoneydewCore.Logging;
using HoneydewCore.Processors;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewExtractors.Core.Metrics.Visitors.Constructors;
using HoneydewExtractors.Core.Metrics.Visitors.Destructors;
using HoneydewExtractors.Core.Metrics.Visitors.Fields;
using HoneydewExtractors.Core.Metrics.Visitors.Methods;
using HoneydewExtractors.Core.Metrics.Visitors.MethodSignatures;
using HoneydewExtractors.Core.Metrics.Visitors.Parameters;
using HoneydewExtractors.Core.Metrics.Visitors.Properties;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewExtractors.CSharp.Metrics.Extraction.Class;
using HoneydewExtractors.CSharp.Metrics.Extraction.Common;
using HoneydewExtractors.CSharp.Metrics.Extraction.CompilationUnit;
using HoneydewExtractors.CSharp.Metrics.Extraction.Constructor;
using HoneydewExtractors.CSharp.Metrics.Extraction.Destructor;
using HoneydewExtractors.CSharp.Metrics.Extraction.Field;
using HoneydewExtractors.CSharp.Metrics.Extraction.Method;
using HoneydewExtractors.CSharp.Metrics.Extraction.MethodCall;
using HoneydewExtractors.CSharp.Metrics.Extraction.Parameter;
using HoneydewExtractors.CSharp.Metrics.Extraction.Property;
using HoneydewExtractors.CSharp.Metrics.Visitors.Method;
using HoneydewExtractors.CSharp.Metrics.Visitors.Method.LocalFunctions;
using HoneydewModels.CSharp;
using HoneydewModels.Reference;
using Moq;
using Xunit;
using ProjectModel = HoneydewModels.CSharp.ProjectModel;
using RepositoryModel = HoneydewModels.CSharp.RepositoryModel;

namespace HoneydewCoreIntegrationTests.Processors
{
    public class RepositoryModelToReferenceRepositoryModelProcessorMethodTypesTests
    {
        private readonly RepositoryModelToReferenceRepositoryModelProcessor _sut;

        private readonly CSharpFactExtractor _extractor;
        private readonly Mock<ILogger> _loggerMock = new();
        private readonly CSharpSyntacticModelCreator _syntacticModelCreator = new();
        private readonly CSharpSemanticModelCreator _semanticModelCreator = new(new CSharpCompilationMaker());

        public RepositoryModelToReferenceRepositoryModelProcessorMethodTypesTests()
        {
            _sut = new RepositoryModelToReferenceRepositoryModelProcessor();

            var compositeVisitor = new CompositeVisitor();

            var calledMethodSetterVisitor = new CalledMethodSetterVisitor(new List<ICSharpMethodSignatureVisitor>
            {
                new MethodCallInfoVisitor()
            });
            var parameterSetterVisitor = new ParameterSetterVisitor(new List<IParameterVisitor>
            {
                new ParameterInfoVisitor()
            });
            var localFunctionsSetterClassVisitor = new LocalFunctionsSetterClassVisitor(new List<ILocalFunctionVisitor>
            {
                new LocalFunctionInfoVisitor(new List<ILocalFunctionVisitor>
                {
                    calledMethodSetterVisitor
                }),
                calledMethodSetterVisitor
            });
            compositeVisitor.Add(new ClassSetterCompilationUnitVisitor(new List<ICSharpClassVisitor>
            {
                new BaseInfoClassVisitor(),
                new BaseTypesClassVisitor(),
                new MethodSetterClassVisitor(new List<ICSharpMethodVisitor>
                {
                    new MethodInfoVisitor(),
                    calledMethodSetterVisitor,
                    parameterSetterVisitor,
                    localFunctionsSetterClassVisitor
                }),
                new ConstructorSetterClassVisitor(new List<ICSharpConstructorVisitor>
                {
                    new ConstructorInfoVisitor(),
                    calledMethodSetterVisitor,
                    parameterSetterVisitor,
                    localFunctionsSetterClassVisitor
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
                        calledMethodSetterVisitor,
                        localFunctionsSetterClassVisitor
                    })
                }),
                new DestructorSetterClassVisitor(new List<IDestructorVisitor>
                {
                    new DestructorInfoVisitor(),
                    calledMethodSetterVisitor,
                    localFunctionsSetterClassVisitor,
                })
            }));

            compositeVisitor.Accept(new LoggerSetterVisitor(_loggerMock.Object));

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
                            new CompilationUnitModel
                            {
                                FilePath = "Project1.Class1",
                                ClassTypes = classTypes
                            }
                        }
                    }
                }
            };


            var referenceSolutionModel = _sut.Process(repositoryModel);
            
            var classModel = referenceSolutionModel.Projects[0].Files[0].Classes[0];
            foreach (var modelMethod in classModel.Methods)
            {
                Assert.Equal(nameof(MethodType.Extension), modelMethod.MethodType);
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
                            new CompilationUnitModel
                            {
                                FilePath = "Project1.Class1",
                                ClassTypes = classTypes
                            }
                        }
                    }
                }
            };


            var referenceSolutionModel = _sut.Process(repositoryModel);


            var classModel = referenceSolutionModel.Projects[0].Files[0].Classes[0];

            Assert.Equal(nameof(MethodType.Destructor), classModel.Destructor.MethodType);
        }
    }
}
