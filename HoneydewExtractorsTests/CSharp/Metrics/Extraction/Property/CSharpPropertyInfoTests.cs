using System.Collections.Generic;
using HoneydewCore.Logging;
using HoneydewExtractors.Core.Metrics.Extraction.Class;
using HoneydewExtractors.Core.Metrics.Extraction.Common;
using HoneydewExtractors.Core.Metrics.Extraction.CompilationUnit;
using HoneydewExtractors.Core.Metrics.Extraction.Method;
using HoneydewExtractors.Core.Metrics.Extraction.MethodCall;
using HoneydewExtractors.Core.Metrics.Extraction.Property;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewExtractors.Core.Metrics.Visitors.Methods;
using HoneydewExtractors.Core.Metrics.Visitors.MethodSignatures;
using HoneydewExtractors.Core.Metrics.Visitors.Properties;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewModels.CSharp;
using Moq;
using Xunit;

namespace HoneydewExtractorsTests.CSharp.Metrics.Extraction.Property
{
    public class CSharpPropertyInfoTests
    {
        private readonly CSharpFactExtractor _sut;
        private readonly Mock<ILogger> _loggerMock = new();

        public CSharpPropertyInfoTests()
        {
            var compositeVisitor = new CompositeVisitor();

            var calledMethodSetterVisitor = new CalledMethodSetterVisitor(new List<ICSharpMethodSignatureVisitor>
            {
                new MethodCallInfoVisitor()
            });

            var methodInfoVisitor = new MethodInfoVisitor();
            compositeVisitor.Add(new ClassSetterCompilationUnitVisitor(new List<ICSharpClassVisitor>
            {
                new BaseInfoClassVisitor(),
                new PropertySetterClassVisitor(new List<ICSharpPropertyVisitor>
                {
                    new PropertyInfoVisitor(),
                    new MethodAccessorSetterPropertyVisitor(new List<IMethodVisitor>
                    {
                        methodInfoVisitor,
                        calledMethodSetterVisitor
                    })
                })
            }));

            compositeVisitor.Accept(new LoggerSetterVisitor(_loggerMock.Object));

            _sut = new CSharpFactExtractor(new CSharpSyntacticModelCreator(),
                new CSharpSemanticModelCreator(new CSharpCompilationMaker()), compositeVisitor);
        }

        [Theory]
        [InlineData("class")]
        [InlineData("interface")]
        [InlineData("record")]
        [InlineData("struct")]
        public void Extract_ShouldHavePropertyInfo_WhenGivenTypeWithProperty(string entityType)
        {
            var fileContent = $@"namespace Models.Main.Items
{{
    public {entityType} MainItem
    {{
        public int Value {{get;set;}}
    }}
}}";
            var classTypes = _sut.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes.Count);

            var classModel = (ClassModel)classTypes[0];

            Assert.Equal(1, classModel.Properties.Count);
            var property = classModel.Properties[0];
            Assert.Equal("Value", property.Name);
            Assert.Equal("public", property.AccessModifier);
            Assert.Equal("Models.Main.Items.MainItem", property.ContainingTypeName);
            Assert.Equal("int", property.Type.Name);
            Assert.False(property.IsEvent);
            Assert.Equal(2, property.Accessors.Count);
            Assert.Equal("get", property.Accessors[0].Name);
            Assert.Equal("set", property.Accessors[1].Name);
        }

        [Theory]
        [InlineData("class")]
        [InlineData("interface")]
        [InlineData("record")]
        [InlineData("struct")]
        public void Extract_ShouldHaveEventPropertyInfo_WhenGivenTypeWithProperty(string entityType)
        {
            var fileContent = $@"namespace Models.Main.Items
{{
    public {entityType} MainItem
    {{
        public event System.Func<int> Value {{add{{}} remove{{}}}}
    }}
}}";
            var classTypes = _sut.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes.Count);

            var classModel = (ClassModel)classTypes[0];

