using System.Collections.Generic;
using HoneydewCore.Logging;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewExtractors.Core.Metrics.Visitors.Parameters;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewExtractors.CSharp.Metrics.Extraction.Class;
using HoneydewExtractors.CSharp.Metrics.Extraction.Common;
using HoneydewExtractors.CSharp.Metrics.Extraction.CompilationUnit;
using HoneydewExtractors.CSharp.Metrics.Extraction.Parameter;
using HoneydewModels.CSharp;
using Moq;
using Xunit;

namespace HoneydewExtractorsTests.CSharp.Metrics.Extraction.ClassLevel
{
    public class CSharpGenericClassTests
    {
        private readonly CSharpFactExtractor _factExtractor;
        private readonly Mock<ILogger> _loggerMock = new();

        public CSharpGenericClassTests()
        {
            var compositeVisitor = new CompositeVisitor();

            compositeVisitor.Add(new ClassSetterCompilationUnitVisitor(new List<ICSharpClassVisitor>
            {
                new BaseInfoClassVisitor(),
                new BaseTypesClassVisitor(),
                new GenericParameterSetterVisitor(new List<IGenericParameterVisitor>
                {
                    new GenericParameterInfoVisitor()
                })
            }));

            compositeVisitor.Accept(new LoggerSetterVisitor(_loggerMock.Object));

            _factExtractor = new CSharpFactExtractor(new CSharpSyntacticModelCreator(),
                new CSharpSemanticModelCreator(new CSharpCompilationMaker(_loggerMock.Object)), compositeVisitor);
        }

        [Theory]
        [InlineData("class")]
        [InlineData("record")]
        [InlineData("struct")]
        [InlineData("interface")]
        public void Extract_ShouldHaveClassNameOfGenericType_WhenProvidedDifferentClassType(string classType)
        {
            var fileContent = $@"namespace Namespace1
{{
    public {classType} Class1<T>  {{ }}
}}";
            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            var classModel = (ClassModel)classTypes[0];

            Assert.Equal("Namespace1.Class1<T>", classModel.Name);
            Assert.Equal(1, classModel.GenericParameters.Count);
            Assert.Equal("T", classModel.GenericParameters[0].Name);
            Assert.Equal("", classModel.GenericParameters[0].Modifier);
            Assert.Empty(classModel.GenericParameters[0].Constraints);
        }


        [Theory]
        [InlineData("class")]
        [InlineData("record")]
        [InlineData("struct")]
        [InlineData("interface")]
        public void Extract_ShouldHaveClassNameGenericTypeWithMultipleContainedTypes_WhenProvidedDifferentClassType(
            string classType)
        {
            var fileContent = $@"namespace Namespace1
{{
    public {classType} Class1<T,R,K> {{ }}
}}";

            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            var classModel = (ClassModel)classTypes[0];

            Assert.Equal("Namespace1.Class1<T,R,K>", classModel.Name);
            Assert.Equal(3, classModel.GenericParameters.Count);
            Assert.Equal("T", classModel.GenericParameters[0].Name);
            Assert.Equal("R", classModel.GenericParameters[1].Name);
            Assert.Equal("K", classModel.GenericParameters[2].Name);
        }

        [Fact]
        public void Extract_ShouldHaveOneBaseGenericType_WhenProvidedWitClass()
        {
            const string fileContent = @"namespace Namespace1
{
    public class Class1<T> : BaseType<T>  { }

    public interface BaseType<T>{}
}";
            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            var classModel = (ClassModel)classTypes[0];

            Assert.Equal(2, classModel.BaseTypes.Count);
            Assert.Equal("object", classModel.BaseTypes[0].Type.Name);

            Assert.Equal("Namespace1.BaseType<T>", classModel.BaseTypes[1].Type.Name);
            Assert.Equal("Namespace1.BaseType", classModel.BaseTypes[1].Type.FullType.Name);
            Assert.Equal(1, classModel.BaseTypes[1].Type.FullType.ContainedTypes.Count);
            Assert.Equal("T", classModel.BaseTypes[1].Type.FullType.ContainedTypes[0].Name);
        }

