using System.Collections.Generic;
using HoneydewCore.Logging;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewExtractors.Core.Metrics.Visitors.Constructors;
using HoneydewExtractors.Core.Metrics.Visitors.Fields;
using HoneydewExtractors.Core.Metrics.Visitors.LocalVariables;
using HoneydewExtractors.Core.Metrics.Visitors.Methods;
using HoneydewExtractors.Core.Metrics.Visitors.MethodSignatures;
using HoneydewExtractors.Core.Metrics.Visitors.Parameters;
using HoneydewExtractors.Core.Metrics.Visitors.Properties;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewExtractors.CSharp.Metrics.Extraction.Class;
using HoneydewExtractors.CSharp.Metrics.Extraction.Common;
using HoneydewExtractors.CSharp.Metrics.Extraction.CompilationUnit;
using HoneydewExtractors.CSharp.Metrics.Extraction.Constructor;
using HoneydewExtractors.CSharp.Metrics.Extraction.Field;
using HoneydewExtractors.CSharp.Metrics.Extraction.LocalVariables;
using HoneydewExtractors.CSharp.Metrics.Extraction.Method;
using HoneydewExtractors.CSharp.Metrics.Extraction.MethodCall;
using HoneydewExtractors.CSharp.Metrics.Extraction.Parameter;
using HoneydewExtractors.CSharp.Metrics.Extraction.Property;
using HoneydewExtractors.Processors;
using HoneydewModels.CSharp;
using Moq;
using Xunit;

namespace HoneydewExtractorsTests.Processors
{
    public class FullNameModelProcessorForGenericTypesTests
    {
        private readonly FullNameModelProcessor _sut;
        private readonly Mock<ILogger> _loggerMock = new();
        private readonly Mock<IProgressLogger> _progressLoggerMock = new();
        private readonly Mock<IProgressLoggerBar> _progressLoggerBarMock = new();
        private readonly CSharpFactExtractor _extractor;
        private readonly CSharpSyntacticModelCreator _syntacticModelCreator = new();
        private readonly CSharpSemanticModelCreator _semanticModelCreator = new(new CSharpCompilationMaker());

        public FullNameModelProcessorForGenericTypesTests()
        {
            _sut = new FullNameModelProcessor(_loggerMock.Object, _progressLoggerMock.Object, false);

            var compositeVisitor = new CompositeVisitor();
            var calledMethodSetterVisitor = new CalledMethodSetterVisitor(new List<IMethodSignatureVisitor>
            {
                new MethodCallInfoVisitor()
            });
            var parameterSetterVisitor = new ParameterSetterVisitor(new List<IParameterVisitor>
            {
                new ParameterInfoVisitor()
            });
            var localVariablesTypeSetterVisitor = new LocalVariablesTypeSetterVisitor(new List<ILocalVariablesVisitor>
            {
                new LocalVariableInfoVisitor()
            });
            compositeVisitor.Add(new ClassSetterCompilationUnitVisitor(new List<IClassVisitor>
            {
                new BaseInfoClassVisitor(),
                new BaseTypesClassVisitor(),
                new ImportsVisitor(),
                new MethodSetterClassVisitor(new List<IMethodVisitor>
                {
                    new MethodInfoVisitor(),
                    calledMethodSetterVisitor,
                    parameterSetterVisitor,
                    localVariablesTypeSetterVisitor,
                }),
                new ConstructorSetterClassVisitor(new List<IConstructorVisitor>
                {
                    new ConstructorInfoVisitor(),
                    calledMethodSetterVisitor,
                    parameterSetterVisitor,
                    localVariablesTypeSetterVisitor,
                }),
                new PropertySetterClassVisitor(new List<IPropertyVisitor>
                {
                    new PropertyInfoVisitor()
                }),
                new FieldSetterClassVisitor(new List<IFieldVisitor>
                {
                    new FieldInfoVisitor()
                })
            }));

            _extractor = new CSharpFactExtractor(compositeVisitor);
        }