            Assert.Equal(1, classModel.Properties.Count);
            var property = classModel.Properties[0];
            Assert.Equal("Value", property.Name);
            Assert.Equal("public", property.AccessModifier);
            Assert.Equal("Models.Main.Items.MainItem", property.ContainingTypeName);
            Assert.Equal("System.Func<int>", property.Type.Name);
            Assert.True(property.IsEvent);
            Assert.Equal(2, property.Accessors.Count);
            Assert.Equal("add", property.Accessors[0].Name);
            Assert.Equal("remove", property.Accessors[1].Name);
        }

        [Fact]
        public void Extract_ShouldHaveAbstractModifier_WhenGivenPropertyInInterface()
        {
            const string fileContent = @"namespace Models.Main.Items
{
    public interface MainItem
    {
        public int Value { get; set; }
    }
}";
            var classTypes = _sut.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes.Count);

            var classModel = (ClassModel)classTypes[0];

            Assert.Equal(1, classModel.Properties.Count);
            var property = classModel.Properties[0];
            Assert.Equal("Value", property.Name);
            Assert.Equal("public", property.AccessModifier);
            Assert.Equal("abstract", property.Modifier);
            Assert.Equal("Models.Main.Items.MainItem", property.ContainingTypeName);
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/ClassLevel/CSharpPropertiesInfo/Accessors/ClassWithPropertyOnlyGet.txt")]
        public void Extract_ShouldHaveOnlyGetAccessors_WhenGivenClassWithProperty(string fileContent)
        {
            var classTypes = _sut.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes.Count);

            var classModel = (ClassModel)classTypes[0];

            Assert.Equal(3, classModel.Properties.Count);
            foreach (var property in classModel.Properties)
            {
                Assert.Equal("Namespace1.Class1", property.ContainingTypeName);
                Assert.Equal(1, property.Accessors.Count);
                Assert.Equal("get", property.Accessors[0].Name);
            }

            Assert.Equal("Namespace1.Class1.JustGet",
                classModel.Properties[0].Accessors[0].ContainingTypeName);
            Assert.Equal("Namespace1.Class1.JustGetBody",
                classModel.Properties[1].Accessors[0].ContainingTypeName);
            Assert.Equal("Namespace1.Class1.GetExpressionBody",
                classModel.Properties[2].Accessors[0].ContainingTypeName);
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/ClassLevel/CSharpPropertiesInfo/Accessors/ClassWithPropertyOnlySet.txt")]
        public void Extract_ShouldHaveOnlySetAccessors_WhenGivenClassWithProperty(string fileContent)
        {
            var classTypes = _sut.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes.Count);

            var classModel = (ClassModel)classTypes[0];

            Assert.Equal(2, classModel.Properties.Count);
            foreach (var property in classModel.Properties)
            {
                Assert.Equal("Namespace1.Class1", property.ContainingTypeName);
                Assert.Equal(1, property.Accessors.Count);
                Assert.Equal("set", property.Accessors[0].Name);
            }

            Assert.Equal("Namespace1.Class1.JustSet",
                classModel.Properties[0].Accessors[0].ContainingTypeName);
            Assert.Equal("Namespace1.Class1.SetExpressionBody",
                classModel.Properties[1].Accessors[0].ContainingTypeName);
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/ClassLevel/CSharpPropertiesInfo/Accessors/ClassWithPropertyOnlyInit.txt")]
        public void Extract_ShouldHaveOnlyInitAccessors_WhenGivenClassWithProperty(string fileContent)
        {
            var classTypes = _sut.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes.Count);

            var classModel = (ClassModel)classTypes[0];

            Assert.Equal(2, classModel.Properties.Count);
            foreach (var property in classModel.Properties)
            {
                Assert.Equal("Namespace1.Class1", property.ContainingTypeName);
                Assert.Equal(1, property.Accessors.Count);
                Assert.Equal("init", property.Accessors[0].Name);
            }

            Assert.Equal("Namespace1.Class1.JustInit",
                classModel.Properties[0].Accessors[0].ContainingTypeName);
            Assert.Equal("Namespace1.Class1.InitExpressionBody",
                classModel.Properties[1].Accessors[0].ContainingTypeName);
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/ClassLevel/CSharpPropertiesInfo/Accessors/ClassWithPropertyGetSet.txt")]
        public void Extract_ShouldHaveGetSetAccessors_WhenGivenClassWithProperty(string fileContent)
        {
            var classTypes = _sut.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes.Count);

            var classModel = (ClassModel)classTypes[0];

            Assert.Equal(2, classModel.Properties.Count);
            foreach (var property in classModel.Properties)
            {
                Assert.Equal("Namespace1.Class1", property.ContainingTypeName);
                Assert.Equal(2, property.Accessors.Count);
                Assert.Equal("get", property.Accessors[0].Name);
                Assert.Equal("set", property.Accessors[1].Name);
            }

            Assert.Equal("Namespace1.Class1.GetSet", classModel.Properties[0].Accessors[0].ContainingTypeName);
            Assert.Equal("Namespace1.Class1.GetSet", classModel.Properties[0].Accessors[1].ContainingTypeName);
            Assert.Equal("Namespace1.Class1.GetSetExpressionBody",
                classModel.Properties[1].Accessors[0].ContainingTypeName);
            Assert.Equal("Namespace1.Class1.GetSetExpressionBody",
                classModel.Properties[1].Accessors[1].ContainingTypeName);
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/ClassLevel/CSharpPropertiesInfo/Accessors/ClassWithPropertyGetInit.txt")]
        public void Extract_ShouldHaveGetInitAccessors_WhenGivenClassWithProperty(string fileContent)
        {
            var classTypes = _sut.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes.Count);

            var classModel = (ClassModel)classTypes[0];

            Assert.Equal(2, classModel.Properties.Count);
            foreach (var property in classModel.Properties)
            {
                Assert.Equal("Namespace1.Class1", property.ContainingTypeName);
                Assert.Equal(2, property.Accessors.Count);
                Assert.Equal("get", property.Accessors[0].Name);
                Assert.Equal("init", property.Accessors[1].Name);
            }

            Assert.Equal("Namespace1.Class1.GetInit",
                classModel.Properties[0].Accessors[0].ContainingTypeName);
            Assert.Equal("Namespace1.Class1.GetInit",
                classModel.Properties[0].Accessors[1].ContainingTypeName);
            Assert.Equal("Namespace1.Class1.GetInitExpressionBody",
                classModel.Properties[1].Accessors[0].ContainingTypeName);
            Assert.Equal("Namespace1.Class1.GetInitExpressionBody",
                classModel.Properties[1].Accessors[1].ContainingTypeName);
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/ClassLevel/CSharpPropertiesInfo/Accessors/ClassWithPropertyExpressionBody_CalledMethods.txt")]
        public void Extract_ShouldCalledMethods_WhenGivenPropertyWithExpressionBodiedMember(string fileContent)
        {
            var classTypes = _sut.Extract(fileContent).ClassTypes;

            Assert.Equal(2, classTypes.Count);

            var classModel = (ClassModel)classTypes[0];

            Assert.Equal(1, classModel.Properties.Count);
            var property = classModel.Properties[0];
            Assert.Equal("Namespace1.Class1", property.ContainingTypeName);
            Assert.Equal("GetExpressionBody", property.Name);
            Assert.Equal(1, property.Accessors.Count);
            Assert.Equal("get", property.Accessors[0].Name);

            var accessor = property.Accessors[0];
            Assert.Equal("Namespace1.Class1.GetExpressionBody",
                accessor.ContainingTypeName);

            Assert.Equal(3, accessor.CalledMethods.Count);

            Assert.Equal("Method", accessor.CalledMethods[0].Name);
            Assert.Equal("ExternClass", accessor.CalledMethods[0].ContainingTypeName);
            Assert.Equal(1, accessor.CalledMethods[0].ParameterTypes.Count);
            Assert.Equal("int", accessor.CalledMethods[0].ParameterTypes[0].Type.Name);

            Assert.Equal("Method2", accessor.CalledMethods[1].Name);
            Assert.Equal("Namespace1.Class2", accessor.CalledMethods[1].ContainingTypeName);
            Assert.Equal(1, accessor.CalledMethods[1].ParameterTypes.Count);
            Assert.Equal("int", accessor.CalledMethods[1].ParameterTypes[0].Type.Name);

            Assert.Equal("MyMethod", accessor.CalledMethods[2].Name);
            Assert.Equal("Namespace1.Class1", accessor.CalledMethods[2].ContainingTypeName);
            Assert.Empty(accessor.CalledMethods[2].ParameterTypes);
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/ClassLevel/CSharpPropertiesInfo/Accessors/ClassWithPropertiesWithStatementBody.txt")]
        public void Extract_ShouldHaveCyclomaticComplexity_WhenGivenClassWithProperties(string fileContent)
        {
            var classTypes = _sut.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes.Count);

            var classModel = (ClassModel)classTypes[0];

            Assert.Equal(2, classModel.Properties.Count);

            foreach (var property in classModel.Properties)
            {
                Assert.Equal(12, property.CyclomaticComplexity);

                foreach (var accessor in property.Accessors)
                {
                    Assert.Equal(6, accessor.CyclomaticComplexity);
                }
            }
        }
    }
}
