using System.Collections.Generic;
using HoneydewCore.Logging;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Attributes;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewExtractors.Core.Metrics.Visitors.Constructors;
using HoneydewExtractors.Core.Metrics.Visitors.Fields;
using HoneydewExtractors.Core.Metrics.Visitors.Methods;
using HoneydewExtractors.Core.Metrics.Visitors.MethodSignatures;
using HoneydewExtractors.Core.Metrics.Visitors.Parameters;
using HoneydewExtractors.Core.Metrics.Visitors.Properties;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewExtractors.CSharp.Metrics.Extraction.Attribute;
using HoneydewExtractors.CSharp.Metrics.Extraction.Class;
using HoneydewExtractors.CSharp.Metrics.Extraction.Common;
using HoneydewExtractors.CSharp.Metrics.Extraction.CompilationUnit;
using HoneydewExtractors.CSharp.Metrics.Extraction.Constructor;
using HoneydewExtractors.CSharp.Metrics.Extraction.Field;
using HoneydewExtractors.CSharp.Metrics.Extraction.Method;
using HoneydewExtractors.CSharp.Metrics.Extraction.MethodCall;
using HoneydewExtractors.CSharp.Metrics.Extraction.Parameter;
using HoneydewExtractors.CSharp.Metrics.Extraction.Property;
using HoneydewExtractors.Processors;
using HoneydewModels.CSharp;
using HoneydewModels.Types;
using Moq;
using Xunit;

namespace HoneydewExtractorsTests.Processors
{
    public class FullNameModelProcessorWithExtractionTests
    {
        private readonly FullNameModelProcessor _sut;
        private readonly Mock<ILogger> _loggerMock = new();
        private readonly Mock<ILogger> _ambiguousClassLoggerMock = new();
        private readonly Mock<IProgressLogger> _progressLoggerMock = new();
        private readonly Mock<IProgressLoggerBar> _progressLoggerBarMock = new();
        private readonly CSharpFactExtractor _extractor;
        private readonly CSharpSyntacticModelCreator _syntacticModelCreator = new();
        private readonly CSharpSemanticModelCreator _semanticModelCreator = new(new CSharpCompilationMaker());

        public FullNameModelProcessorWithExtractionTests()
        {
            _sut = new FullNameModelProcessor(_loggerMock.Object, _ambiguousClassLoggerMock.Object,
                _progressLoggerMock.Object, false);

            var compositeVisitor = new CompositeVisitor();
            var calledMethodSetterVisitor = new CalledMethodSetterVisitor(new List<IMethodSignatureVisitor>
            {
                new MethodCallInfoVisitor()
            });
            var attributeSetterVisitor = new AttributeSetterVisitor(new List<IAttributeVisitor>
            {
                new AttributeInfoVisitor()
            });
            var parameterSetterVisitor = new ParameterSetterVisitor(new List<IParameterVisitor>
            {
                new ParameterInfoVisitor(),
                attributeSetterVisitor,
            });
            compositeVisitor.Add(new ClassSetterCompilationUnitVisitor(new List<IClassVisitor>
            {
                new BaseInfoClassVisitor(),
                new BaseTypesClassVisitor(),
                new ImportsVisitor(),
                attributeSetterVisitor,
                new MethodSetterClassVisitor(new List<IMethodVisitor>
                {
                    new MethodInfoVisitor(),
                    calledMethodSetterVisitor,
                    parameterSetterVisitor,
                    attributeSetterVisitor,
                }),
                new ConstructorSetterClassVisitor(new List<IConstructorVisitor>
                {
                    new ConstructorInfoVisitor(),
                    calledMethodSetterVisitor,
                    parameterSetterVisitor,
                    attributeSetterVisitor,
                }),
                new PropertySetterClassVisitor(new List<IPropertyVisitor>
                {
                    new PropertyInfoVisitor(),
                    attributeSetterVisitor,
                }),
                new FieldSetterClassVisitor(new List<IFieldVisitor>
                {
                    new FieldInfoVisitor(),
                    attributeSetterVisitor,
                })
            }));

            _extractor = new CSharpFactExtractor(compositeVisitor);
        }