        [Fact]
        public void Process_ShouldReturnTheFullClassNames_WhenGivenClassModelsWithGenericTypes()
        {
            const string fileContent1 = @"
namespace OtherNamespace
{
    public class Generic<T> { }
}";

            const string fileContent2 = @"
namespace OtherMyNamespace
{
    public class Generic { }
}";

            const string fileContent3 = @"
using OtherNamespace;
using OtherMyNamespace;
    
namespace NameSpace1
{
    public class MyClass
    {
        public void Method(Generic<Generic<int>> g)
        {
            Generic<Generic<string>> a = new();
            var f = new Generic<float>();
        }
    }
}";

            var syntaxTree1 = _syntacticModelCreator.Create(fileContent1);
            var semanticModel1 = _semanticModelCreator.Create(syntaxTree1);

            var syntaxTree2 = _syntacticModelCreator.Create(fileContent2);
            var semanticModel2 = _semanticModelCreator.Create(syntaxTree2);
            
            var syntaxTree3 = _syntacticModelCreator.Create(fileContent3);
            var semanticModel3 = _semanticModelCreator.Create(syntaxTree3);
            
            var classModels1 = _extractor.Extract(syntaxTree1, semanticModel1).ClassTypes;
            var classModels2 = _extractor.Extract(syntaxTree2, semanticModel2).ClassTypes;
            var classModels3 = _extractor.Extract(syntaxTree3, semanticModel3).ClassTypes;

            var repositoryModel = new RepositoryModel();
            var solutionModel = new SolutionModel();
            var projectModel = new ProjectModel();

            foreach (var classModel in classModels1)
            {
                projectModel.Add(classModel);
            }

            foreach (var classModel in classModels2)
            {
                projectModel.Add(classModel);
            }

            foreach (var classModel in classModels3)
            {
                projectModel.Add(classModel);
            }

            solutionModel.Projects.Add(projectModel);
            repositoryModel.Solutions.Add(solutionModel);

            _progressLoggerMock.Setup(logger => logger.CreateProgressLogger(3, "Resolving Class Names"))
                .Returns(_progressLoggerBarMock.Object);
            _progressLoggerMock.Setup(logger =>
                    logger.CreateProgressLogger(3, "Resolving Using Statements for Each Class"))
                .Returns(_progressLoggerBarMock.Object);
            _progressLoggerMock.Setup(logger =>
                    logger.CreateProgressLogger(3, "Resolving Class Elements (Fields, Methods, Properties,...)"))
                .Returns(_progressLoggerBarMock.Object);

            var actualRepositoryModel = _sut.Process(repositoryModel);

            var genericClass = actualRepositoryModel.Solutions[0].Projects[0].Namespaces[0].ClassModels[0];
            Assert.Equal("OtherNamespace.Generic<T>", genericClass.Name);

            var classType = (ClassModel)actualRepositoryModel.Solutions[0].Projects[0].Namespaces[2].ClassModels[0];
            Assert.Equal("OtherNamespace.Generic<OtherNamespace.Generic<int>>",
                classType.Methods[0].ParameterTypes[0].Type.Name);
            Assert.Equal("OtherNamespace.Generic<OtherNamespace.Generic<string>>",
                classType.Methods[0].LocalVariableTypes[0].Type.Name);
            Assert.Equal("OtherNamespace.Generic<float>", classType.Methods[0].LocalVariableTypes[1].Type.Name);
        }