        [Fact]
        public void Extract_ShouldHaveOneBaseGenericType_WhenProvidedWitStruct()
        {
            const string fileContent = @"namespace Namespace1
{
    public struct Class1<T> : BaseType<T>  { }

    public interface BaseType<T>{}
}";
            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            var classModel = (ClassModel)classTypes[0];

            Assert.Equal(2, classModel.BaseTypes.Count);
            Assert.Equal("System.ValueType", classModel.BaseTypes[0].Type.Name);

            Assert.Equal("Namespace1.BaseType<T>", classModel.BaseTypes[1].Type.Name);
            Assert.Equal("Namespace1.BaseType", classModel.BaseTypes[1].Type.FullType.Name);
            Assert.Equal(1, classModel.BaseTypes[1].Type.FullType.ContainedTypes.Count);
            Assert.Equal("T", classModel.BaseTypes[1].Type.FullType.ContainedTypes[0].Name);
        }

        [Fact]
        public void Extract_ShouldHaveOneBaseGenericType_WhenProvidedWithRecord()
        {
            const string fileContent = @"namespace Namespace1
{
    public record Class1<T> : BaseType<T>  { }

    public interface BaseType<T>{}
}";
            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            var classModel = (ClassModel)classTypes[0];

            Assert.Equal(3, classModel.BaseTypes.Count);

            Assert.Equal("object", classModel.BaseTypes[0].Type.Name);

            Assert.Equal("Namespace1.BaseType<T>", classModel.BaseTypes[1].Type.Name);
            Assert.Equal("Namespace1.BaseType", classModel.BaseTypes[1].Type.FullType.Name);
            Assert.Equal(1, classModel.BaseTypes[1].Type.FullType.ContainedTypes.Count);
            Assert.Equal("T", classModel.BaseTypes[1].Type.FullType.ContainedTypes[0].Name);

            Assert.Equal("System.IEquatable<Namespace1.Class1<T>>", classModel.BaseTypes[2].Type.Name);
            Assert.Equal("System.IEquatable", classModel.BaseTypes[2].Type.FullType.Name);
            Assert.Equal(1, classModel.BaseTypes[2].Type.FullType.ContainedTypes.Count);
            Assert.Equal("Namespace1.Class1", classModel.BaseTypes[2].Type.FullType.ContainedTypes[0].Name);
            Assert.Equal("T", classModel.BaseTypes[2].Type.FullType.ContainedTypes[0].ContainedTypes[0].Name);
        }


        [Fact]
        public void Extract_ShouldHaveOneBaseGenericType_WhenProvidedWithInterface()
        {
            const string fileContent = @"namespace Namespace1
{
    public interface Class1<T> : BaseType<T>  { }

    public interface BaseType<T>{}
}";
            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            var classModel = (ClassModel)classTypes[0];

            Assert.Equal(1, classModel.BaseTypes.Count);
            Assert.Equal("Namespace1.BaseType<T>", classModel.BaseTypes[0].Type.Name);
            Assert.Equal("Namespace1.BaseType", classModel.BaseTypes[0].Type.FullType.Name);
            Assert.Equal(1, classModel.BaseTypes[0].Type.FullType.ContainedTypes.Count);
            Assert.Equal("T", classModel.BaseTypes[0].Type.FullType.ContainedTypes[0].Name);
        }