        [Fact]
        public void Process_ShouldReturnFullName_WhenProvidedWithUsings()
        {
            const string fileContent1 = @"
namespace Models
{
    record Model
    {
    }
}
";
            const string fileContent2 = @"
using System;
namespace Services
{
    using Models;

    class Service
    {
        Model _m {get;set;}

        Model GetModel()
        {
            return new();
        }
    }
}
";

            var syntaxTree1 = _syntacticModelCreator.Create(fileContent1);
            var semanticModel1 = _semanticModelCreator.Create(syntaxTree1);

            var syntaxTree2 = _syntacticModelCreator.Create(fileContent2);
            var semanticModel2 = _semanticModelCreator.Create(syntaxTree2);


            var compilationUnit1 = _extractor.Extract(syntaxTree1, semanticModel1);
            var compilationUnit2 = _extractor.Extract(syntaxTree2, semanticModel2);

            var repositoryModel = new RepositoryModel();
            var solutionModel = new SolutionModel();
            var projectModel = new ProjectModel();

            projectModel.Add(compilationUnit1);
            projectModel.Add(compilationUnit2);

            repositoryModel.Projects.Add(projectModel);
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

            var model = (ClassModel)actualRepositoryModel.Projects[0].CompilationUnits[1].ClassTypes[0];
            Assert.Equal("Models.Model", model.Properties[0].Type.Name);
            Assert.Equal("Models.Model", model.Methods[0].ReturnValue.Type.Name);
        }

        [Fact]
        public void Process_ShouldReturnFullName_WhenProvidedWithStaticUsings()
        {
            const string fileContent1 = @"
using System;

namespace Utils
{
    using static Math;

    public class IntWrapper
    {
        public void Call()
        {
        }
    }

    public class StringWrapper
    {
        public void Call()
        {
        }
    }

    public static class Util
    {
        public static IntWrapper Int;

        public static StringWrapper Name { get; set; }

        public static double Radical(double value)
        {
            return Sqrt(value);
        }
    }
}";

            const string fileContent2 = @"
namespace MyNamespace
{
    using static Utils.Util;

    public class Client
    {
        public Client()
        {
            var v = Radical(2.0);

            Int.Call();
            Name.Call();
        }

        public void Calculate(double value)
        {
            var v = Radical(value);

            Int.Call();
            Name.Call();
        }
    }
}
";

            var syntaxTree1 = _syntacticModelCreator.Create(fileContent1);
            var semanticModel1 = _semanticModelCreator.Create(syntaxTree1);

            var syntaxTree2 = _syntacticModelCreator.Create(fileContent2);
            var semanticModel2 = _semanticModelCreator.Create(syntaxTree2);

            var compilationUnit1 = _extractor.Extract(syntaxTree1, semanticModel1);
            var compilationUnit2 = _extractor.Extract(syntaxTree2, semanticModel2);

            var repositoryModel = new RepositoryModel();
            var solutionModel = new SolutionModel();
            var projectModel = new ProjectModel();

            projectModel.Add(compilationUnit1);
            projectModel.Add(compilationUnit2);

            repositoryModel.Projects.Add(projectModel);
            repositoryModel.Solutions.Add(solutionModel);

            _progressLoggerMock.Setup(logger => logger.CreateProgressLogger(4, "Resolving Class Names"))
                .Returns(_progressLoggerBarMock.Object);
            _progressLoggerMock.Setup(logger =>
                    logger.CreateProgressLogger(4, "Resolving Using Statements for Each Class"))
                .Returns(_progressLoggerBarMock.Object);
            _progressLoggerMock.Setup(logger =>
                    logger.CreateProgressLogger(4, "Resolving Class Elements (Fields, Methods, Properties,...)"))
                .Returns(_progressLoggerBarMock.Object);


            var actualRepositoryModel = _sut.Process(repositoryModel);

            var utilClass =
                (ClassModel)actualRepositoryModel.Projects[0].CompilationUnits[0].ClassTypes[2];
            Assert.Equal("Sqrt", utilClass.Methods[0].CalledMethods[0].Name);
            Assert.Equal("System.Math", utilClass.Methods[0].CalledMethods[0].ContainingTypeName);
            Assert.Equal(1, utilClass.Methods[0].CalledMethods[0].ParameterTypes.Count);
            Assert.Equal("double", utilClass.Methods[0].CalledMethods[0].ParameterTypes[0].Type.Name);

            var clientClass =
                (ClassModel)actualRepositoryModel.Projects[0].CompilationUnits[1].ClassTypes[0];

            Assert.Equal(3, clientClass.Methods[0].CalledMethods.Count);

            var methodArray = new IMethodSkeletonType[]
            {
                clientClass.Methods[0],
                clientClass.Constructors[0],
            };

            foreach (var methodModel in methodArray)
            {
                var radicalCall = methodModel.CalledMethods[0];
                Assert.Equal("Radical", radicalCall.Name);
                Assert.Equal("Utils.Util", radicalCall.ContainingTypeName);
                Assert.Equal(1, radicalCall.ParameterTypes.Count);

                var intCall = methodModel.CalledMethods[1];
                Assert.Equal("Call", intCall.Name);
                Assert.Equal("Utils.IntWrapper", intCall.ContainingTypeName);
                Assert.Empty(intCall.ParameterTypes);

                var stringCall = methodModel.CalledMethods[2];
                Assert.Equal("Call", stringCall.Name);
                Assert.Equal("Utils.StringWrapper", stringCall.ContainingTypeName);
                Assert.Empty(stringCall.ParameterTypes);
            }

            Assert.Equal("double", clientClass.Methods[0].CalledMethods[0].ParameterTypes[0].Type.Name);
            Assert.Equal("System.Double", clientClass.Constructors[0].CalledMethods[0].ParameterTypes[0].Type.Name);
        }