        [Fact]
        public void Process_ShouldReturnTheFullClassNames_WhenGivenClassModelsWithGenericTypesWithMultipleParameters()
        {
            const string fileContent1 = @"
namespace OtherNamespace
{
    public class Generic<T> { }
}";

            const string fileContent2 = @"
namespace OtherMyNamespace
{
    public class Generic<K,R> { }
}";

            const string fileContent3 = @"
using OtherNamespace;
using OtherMyNamespace;
    
namespace NameSpace1
{
    public class MyClass
    {
        public void Method(Generic<Generic<int>,Generic<string,char>> g)
        {
            Generic<Generic<string>> a = new();
            var f = new Generic<float, double>();
        }
    }
}";

            var syntaxTree1 = _syntacticModelCreator.Create(fileContent1);
            var semanticModel1 = _semanticModelCreator.Create(syntaxTree1);

            var syntaxTree2 = _syntacticModelCreator.Create(fileContent2);
            var semanticModel2 = _semanticModelCreator.Create(syntaxTree2);
            
            var syntaxTree3 = _syntacticModelCreator.Create(fileContent3);
            var semanticModel3 = _semanticModelCreator.Create(syntaxTree3);
            
            var classModels1 = _extractor.Extract(syntaxTree1, semanticModel1).ClassTypes;
            var classModels2 = _extractor.Extract(syntaxTree2, semanticModel2).ClassTypes;
            var classModels3 = _extractor.Extract(syntaxTree3, semanticModel3).ClassTypes;
            
            var repositoryModel = new RepositoryModel();
            var solutionModel = new SolutionModel();
            var projectModel = new ProjectModel();

            foreach (var classModel in classModels1)
            {
                projectModel.Add(classModel);
            }

            foreach (var classModel in classModels2)
            {
                projectModel.Add(classModel);
            }

            foreach (var classModel in classModels3)
            {
                projectModel.Add(classModel);
            }

            solutionModel.Projects.Add(projectModel);
            repositoryModel.Solutions.Add(solutionModel);

            _progressLoggerMock.Setup(logger => logger.CreateProgressLogger(3, "Resolving Class Names"))
                .Returns(_progressLoggerBarMock.Object);
            _progressLoggerMock.Setup(logger =>
                    logger.CreateProgressLogger(3, "Resolving Using Statements for Each Class"))
                .Returns(_progressLoggerBarMock.Object);
            _progressLoggerMock.Setup(logger =>
                    logger.CreateProgressLogger(3, "Resolving Class Elements (Fields, Methods, Properties,...)"))
                .Returns(_progressLoggerBarMock.Object);

            var actualRepositoryModel = _sut.Process(repositoryModel);

            var genericClass = actualRepositoryModel.Solutions[0].Projects[0].Namespaces[0].ClassModels[0];
            Assert.Equal("OtherNamespace.Generic<T>", genericClass.Name);

            var classType = (ClassModel)actualRepositoryModel.Solutions[0].Projects[0].Namespaces[2].ClassModels[0];
            Assert.Equal("OtherMyNamespace.Generic<OtherNamespace.Generic<int>,OtherMyNamespace.Generic<string,char>>",
                classType.Methods[0].ParameterTypes[0].Type.Name);
            Assert.Equal("OtherNamespace.Generic<OtherNamespace.Generic<string>>",
                classType.Methods[0].LocalVariableTypes[0].Type.Name);
            Assert.Equal("OtherMyNamespace.Generic<float,double>",
                classType.Methods[0].LocalVariableTypes[1].Type.Name);
        }

        [Fact]
        public void Process_ShouldReturnTheFullClassNames_WhenGivenClassModelsWithGenericTypesWithSystemTypes()
        {
            const string fileContent1 = @"
namespace OtherNamespace
{
    public class Class1 { }
}";

            const string fileContent2 = @"
using OtherNamespace;
using System.Collections.Generic;
    
namespace NameSpace1
{
    public class MyClass
    {
        public void Method(List<Class1> l)
        {
            List<Class1> a = new();
            var f = new List<Class1>();
        }
    }
}";

            var syntaxTree1 = _syntacticModelCreator.Create(fileContent1);
            var semanticModel1 = _semanticModelCreator.Create(syntaxTree1);

            var syntaxTree2 = _syntacticModelCreator.Create(fileContent2);
            var semanticModel2 = _semanticModelCreator.Create(syntaxTree2);
            
            
            var classModels1 = _extractor.Extract(syntaxTree1, semanticModel1).ClassTypes;
            var classModels2 = _extractor.Extract(syntaxTree2, semanticModel2).ClassTypes;
            

            var repositoryModel = new RepositoryModel();
            var solutionModel = new SolutionModel();
            var projectModel = new ProjectModel();

            foreach (var classModel in classModels1)
            {
                projectModel.Add(classModel);
            }

            foreach (var classModel in classModels2)
            {
                projectModel.Add(classModel);
            }

            solutionModel.Projects.Add(projectModel);
            repositoryModel.Solutions.Add(solutionModel);

            _progressLoggerMock.Setup(logger => logger.CreateProgressLogger(2, "Resolving Class Names"))
                .Returns(_progressLoggerBarMock.Object);
            _progressLoggerMock.Setup(logger =>
                    logger.CreateProgressLogger(2, "Resolving Using Statements for Each Class"))
                .Returns(_progressLoggerBarMock.Object);
            _progressLoggerMock.Setup(logger =>
                    logger.CreateProgressLogger(2, "Resolving Class Elements (Fields, Methods, Properties,...)"))
                .Returns(_progressLoggerBarMock.Object);

            var actualRepositoryModel = _sut.Process(repositoryModel);

            var classType = (ClassModel)actualRepositoryModel.Solutions[0].Projects[0].Namespaces[1].ClassModels[0];
            Assert.Equal("System.Collections.Generic.List<OtherNamespace.Class1>",
                classType.Methods[0].ParameterTypes[0].Type.Name);
            Assert.Equal("System.Collections.Generic.List<OtherNamespace.Class1>",
                classType.Methods[0].LocalVariableTypes[0].Type.Name);
            Assert.Equal("System.Collections.Generic.List<OtherNamespace.Class1>",
                classType.Methods[0].LocalVariableTypes[1].Type.Name);
        }
    }
}
