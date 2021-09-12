using System.Collections.Generic;
using HoneydewCore.Logging;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewExtractors.Core.Metrics.Visitors.Parameters;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewExtractors.CSharp.Metrics.Extraction.Common;
using HoneydewExtractors.CSharp.Metrics.Extraction.CompilationUnit;
using HoneydewExtractors.CSharp.Metrics.Extraction.Delegate;
using HoneydewExtractors.CSharp.Metrics.Extraction.Parameter;
using HoneydewModels.CSharp;
using Moq;
using Xunit;

namespace HoneydewExtractorsTests.CSharp.Metrics.Extraction.Delegate
{
    public class CSharpGenericDelegateTests
    {
        private readonly CSharpFactExtractor _factExtractor;
        private readonly Mock<ILogger> _loggerMock = new();

        public CSharpGenericDelegateTests()
        {
            var compositeVisitor = new CompositeVisitor();

            compositeVisitor.Add(new DelegateSetterCompilationUnitVisitor(new List<ICSharpDelegateVisitor>
            {
                new BaseInfoDelegateVisitor(),
                new GenericParameterSetterVisitor(new List<IGenericParameterVisitor>
                {
                    new GenericParameterInfoVisitor()
                })
            }));

            compositeVisitor.Accept(new LoggerSetterVisitor(_loggerMock.Object));

            _factExtractor = new CSharpFactExtractor(new CSharpSyntacticModelCreator(),
                new CSharpSemanticModelCreator(new CSharpCompilationMaker(_loggerMock.Object)), compositeVisitor);
        }

        [Fact]
        public void Extract_ShouldHaveDelegateNameOfGenericType_WhenProvidedWithGenericDelegate()
        {
            const string fileContent = @"namespace Namespace1
{
    public delegate T Delegate1<T>(T item);
}";
            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            var classModel = (DelegateModel)classTypes[0];

            Assert.Equal("Namespace1.Delegate1<T>", classModel.Name);
            Assert.Equal(1, classModel.GenericParameters.Count);
            Assert.Equal("T", classModel.GenericParameters[0].Name);
            Assert.Equal("", classModel.GenericParameters[0].Modifier);
            Assert.Empty(classModel.GenericParameters[0].Constraints);
        }

        [Fact]
        public void
            Extract_ShouldHaveDelegateNameGenericTypeWithMultipleContainedTypes_WhenProvidedWithGenericDelegate()
        {
            const string fileContent = @"namespace Namespace1
{
    public delegate T Delegate1<T,R,K>(R r, K k1, K k2);
}";

            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            var classModel = (DelegateModel)classTypes[0];

            Assert.Equal("Namespace1.Delegate1<T,R,K>", classModel.Name);
            Assert.Equal(3, classModel.GenericParameters.Count);
            Assert.Equal("T", classModel.GenericParameters[0].Name);
            Assert.Equal("", classModel.GenericParameters[0].Modifier);
            Assert.Equal("R", classModel.GenericParameters[1].Name);
            Assert.Equal("", classModel.GenericParameters[1].Modifier);
            Assert.Equal("K", classModel.GenericParameters[2].Name);
            Assert.Equal("", classModel.GenericParameters[2].Modifier);
        }

        [Fact]
        public void Extract_ShouldHaveGenericModifiers_WhenProvidedWithGenericDelegate()
        {
            const string fileContent = @"namespace Namespace1
{
    public delegate T Delegate1<out T, in TR, in TK>(TR r, TK tk, TK tk2);
}";

            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            var classModel = (DelegateModel)classTypes[0];

            Assert.Equal("Namespace1.Delegate1<out T, in TR, in TK>", classModel.Name);
            Assert.Equal(3, classModel.GenericParameters.Count);
            Assert.Equal("T", classModel.GenericParameters[0].Name);
            Assert.Equal("out", classModel.GenericParameters[0].Modifier);
            Assert.Equal("TR", classModel.GenericParameters[1].Name);
            Assert.Equal("in", classModel.GenericParameters[1].Modifier);
            Assert.Equal("TK", classModel.GenericParameters[2].Name);
            Assert.Equal("in", classModel.GenericParameters[2].Modifier);
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/Delegate/GenericExtraction/GenericTypeWithPredefinedConstrains.txt")]
        public void Extract_ShouldHaveGenericTypesWithPredefinedConstrains_WhenProvidedWithDelegate(string fileContent)
        {
            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            var classModel1 = (DelegateModel)classTypes[0];

            Assert.Equal("Namespace1.Delegate1<out T, in TK, in TR, in TP>", classModel1.Name);
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
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/Delegate/GenericExtraction/GenericTypeWithMultipleConstrains.txt")]
        public void Extract_ShouldHaveGenericTypesWithMultipleConstrains_WhenProvidedWithDelegate(string fileContent)
        {
            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            var classModel = (DelegateModel)classTypes[0];

            Assert.Equal("Namespace1.Delegate1<out T, in TR, in TK>", classModel.Name);
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