        [Fact]
        public void Process_ShouldReturnFullName_WhenProvidedWithNamespaceAliasUsings()
        {
            const string fileContent1 = @"
namespace PC
{
    using Project = MyCompany.Project;

    public class A
    {
        public Project.MyClass M()
        {
            return new Project.MyClass();
        }
    }
}";

            const string fileContent2 = @"
namespace MyCompany
{
    using PC;

    namespace Project
    {
        using Company = MyCompany.Other;

        public class MyClass
        {
            public A Method()
            {
                var a = new A();
                a.M();
                new Company.SomeClass().F();
                return a;
            }
        }
    }

    namespace Other
    {
        public class SomeClass
        {
            public void F()
            {
            }
        }
    }
}
";

            var syntaxTree1 = _syntacticModelCreator.Create(fileContent1);
            var semanticModel1 = _semanticModelCreator.Create(syntaxTree1);

            var syntaxTree2 = _syntacticModelCreator.Create(fileContent2);
            var semanticModel2 = _semanticModelCreator.Create(syntaxTree2);

            var compilationUnit1 = _extractor.Extract(syntaxTree1, semanticModel1);
            var compilationUnit2 = _extractor.Extract(syntaxTree2, semanticModel2);

            var repositoryModel = new RepositoryModel();
            var solutionModel = new SolutionModel();
            var projectModel = new ProjectModel();

            projectModel.Add(compilationUnit1);
            projectModel.Add(compilationUnit2);

            repositoryModel.Projects.Add(projectModel);
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

            var classA = (ClassModel)actualRepositoryModel.Projects[0].CompilationUnits[0].ClassTypes[0];
            Assert.Equal("M", classA.Methods[0].Name);
            Assert.Equal("MyCompany.Project.MyClass", classA.Methods[0].ReturnValue.Type.Name);

            var myClass = (ClassModel)actualRepositoryModel.Projects[0].CompilationUnits[1].ClassTypes[0];

            Assert.Equal("Method", myClass.Methods[0].Name);
            Assert.Equal("PC.A", myClass.Methods[0].ReturnValue.Type.Name);

            Assert.Equal(2, myClass.Methods[0].CalledMethods.Count);

            Assert.Equal("M", myClass.Methods[0].CalledMethods[0].Name);
            Assert.Equal("PC.A", myClass.Methods[0].CalledMethods[0].ContainingTypeName);

            Assert.Equal("F", myClass.Methods[0].CalledMethods[1].Name);
            Assert.Equal("MyCompany.Other.SomeClass", myClass.Methods[0].CalledMethods[1].ContainingTypeName);
        }