        [Fact]
        public void Extract_ShouldHaveMultipleBaseGenericTypes_WhenProvidedWithClass()
        {
            const string fileContent = @"namespace Namespace1
{
    public class Class1<T,R,K> : Base1<T>, Base2, Base3<R,K>, Base4<C<T,R>, K> { }
}";

            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            var classModel = (ClassModel)classTypes[0];

            Assert.Equal("Namespace1.Class1<T,R,K>", classModel.Name);

            Assert.Equal(4, classModel.BaseTypes.Count);

            var baseType1 = classModel.BaseTypes[0].Type;
            Assert.Equal("Base1<T>", baseType1.Name);
            Assert.Equal("Base1", baseType1.FullType.Name);
            Assert.Equal(1, baseType1.FullType.ContainedTypes.Count);
            Assert.Equal("T", baseType1.FullType.ContainedTypes[0].Name);
            Assert.Empty(baseType1.FullType.ContainedTypes[0].ContainedTypes);

            var baseType2 = classModel.BaseTypes[1].Type;
            Assert.Equal("Base2", baseType2.Name);
            Assert.Equal("Base2", baseType2.FullType.Name);
            Assert.Empty(baseType2.FullType.ContainedTypes);

            var baseType3 = classModel.BaseTypes[2].Type;
            Assert.Equal("Base3<R, K>", baseType3.Name);
            Assert.Equal("Base3", baseType3.FullType.Name);
            Assert.Equal(2, baseType3.FullType.ContainedTypes.Count);
            Assert.Equal("R", baseType3.FullType.ContainedTypes[0].Name);
            Assert.Empty(baseType3.FullType.ContainedTypes[0].ContainedTypes);
            Assert.Equal("K", baseType3.FullType.ContainedTypes[1].Name);
            Assert.Empty(baseType3.FullType.ContainedTypes[1].ContainedTypes);

            var baseType4 = classModel.BaseTypes[3].Type;
            Assert.Equal("Base4<C<T, R>, K>", baseType4.Name);
            Assert.Equal("Base4", baseType4.FullType.Name);
            Assert.Equal(2, baseType4.FullType.ContainedTypes.Count);
            Assert.Equal("C", baseType4.FullType.ContainedTypes[0].Name);
            Assert.Equal(2, baseType4.FullType.ContainedTypes[0].ContainedTypes.Count);
            Assert.Equal("T", baseType4.FullType.ContainedTypes[0].ContainedTypes[0].Name);
            Assert.Empty(baseType4.FullType.ContainedTypes[0].ContainedTypes[0].ContainedTypes);
            Assert.Equal("R", baseType4.FullType.ContainedTypes[0].ContainedTypes[1].Name);
            Assert.Empty(baseType4.FullType.ContainedTypes[0].ContainedTypes[1].ContainedTypes);

            Assert.Equal("K", baseType4.FullType.ContainedTypes[1].Name);
            Assert.Empty(baseType4.FullType.ContainedTypes[1].ContainedTypes);
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/ClassLevel/GenericExtraction/GenericBaseTypeWithConcreteType.txt")]
        public void Extract_ShouldHaveMultipleBaseConcreteGenericTypes_WhenProvidedWithClass(string fileContent)
        {
            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            var classModel = (ClassModel)classTypes[0];

            Assert.Equal("Namespace1.Class1", classModel.Name);

            Assert.Equal(2, classModel.BaseTypes.Count);

            var baseType1 = classModel.BaseTypes[0].Type;
            Assert.Equal("Namespace1.GenericClass<string>", baseType1.Name);
            Assert.Equal("Namespace1.GenericClass", baseType1.FullType.Name);
            Assert.Equal(1, baseType1.FullType.ContainedTypes.Count);
            Assert.Equal("string", baseType1.FullType.ContainedTypes[0].Name);
            Assert.Empty(baseType1.FullType.ContainedTypes[0].ContainedTypes);

            var baseType2 = classModel.BaseTypes[1].Type;
            Assert.Equal("Namespace1.IInterface<Namespace1.Class1, ExternClass>", baseType2.Name);
            Assert.Equal("Namespace1.IInterface", baseType2.FullType.Name);
            Assert.Equal(2, baseType2.FullType.ContainedTypes.Count);
            Assert.Equal("Namespace1.Class1", baseType2.FullType.ContainedTypes[0].Name);
            Assert.Empty(baseType2.FullType.ContainedTypes[0].ContainedTypes);
            Assert.Equal("ExternClass", baseType2.FullType.ContainedTypes[1].Name);
            Assert.Empty(baseType2.FullType.ContainedTypes[1].ContainedTypes);
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/ClassLevel/GenericExtraction/GenericInterfaceWithModifiers.txt")]
        public void Extract_ShouldHaveGenericModifiers_WhenProvidedWithGenericInterface(string fileContent)
        {
            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            var classModel = (ClassModel)classTypes[0];

            Assert.Equal("Namespace1.IInterface<out T, in TK>", classModel.Name);
            Assert.Equal(2, classModel.GenericParameters.Count);
            Assert.Equal("T", classModel.GenericParameters[0].Name);
            Assert.Equal("out", classModel.GenericParameters[0].Modifier);
            Assert.Empty(classModel.GenericParameters[0].Constraints);

            Assert.Equal("TK", classModel.GenericParameters[1].Name);
            Assert.Equal("in", classModel.GenericParameters[1].Modifier);
            Assert.Empty(classModel.GenericParameters[1].Constraints);
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/ClassLevel/GenericExtraction/GenericTypeWithPredefinedConstrains.txt")]
        public void Extract_ShouldHaveGenericTypesWithPredefinedConstrains_WhenProvidedWithClass(string fileContent)
        {
            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            var classModel1 = (ClassModel)classTypes[0];
            var classModel2 = (ClassModel)classTypes[1];

            Assert.Equal("Namespace1.Class1<T, TK, TR, TP>", classModel1.Name);
            Assert.Equal(4, classModel1.GenericParameters.Count);
            Assert.Equal("T", classModel1.GenericParameters[0].Name);
            Assert.Equal(1, classModel1.GenericParameters[0].Constraints.Count);
            Assert.Equal("struct", classModel1.GenericParameters[0].Constraints[0].Name);

            Assert.Equal("TK", classModel1.GenericParameters[1].Name);
            Assert.Equal(1, classModel1.GenericParameters[1].Constraints.Count);
            Assert.Equal("class?", classModel1.GenericParameters[1].Constraints[0].Name);

            Assert.Equal("TR", classModel1.GenericParameters[2].Name);
            Assert.Equal(1, classModel1.GenericParameters[2].Constraints.Count);
            Assert.Equal("notnull", classModel1.GenericParameters[2].Constraints[0].Name);

            Assert.Equal("TP", classModel1.GenericParameters[3].Name);
            Assert.Equal(1, classModel1.GenericParameters[3].Constraints.Count);
            Assert.Equal("Namespace1.IInterface2<T, Namespace1.IInterface2<T, TK>>",
                classModel1.GenericParameters[3].Constraints[0].Name);
            Assert.Equal("Namespace1.IInterface2", classModel1.GenericParameters[3].Constraints[0].FullType.Name);
            Assert.Equal(2, classModel1.GenericParameters[3].Constraints[0].FullType.ContainedTypes.Count);
            Assert.Equal("T", classModel1.GenericParameters[3].Constraints[0].FullType.ContainedTypes[0].Name);
            Assert.Equal("Namespace1.IInterface2",
                classModel1.GenericParameters[3].Constraints[0].FullType.ContainedTypes[1].Name);
            Assert.Equal(2,
                classModel1.GenericParameters[3].Constraints[0].FullType.ContainedTypes[1].ContainedTypes.Count);
            Assert.Equal("T",
                classModel1.GenericParameters[3].Constraints[0].FullType.ContainedTypes[1].ContainedTypes[0].Name);
            Assert.Equal("TK",
                classModel1.GenericParameters[3].Constraints[0].FullType.ContainedTypes[1].ContainedTypes[1].Name);


            Assert.Equal("Namespace1.IInterface<T, TK>", classModel2.Name);

            Assert.Equal("T", classModel2.GenericParameters[0].Name);
            Assert.Equal(1, classModel2.GenericParameters[0].Constraints.Count);
            Assert.Equal("new()", classModel2.GenericParameters[0].Constraints[0].Name);
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/ClassLevel/GenericExtraction/GenericTypeWithMultipleConstrains.txt")]
        public void Extract_ShouldHaveGenericTypesWithMultipleConstrains_WhenProvidedWithClass(string fileContent)
        {
            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            var classModel = (ClassModel)classTypes[0];

            Assert.Equal("Namespace1.Class1<T, TK, TR>", classModel.Name);
            Assert.Equal(3, classModel.GenericParameters.Count);
            Assert.Equal("T", classModel.GenericParameters[0].Name);
            Assert.Equal(2, classModel.GenericParameters[0].Constraints.Count);
            Assert.Equal("Namespace1.IInterface", classModel.GenericParameters[0].Constraints[0].Name);
            Assert.Equal("Namespace1.IInterface2<TK, TR>", classModel.GenericParameters[0].Constraints[1].Name);
            Assert.Equal("Namespace1.IInterface2", classModel.GenericParameters[0].Constraints[1].FullType.Name);
            Assert.Equal(2, classModel.GenericParameters[0].Constraints[1].FullType.ContainedTypes.Count);
            Assert.Equal("TK", classModel.GenericParameters[0].Constraints[1].FullType.ContainedTypes[0].Name);
            Assert.Equal("TR", classModel.GenericParameters[0].Constraints[1].FullType.ContainedTypes[1].Name);
        }
    }
}
