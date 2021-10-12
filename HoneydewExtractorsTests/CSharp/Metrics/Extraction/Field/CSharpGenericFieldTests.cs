using System.Collections.Generic;
using HoneydewCore.Logging;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewExtractors.Core.Metrics.Visitors.Fields;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewExtractors.CSharp.Metrics.Extraction.Class;
using HoneydewExtractors.CSharp.Metrics.Extraction.CompilationUnit;
using HoneydewExtractors.CSharp.Metrics.Extraction.Field;
using HoneydewModels.CSharp;
using Moq;
using Xunit;

namespace HoneydewExtractorsTests.CSharp.Metrics.Extraction.Field
{
    public class CSharpGenericFieldTests
    {
        private readonly CSharpFactExtractor _factExtractor;
        private readonly Mock<ILogger> _loggerMock = new();
        private readonly CSharpSyntacticModelCreator _syntacticModelCreator = new();
        private readonly CSharpSemanticModelCreator _semanticModelCreator = new(new CSharpCompilationMaker());

        public CSharpGenericFieldTests()
        {
            var compositeVisitor = new CompositeVisitor();

            compositeVisitor.Add(new ClassSetterCompilationUnitVisitor(new List<ICSharpClassVisitor>
            {
                new BaseInfoClassVisitor(),
                new FieldSetterClassVisitor(new List<IFieldVisitor>
                {
                    new FieldInfoVisitor(),
                })
            }));

            compositeVisitor.Accept(new LoggerSetterVisitor(_loggerMock.Object));

            _factExtractor = new CSharpFactExtractor(compositeVisitor);
        }

        [Theory]
        [InlineData("class")]
        [InlineData("record")]
        [InlineData("struct")]
        public void Extract_ShouldHaveFieldOfGenericType_WhenProvidedDifferentClassType(string classType)
        {
            var fileContent = $@"namespace Namespace1
{{
    public {classType} Class1 
    {{       
        public GenericClass<string> GField;    

        public event System.Func<int> FField;
    }}

    public class GenericClass<T> {{}}
}}";

                      var syntaxTree = _syntacticModelCreator.Create(fileContent);
            var semanticModel = _semanticModelCreator.Create(syntaxTree);
            var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

            var classModel = (ClassModel)classTypes[0];

            Assert.Equal(2, classModel.Fields.Count);

            var field1 = classModel.Fields[0];
            Assert.Equal("Namespace1.GenericClass<string>", field1.Type.Name);
            Assert.Equal(1, field1.Type.FullType.ContainedTypes.Count);
            Assert.Equal("Namespace1.GenericClass", field1.Type.FullType.Name);
            Assert.Equal(1, field1.Type.FullType.ContainedTypes.Count);
            Assert.Equal("string", field1.Type.FullType.ContainedTypes[0].Name);
            Assert.Empty(field1.Type.FullType.ContainedTypes[0].ContainedTypes);

            var field2 = classModel.Fields[1];
            Assert.Equal("System.Func<int>", field2.Type.Name);
            Assert.Equal(1, field2.Type.FullType.ContainedTypes.Count);
            Assert.Equal("System.Func", field2.Type.FullType.Name);
            Assert.Equal(1, field2.Type.FullType.ContainedTypes.Count);
            Assert.Equal("int", field2.Type.FullType.ContainedTypes[0].Name);
            Assert.Empty(field2.Type.FullType.ContainedTypes[0].ContainedTypes);
        }


        [Theory]
        [InlineData("class")]
        [InlineData("record")]
        [InlineData("struct")]
        public void Extract_ShouldHaveFieldOfGenericTypeWithMultipleContainedTypes_WhenProvidedDifferentClassType(
            string classType)
        {
            var fileContent = $@"namespace Namespace1
{{
    public {classType} Class1 
    {{       
        public GenericClass<string,int, double> GField;    

        public GenericClass<GenericClass<int,string,double>, char, GenericClass<long,string,float>> FField;
    }}

    public class GenericClass<T,R,K> {{}}
}}";

                      var syntaxTree = _syntacticModelCreator.Create(fileContent);
            var semanticModel = _semanticModelCreator.Create(syntaxTree);
            var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

            var classModel = (ClassModel)classTypes[0];

            Assert.Equal(2, classModel.Fields.Count);

            var field1 = classModel.Fields[0];
            Assert.Equal("Namespace1.GenericClass<string, int, double>", field1.Type.Name);
            Assert.Equal("Namespace1.GenericClass", field1.Type.FullType.Name);
            Assert.Equal(3, field1.Type.FullType.ContainedTypes.Count);
            var containedType1 = field1.Type.FullType.ContainedTypes[0];
            Assert.Equal("string", containedType1.Name);
            Assert.Empty(containedType1.ContainedTypes);

            var containedType2 = field1.Type.FullType.ContainedTypes[1];
            Assert.Equal("int", containedType2.Name);
            Assert.Empty(containedType2.ContainedTypes);

            var containedType3 = field1.Type.FullType.ContainedTypes[2];
            Assert.Equal("double", containedType3.Name);
            Assert.Empty(containedType3.ContainedTypes);

            var field2 = classModel.Fields[1];
            Assert.Equal(
                "Namespace1.GenericClass<Namespace1.GenericClass<int, string, double>, char, Namespace1.GenericClass<long, string, float>>",
                field2.Type.Name);
            Assert.Equal("Namespace1.GenericClass", field2.Type.FullType.Name);
            Assert.Equal(3, field2.Type.FullType.ContainedTypes.Count);
            var containedType4 = field2.Type.FullType.ContainedTypes[0];
            Assert.Equal("Namespace1.GenericClass", containedType4.Name);
            Assert.Equal(3, containedType4.ContainedTypes.Count);

            var containedType7 = containedType4.ContainedTypes[0];
            Assert.Equal("int", containedType7.Name);
            Assert.Empty(containedType7.ContainedTypes);

            var containedType8 = containedType4.ContainedTypes[1];
            Assert.Equal("string", containedType8.Name);
            Assert.Empty(containedType8.ContainedTypes);

            var containedType9 = containedType4.ContainedTypes[2];
            Assert.Equal("double", containedType9.Name);
            Assert.Empty(containedType9.ContainedTypes);


            var containedType5 = field2.Type.FullType.ContainedTypes[1];
            Assert.Equal("char", containedType5.Name);
            Assert.Empty(containedType5.ContainedTypes);


            var containedType6 = field2.Type.FullType.ContainedTypes[2];
            Assert.Equal("Namespace1.GenericClass", containedType6.Name);
            Assert.Equal(3, containedType6.ContainedTypes.Count);

            var containedType10 = containedType6.ContainedTypes[0];
            Assert.Equal("long", containedType10.Name);
            Assert.Empty(containedType10.ContainedTypes);

            var containedType11 = containedType6.ContainedTypes[1];
            Assert.Equal("string", containedType11.Name);
            Assert.Empty(containedType11.ContainedTypes);

            var containedType12 = containedType6.ContainedTypes[2];
            Assert.Equal("float", containedType12.Name);
            Assert.Empty(containedType12.ContainedTypes);
        }
    }
}