        [Fact]
        public void Process_ShouldReturnFullName_WhenProvidedWithClassAliasUsings()
        {
            const string fileContent1 = @"

namespace NameSpace1
{
    public class MyClass
    {
        public override string ToString()
        {
            return ""MyClass"";
        }
    }
}";

            const string fileContent2 = @"
namespace NameSpace2
{
    class MyClass<T>
    {
        public override string ToString()
        {
            return ""MyClass<T>"";
        }
    }
}";
            const string fileContent3 = @"

namespace NameSpace3
{
    using System;
    using AliasToMyClass = NameSpace1.MyClass;
    using UsingAlias = NameSpace2.MyClass<int>;

    class MainClass
    {
        static void Main()
        {
            var instance1 = new AliasToMyClass();
            var s1 = instance1.ToString();

            var instance2 = new UsingAlias();
            var s2 = instance2.ToString();
        }
    }
}";

            var syntaxTree1 = _syntacticModelCreator.Create(fileContent1);
            var semanticModel1 = _semanticModelCreator.Create(syntaxTree1);

            var syntaxTree2 = _syntacticModelCreator.Create(fileContent2);
            var semanticModel2 = _semanticModelCreator.Create(syntaxTree2);

            var syntaxTree3 = _syntacticModelCreator.Create(fileContent3);
            var semanticModel3 = _semanticModelCreator.Create(syntaxTree3);

            var compilationUnit1 = _extractor.Extract(syntaxTree1, semanticModel1);
            var compilationUnit2 = _extractor.Extract(syntaxTree2, semanticModel2);
            var compilationUnit3 = _extractor.Extract(syntaxTree3, semanticModel3);

            var repositoryModel = new RepositoryModel();
            var solutionModel = new SolutionModel();
            var projectModel = new ProjectModel();

            projectModel.Add(compilationUnit1);
            projectModel.Add(compilationUnit2);
            projectModel.Add(compilationUnit3);

            repositoryModel.Projects.Add(projectModel);
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

            var mainClass =
                (ClassModel)actualRepositoryModel.Projects[0].CompilationUnits[2].ClassTypes[0];
            Assert.Equal("Main", mainClass.Methods[0].Name);

            Assert.Equal(2, mainClass.Methods[0].CalledMethods.Count);

            Assert.Equal("ToString", mainClass.Methods[0].CalledMethods[0].Name);
            Assert.Equal("NameSpace1.MyClass", mainClass.Methods[0].CalledMethods[0].ContainingTypeName);
            Assert.Empty(mainClass.Methods[0].CalledMethods[0].ParameterTypes);

            Assert.Equal("ToString", mainClass.Methods[0].CalledMethods[1].Name);
            Assert.Equal("NameSpace2.MyClass<int>", mainClass.Methods[0].CalledMethods[1].ContainingTypeName);
            Assert.Empty(mainClass.Methods[0].CalledMethods[1].ParameterTypes);
        }


        [Fact]
        public void Process_ShouldReturnFullNameOfClassAliasUsings_WhenProvidedUsings()
        {
            const string fileContent1 = @"
namespace Models
{
    namespace SomeModels
    {
        public class MyModel
        {
            public void Method()
            {
            }
        }
    }
}";

            const string fileContent2 = @"
namespace MyNamespace
{
    using System;
    using Models.SomeModels;

    namespace MyOtherNamespace
    {
        using M = MyModel;

        class MyClass
        {
            public M Call()
            {
                var m = new M();
                m.Method();
                return m;                
            }
        }
    }
}";

            var syntaxTree1 = _syntacticModelCreator.Create(fileContent1);
            var semanticModel1 = _semanticModelCreator.Create(syntaxTree1);

            var syntaxTree2 = _syntacticModelCreator.Create(fileContent2);
            var semanticModel2 = _semanticModelCreator.Create(syntaxTree2);

            var compilationUnit1 = _extractor.Extract(syntaxTree1, semanticModel1);
            var compilationUnit2 = _extractor.Extract(syntaxTree2, semanticModel2);

            var repositoryModel = new RepositoryModel();
            var solutionModel = new SolutionModel();

            var projectModel = new ProjectModel();

            projectModel.Add(compilationUnit1);
            projectModel.Add(compilationUnit2);

            repositoryModel.Projects.Add(projectModel);
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

            var myClass = (ClassModel)actualRepositoryModel.Projects[0].CompilationUnits[1].ClassTypes[0];

            Assert.Equal("Call", myClass.Methods[0].Name);
            Assert.Equal("Models.SomeModels.MyModel", myClass.Methods[0].ReturnValue.Type.Name);

            Assert.Equal(1, myClass.Methods[0].CalledMethods.Count);
            Assert.Equal("Method", myClass.Methods[0].CalledMethods[0].Name);
            Assert.Equal("Models.SomeModels.MyModel", myClass.Methods[0].CalledMethods[0].ContainingTypeName);
        }


