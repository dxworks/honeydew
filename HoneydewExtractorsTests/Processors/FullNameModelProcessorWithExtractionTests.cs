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
        private readonly Mock<IProgressLogger> _progressLoggerMock = new();
        private readonly Mock<IProgressLoggerBar> _progressLoggerBarMock = new();
        private readonly CSharpFactExtractor _extractor;

        public FullNameModelProcessorWithExtractionTests()
        {
            _sut = new FullNameModelProcessor(_loggerMock.Object, _progressLoggerMock.Object, false);

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

            _extractor = new CSharpFactExtractor(new CSharpSyntacticModelCreator(),
                new CSharpSemanticModelCreator(new CSharpCompilationMaker(_loggerMock.Object)), compositeVisitor);
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

            var classModels1 = _extractor.Extract(fileContent1).ClassTypes;
            var classModels2 = _extractor.Extract(fileContent2).ClassTypes;

            var repositoryModel = new RepositoryModel();
            var solutionModel = new SolutionModel();
            var projectModel = new ProjectModel();

            foreach (var classModel in classModels1)
            {
                projectModel.Add((ClassModel)classModel);
            }

            foreach (var classModel in classModels2)
            {
                projectModel.Add((ClassModel)classModel);
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

            var model = (ClassModel)actualRepositoryModel.Solutions[0].Projects[0].Namespaces[1].ClassModels[0];
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

            var classModels1 = _extractor.Extract(fileContent1).ClassTypes;
            var classModels2 = _extractor.Extract(fileContent2).ClassTypes;

            var repositoryModel = new RepositoryModel();
            var solutionModel = new SolutionModel();
            var projectModel = new ProjectModel();

            foreach (var classModel in classModels1)
            {
                projectModel.Add((ClassModel)classModel);
            }

            foreach (var classModel in classModels2)
            {
                projectModel.Add((ClassModel)classModel);
            }

            solutionModel.Projects.Add(projectModel);
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

            var utilClass = (ClassModel)actualRepositoryModel.Solutions[0].Projects[0].Namespaces[0].ClassModels[2];
            Assert.Equal("Sqrt", utilClass.Methods[0].CalledMethods[0].Name);
            Assert.Equal("System.Math", utilClass.Methods[0].CalledMethods[0].ContainingTypeName);
            Assert.Equal(1, utilClass.Methods[0].CalledMethods[0].ParameterTypes.Count);
            Assert.Equal("double", utilClass.Methods[0].CalledMethods[0].ParameterTypes[0].Type.Name);

            var clientClass = (ClassModel)actualRepositoryModel.Solutions[0].Projects[0].Namespaces[1].ClassModels[0];

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

            var classModels1 = _extractor.Extract(fileContent1).ClassTypes;
            var classModels2 = _extractor.Extract(fileContent2).ClassTypes;

            var repositoryModel = new RepositoryModel();
            var solutionModel = new SolutionModel();
            var projectModel = new ProjectModel();

            foreach (var classModel in classModels1)
            {
                projectModel.Add((ClassModel)classModel);
            }

            foreach (var classModel in classModels2)
            {
                projectModel.Add((ClassModel)classModel);
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

            var classA = (ClassModel)actualRepositoryModel.Solutions[0].Projects[0].Namespaces[0].ClassModels[0];
            Assert.Equal("M", classA.Methods[0].Name);
            Assert.Equal("MyCompany.Project.MyClass", classA.Methods[0].ReturnValue.Type.Name);

            var myClass = (ClassModel)actualRepositoryModel.Solutions[0].Projects[0].Namespaces[1].ClassModels[0];

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

            var classModels1 = _extractor.Extract(fileContent1).ClassTypes;
            var classModels2 = _extractor.Extract(fileContent2).ClassTypes;
            var classModels3 = _extractor.Extract(fileContent3).ClassTypes;

            var repositoryModel = new RepositoryModel();
            var solutionModel = new SolutionModel();
            var projectModel = new ProjectModel();

            foreach (var classModel in classModels1)
            {
                projectModel.Add((ClassModel)classModel);
            }

            foreach (var classModel in classModels2)
            {
                projectModel.Add((ClassModel)classModel);
            }

            foreach (var classModel in classModels3)
            {
                projectModel.Add((ClassModel)classModel);
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

            var mainClass = (ClassModel)actualRepositoryModel.Solutions[0].Projects[0].Namespaces[2].ClassModels[0];
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

            var classModels1 = _extractor.Extract(fileContent1).ClassTypes;
            var classModels2 = _extractor.Extract(fileContent2).ClassTypes;

            var repositoryModel = new RepositoryModel();
            var solutionModel = new SolutionModel();

            var projectModel = new ProjectModel();
            foreach (var classModel in classModels1)
            {
                projectModel.Add((ClassModel)classModel);
            }

            foreach (var classModel in classModels2)
            {
                projectModel.Add((ClassModel)classModel);
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

            var myClass = (ClassModel)actualRepositoryModel.Solutions[0].Projects[0].Namespaces[1].ClassModels[0];

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

            var classTypes = _extractor.Extract(fileContent).ClassTypes;

            var repositoryModel = new RepositoryModel();
            var solutionModel = new SolutionModel();

            var projectModel = new ProjectModel();

            foreach (var classType in classTypes)
            {
                var classModel = (ClassModel)classType;
                projectModel.Add(classModel);
            }

            solutionModel.Projects.Add(projectModel);
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

            var myClass = (ClassModel)actualRepositoryModel.Solutions[0].Projects[0].Namespaces[0].ClassModels[0];

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
            var classModels1 = _extractor.Extract(fileContent1).ClassTypes;
            var classModels2 = _extractor.Extract(fileContent2).ClassTypes;

            var repositoryModel = new RepositoryModel();
            var solutionModel = new SolutionModel();

            var projectModel = new ProjectModel();
            foreach (var classModel in classModels1)
            {
                projectModel.Add((ClassModel)classModel);
            }

            foreach (var classModel in classModels2)
            {
                projectModel.Add((ClassModel)classModel);
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

            var myClass = actualRepositoryModel.Solutions[0].Projects[0].Namespaces[1].ClassModels[0];

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

            var firstProjectClassModels = _extractor.Extract(fileContent2).ClassTypes;
            var secondProjectClassModels = _extractor.Extract(fileContent3).ClassTypes;

            var classTypes = _extractor.Extract(fileContent).ClassTypes;

            var repositoryModel = new RepositoryModel();
            var solutionModel = new SolutionModel();

            var projectModel1 = new ProjectModel();
            foreach (var classType in classTypes)
            {
                var classModel = (ClassModel)classType;
                projectModel1.Add(classModel);
            }

            var projectModel2 = new ProjectModel();
            foreach (var classModel in firstProjectClassModels)
            {
                projectModel2.Add((ClassModel)classModel);
            }

            var projectModel3 = new ProjectModel();
            foreach (var classModel in secondProjectClassModels)
            {
                projectModel3.Add((ClassModel)classModel);
            }

            solutionModel.Projects.Add(projectModel1);
            solutionModel.Projects.Add(projectModel2);
            solutionModel.Projects.Add(projectModel3);
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

            var myClass = (ClassModel)actualRepositoryModel.Solutions[0].Projects[0].Namespaces[0].ClassModels[0];

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

            var firstProjectClassModels = _extractor.Extract(fileContent2).ClassTypes;
            var secondProjectClassModels = _extractor.Extract(fileContent3).ClassTypes;

            var classTypes = _extractor.Extract(fileContent1).ClassTypes;

            var repositoryModel = new RepositoryModel();
            var solutionModel = new SolutionModel();

            var projectModel1 = new ProjectModel();
            foreach (var classType in classTypes)
            {
                projectModel1.Add((ClassModel)classType);
            }

            var projectModel2 = new ProjectModel();
            foreach (var classModel in firstProjectClassModels)
            {
                projectModel2.Add((ClassModel)classModel);
            }

            var projectModel3 = new ProjectModel();
            foreach (var classModel in secondProjectClassModels)
            {
                projectModel3.Add((ClassModel)classModel);
            }

            solutionModel.Projects.Add(projectModel1);
            solutionModel.Projects.Add(projectModel2);
            solutionModel.Projects.Add(projectModel3);
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

            var myClass = (ClassModel)actualRepositoryModel.Solutions[0].Projects[0].Namespaces[0].ClassModels[0];

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

            var classModels1 = _extractor.Extract(fileContent1).ClassTypes;
            var classModels2 = _extractor.Extract(fileContent2).ClassTypes;
            var classModels3 = _extractor.Extract(fileContent3).ClassTypes;

            var repositoryModel = new RepositoryModel();
            var solutionModel = new SolutionModel();
            var projectModel = new ProjectModel();

            foreach (var classModel in classModels1)
            {
                projectModel.Add((ClassModel)classModel);
            }

            foreach (var classModel in classModels2)
            {
                projectModel.Add((ClassModel)classModel);
            }

            foreach (var classModel in classModels3)
            {
                projectModel.Add((ClassModel)classModel);
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

            var mainClass = (ClassModel)actualRepositoryModel.Solutions[0].Projects[0].Namespaces[2].ClassModels[0];
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

            var classModels1 = _extractor.Extract(fileContent1).ClassTypes;
            var classModels2 = _extractor.Extract(fileContent2).ClassTypes;
            var classModels3 = _extractor.Extract(fileContent3).ClassTypes;

            var repositoryModel = new RepositoryModel();
            var solutionModel = new SolutionModel();
            var projectModel = new ProjectModel();

            foreach (var classModel in classModels1)
            {
                projectModel.Add((ClassModel)classModel);
            }

            foreach (var classModel in classModels2)
            {
                projectModel.Add((ClassModel)classModel);
            }

            foreach (var classModel in classModels3)
            {
                projectModel.Add((ClassModel)classModel);
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

            var mainClass = (ClassModel)actualRepositoryModel.Solutions[0].Projects[0].Namespaces[2].ClassModels[0];
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
            var classModels1 = _extractor.Extract(fileContent1).ClassTypes;
            var classModels2 = _extractor.Extract(fileContent2).ClassTypes;

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

            var mainClass = (ClassModel)actualRepositoryModel.Solutions[0].Projects[0].Namespaces[1].ClassModels[0];

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