        [Fact]
        public void Process_ShouldReturnFullNameOfClassAliasUsings_WhenProvidedSystemUsings()
        {
            const string fileContent = @"
namespace MyNamespace
{
    using System;
    using System.Collections;
    using System.Globalization;

    namespace MyOtherNamespace
    {
        using Comp = Comparer;

        class MyClass
        {
            public Comp Call()
            {
                var c = new Comp(CultureInfo.CurrentCulture);
                var s=c.ToString();
                return c;                
            }
        }
    }
}";

            var syntaxTree = _syntacticModelCreator.Create(fileContent);
            var semanticModel = _semanticModelCreator.Create(syntaxTree);

            var compilationUnit = _extractor.Extract(syntaxTree, semanticModel);

            var repositoryModel = new RepositoryModel();
            var solutionModel = new SolutionModel();

            var projectModel = new ProjectModel();

            projectModel.Add(compilationUnit);

            repositoryModel.Projects.Add(projectModel);
            repositoryModel.Solutions.Add(solutionModel);

            _progressLoggerMock.Setup(logger => logger.CreateProgressLogger(1, "Resolving Class Names"))
                .Returns(_progressLoggerBarMock.Object);
            _progressLoggerMock.Setup(logger =>
                    logger.CreateProgressLogger(1, "Resolving Using Statements for Each Class"))
                .Returns(_progressLoggerBarMock.Object);
            _progressLoggerMock.Setup(logger =>
                    logger.CreateProgressLogger(1, "Resolving Class Elements (Fields, Methods, Properties,...)"))
                .Returns(_progressLoggerBarMock.Object);


            var actualRepositoryModel = _sut.Process(repositoryModel);

            var myClass = (ClassModel)actualRepositoryModel.Projects[0].CompilationUnits[0].ClassTypes[0];

            Assert.Equal("Call", myClass.Methods[0].Name);
            Assert.Equal("System.Collections.Comparer", myClass.Methods[0].ReturnValue.Type.Name);

            Assert.Equal(1, myClass.Methods[0].CalledMethods.Count);
            Assert.Equal("ToString", myClass.Methods[0].CalledMethods[0].Name);
            Assert.Equal("System.Collections.Comparer", myClass.Methods[0].CalledMethods[0].ContainingTypeName);
        }

        [Theory]
        [InlineData(@"
public M m;
")]
        [InlineData(@"
public M m {get;set;}
")]
        [InlineData(@"
public M Call() {return null;}
")]
        [InlineData(@"
public void Call(M m) {}
")]
        [InlineData(@"
public void Call() { var m=new M();m.Method();}
")]
        [InlineData(@"public M Call()
            {
                var m = new M();
                m.Method();
                return m;                
            }")]
        [InlineData(@"
public int Value
{
    set
    {
        new M().Method(); 
        throw new System.NotImplementedException();
    }
}")]
        [InlineData(@"
public int Value
{
    get
    {
        M m = new();
        m.Method(); 
        throw new System.NotImplementedException();
    }
}")]
        public void Process_ShouldHaveAliasesWithCorrectType_WhenProvidedWithAliasClassUsings(string snippet)
        {
            const string fileContent1 = @"
namespace Models
{
    namespace SomeModels
    {
        public class MyModel
        {
            public void Method()
            {
            }
        }
    }
}";

            var fileContent2 = $@"
namespace MyNamespace
{{
    using System;
    using Models.SomeModels;

    namespace MyOtherNamespace
    {{
        using M = MyModel;

        class MyClass
        {{
            {snippet}            
        }}
    }}
}}";
            var syntaxTree1 = _syntacticModelCreator.Create(fileContent1);
            var semanticModel1 = _semanticModelCreator.Create(syntaxTree1);

            var syntaxTree2 = _syntacticModelCreator.Create(fileContent2);
            var semanticModel2 = _semanticModelCreator.Create(syntaxTree2);

            var compilationUnit1 = _extractor.Extract(syntaxTree1, semanticModel1);
            var compilationUnit2 = _extractor.Extract(syntaxTree2, semanticModel2);

            var repositoryModel = new RepositoryModel();
            var solutionModel = new SolutionModel();

            var projectModel = new ProjectModel();

            projectModel.Add(compilationUnit1);
            projectModel.Add(compilationUnit2);

            repositoryModel.Projects.Add(projectModel);
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

            var myClass = actualRepositoryModel.Projects[0].CompilationUnits[1].ClassTypes[0];

            Assert.Equal(nameof(EAliasType.Class), myClass.Imports[0].AliasType);
        }


        [Theory]
        [InlineData(@"using HoneydewTestProject.A;

namespace HoneydewTestProject
{
    public class ReferencesABViaNamespacePrefix
    {
        public void MethodC1A(C1 c1Params)
        {

        }

        public void MethodC1B(B.C1 c1Params)
        {

        }
    }
}")]
        [InlineData(@"using HoneydewTestProject.A;
using C1B = HoneydewTestProject.B.C1;

namespace HoneydewTestProject
{
    public class ReferencesABViaUsingAlias
    {
        public void MethodC1A(C1 c1Params)
        {

        }

        public void MethodC1B(C1B c1Params)
        {

        }
    }
}")]
        public void
            Process_ShouldReturnFullNameOfParameters_WhenProvidedWithMethodsWithTheSameClassNameInTheSameNamespace_ButInDifferentProjects(
                string fileContent)
        {
            const string fileContent2 = @"namespace HoneydewTestProject.A
{
    public class C1
    {
        public int X { get; set; }
    }
}";

            const string fileContent3 = @"namespace HoneydewTestProject.B
{
    public class C1
    {
        public int X { get; set; }
    }
}";
            var syntaxTree2 = _syntacticModelCreator.Create(fileContent2);
            var semanticModel2 = _semanticModelCreator.Create(syntaxTree2);

            var syntaxTree3 = _syntacticModelCreator.Create(fileContent3);
            var semanticModel3 = _semanticModelCreator.Create(syntaxTree3);

            var firstProjectCompilationUnit = _extractor.Extract(syntaxTree2, semanticModel2);
            var secondProjectCompilationUnit = _extractor.Extract(syntaxTree3, semanticModel3);

            var syntaxTree1 = _syntacticModelCreator.Create(fileContent);
            var semanticModel1 = _semanticModelCreator.Create(syntaxTree1);

            var compilationUnitType = _extractor.Extract(syntaxTree1, semanticModel1);

            var repositoryModel = new RepositoryModel();
            var solutionModel = new SolutionModel();

            var projectModel1 = new ProjectModel();

            projectModel1.Add(compilationUnitType);

            var projectModel2 = new ProjectModel();
            projectModel2.Add(firstProjectCompilationUnit);

            var projectModel3 = new ProjectModel();
            projectModel3.Add(secondProjectCompilationUnit);

            repositoryModel.Projects.Add(projectModel1);
            repositoryModel.Projects.Add(projectModel2);
            repositoryModel.Projects.Add(projectModel3);
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

            var myClass = (ClassModel)actualRepositoryModel.Projects[0].CompilationUnits[0].ClassTypes[0];

            Assert.Equal("MethodC1A", myClass.Methods[0].Name);
            Assert.Equal(1, myClass.Methods[0].ParameterTypes.Count);
            Assert.Equal("HoneydewTestProject.A.C1", myClass.Methods[0].ParameterTypes[0].Type.Name);

            Assert.Equal("MethodC1B", myClass.Methods[1].Name);
            Assert.Equal(1, myClass.Methods[1].ParameterTypes.Count);
            Assert.Equal("HoneydewTestProject.B.C1", myClass.Methods[1].ParameterTypes[0].Type.Name);
        }

        [Fact]
        public void
            Process_ShouldReturnFullNameOfParameters_WhenProvidedWithMethodsWithClassImportedFromAnotherProject_AndNamespaceIsImportedWithoutRedundantQualifier()
        {
            const string fileContent1 = @"namespace HoneydewTestProject
{
    using A;

    public class ReferencesOnlyAUsingInsideOfNamespaceNoRedundantQualifier
    {
        public void Method(C1 c1Params)
        {

        }
    }
}";
            const string fileContent2 = @"namespace HoneydewTestProject.A
{
    public class C1
    {
        public int X { get; set; }
    }
}";

            const string fileContent3 = @"namespace HoneydewTestProject.B
{
    public class C1
    {
        public int X { get; set; }
    }
}";
            var syntaxTree2 = _syntacticModelCreator.Create(fileContent2);
            var semanticModel2 = _semanticModelCreator.Create(syntaxTree2);

            var syntaxTree3 = _syntacticModelCreator.Create(fileContent3);
            var semanticModel3 = _semanticModelCreator.Create(syntaxTree3);

            var firstProjectCompilationUnit = _extractor.Extract(syntaxTree2, semanticModel2);
            var secondProjectCompilationUnit = _extractor.Extract(syntaxTree3, semanticModel3);

            var syntaxTree1 = _syntacticModelCreator.Create(fileContent1);
            var semanticModel1 = _semanticModelCreator.Create(syntaxTree1);

            var compilationUnitType = _extractor.Extract(syntaxTree1, semanticModel1);

            var repositoryModel = new RepositoryModel();
            var solutionModel = new SolutionModel();

            var projectModel1 = new ProjectModel();
            projectModel1.Add(compilationUnitType);

            var projectModel2 = new ProjectModel();
            projectModel2.Add(firstProjectCompilationUnit);


            var projectModel3 = new ProjectModel();
            projectModel3.Add(secondProjectCompilationUnit);


            repositoryModel.Projects.Add(projectModel1);
            repositoryModel.Projects.Add(projectModel2);
            repositoryModel.Projects.Add(projectModel3);
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

            var myClass = (ClassModel)actualRepositoryModel.Projects[0].CompilationUnits[0].ClassTypes[0];

            Assert.Equal("Method", myClass.Methods[0].Name);
            Assert.Equal(1, myClass.Methods[0].ParameterTypes.Count);
            Assert.Equal("HoneydewTestProject.A.C1", myClass.Methods[0].ParameterTypes[0].Type.Name);
        }

        [Fact]
        public void Process_ShouldReturnFullName_WhenProvidedWithClassInNamespaceAndChildNamespace()
        {
            const string fileContent1 = @"
namespace NameSpace1.N1
{
    public class MyClass { }
}";

            const string fileContent2 = @"
namespace NameSpace1.N1.Child
{
    public class MyClass { }
}";
            const string fileContent3 = @"using NameSpace1.N1;

namespace NameSpace3
{
    public class MainClass : MyClass
    {
        public void Function(MyClass c) {   }
    }
}";

            var syntaxTree1 = _syntacticModelCreator.Create(fileContent1);
            var semanticModel1 = _semanticModelCreator.Create(syntaxTree1);

            var syntaxTree2 = _syntacticModelCreator.Create(fileContent2);
            var semanticModel2 = _semanticModelCreator.Create(syntaxTree2);

            var syntaxTree3 = _syntacticModelCreator.Create(fileContent3);
            var semanticModel3 = _semanticModelCreator.Create(syntaxTree3);

            var compilationUnit1 = _extractor.Extract(syntaxTree1, semanticModel1);
            var compilationUnit2 = _extractor.Extract(syntaxTree2, semanticModel2);
            var compilationUnit3 = _extractor.Extract(syntaxTree3, semanticModel3);

            var repositoryModel = new RepositoryModel();
            var solutionModel = new SolutionModel();
            var projectModel = new ProjectModel();

            projectModel.Add(compilationUnit1);
            projectModel.Add(compilationUnit2);
            projectModel.Add(compilationUnit3);

            repositoryModel.Projects.Add(projectModel);
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

            var mainClass =
                (ClassModel)actualRepositoryModel.Projects[0].CompilationUnits[2].ClassTypes[0];
            Assert.Equal("NameSpace1.N1.MyClass", mainClass.BaseTypes[0].Type.Name);
            Assert.Equal("NameSpace1.N1.MyClass", mainClass.Methods[0].ParameterTypes[0].Type.Name);
        }

        [Fact]
        public void
            Process_ShouldReturnFullName_WhenProvidedWithClassInNamespaceAndChildNamespaceButUsedInSomeOtherChildNamespace()
        {
            const string fileContent1 = @"
namespace NameSpace1.N1
{
    public class MyClass { }
}";

            const string fileContent2 = @"
namespace NameSpace1.N1.Child
{
    public class MyClass { }
}";
            const string fileContent3 = @"
namespace NameSpace1.N1.OtherChild
{
    public class MainClass : MyClass
    {
        public MyClass Function() { return null;  }
    }
}";

            var syntaxTree1 = _syntacticModelCreator.Create(fileContent1);
            var semanticModel1 = _semanticModelCreator.Create(syntaxTree1);

            var syntaxTree2 = _syntacticModelCreator.Create(fileContent2);
            var semanticModel2 = _semanticModelCreator.Create(syntaxTree2);

            var syntaxTree3 = _syntacticModelCreator.Create(fileContent3);
            var semanticModel3 = _semanticModelCreator.Create(syntaxTree3);

            var compilationUnit1 = _extractor.Extract(syntaxTree1, semanticModel1);
            var compilationUnit2 = _extractor.Extract(syntaxTree2, semanticModel2);
            var compilationUnit3 = _extractor.Extract(syntaxTree3, semanticModel3);

            var repositoryModel = new RepositoryModel();
            var solutionModel = new SolutionModel();
            var projectModel = new ProjectModel();

            projectModel.Add(compilationUnit1);
            projectModel.Add(compilationUnit2);
            projectModel.Add(compilationUnit3);

            repositoryModel.Projects.Add(projectModel);
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

            var mainClass =
                (ClassModel)actualRepositoryModel.Projects[0].CompilationUnits[2].ClassTypes[0];
            Assert.Equal("NameSpace1.N1.MyClass", mainClass.BaseTypes[0].Type.Name);
            Assert.Equal("NameSpace1.N1.MyClass", mainClass.Methods[0].ReturnValue.Type.Name);
        }

        [Fact]
        public void
            Process_ShouldReturnAttributeFullName_WhenProvidedWithCustomAttribute()
        {
            const string fileContent1 = @"
namespace AttributeNamespace
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    class MyAttribute : Attribute
    {
        public MyAttribute()
        {
        }
    }
}";

            const string fileContent2 = @"
using AttributeNamespace;

namespace NameSpace1
{
    [My]
    public class MyClass 
    {
        [return: My]
        [method: MyAttribute]
        public void Method([My] int value)
        {
        }
    }
}";
            var syntaxTree1 = _syntacticModelCreator.Create(fileContent1);
            var semanticModel1 = _semanticModelCreator.Create(syntaxTree1);

            var syntaxTree2 = _syntacticModelCreator.Create(fileContent2);
            var semanticModel2 = _semanticModelCreator.Create(syntaxTree2);

            var compilationUnit1 = _extractor.Extract(syntaxTree1, semanticModel1);
            var compilationUnit2 = _extractor.Extract(syntaxTree2, semanticModel2);

            var repositoryModel = new RepositoryModel();
            var solutionModel = new SolutionModel();
            var projectModel = new ProjectModel();

            projectModel.Add(compilationUnit1);
            projectModel.Add(compilationUnit2);

            repositoryModel.Projects.Add(projectModel);
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

            var mainClass =
                (ClassModel)actualRepositoryModel.Projects[0].CompilationUnits[1].ClassTypes[0];

            var attributes = new[]
            {
                mainClass.Attributes[0],
                mainClass.Methods[0].Attributes[0],
                mainClass.Methods[0].ReturnValue.Attributes[0],
                mainClass.Methods[0].ParameterTypes[0].Attributes[0],
            };

            foreach (var attributeType in attributes)
            {
                Assert.Equal("AttributeNamespace.MyAttribute", attributeType.Name);
            }
        }
    }
}
