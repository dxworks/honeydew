using System.Collections.Generic;
using HoneydewCore.Logging;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewExtractors.Core.Metrics.Visitors.Constructors;
using HoneydewExtractors.Core.Metrics.Visitors.Methods;
using HoneydewExtractors.Core.Metrics.Visitors.Properties;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewExtractors.CSharp.Metrics.Extraction.Class;
using HoneydewExtractors.CSharp.Metrics.Extraction.CompilationUnit;
using HoneydewExtractors.CSharp.Metrics.Extraction.Constructor;
using HoneydewExtractors.CSharp.Metrics.Extraction.Delegate;
using HoneydewExtractors.CSharp.Metrics.Extraction.Method;
using HoneydewExtractors.CSharp.Metrics.Extraction.Property;
using HoneydewModels.CSharp;
using Moq;
using Xunit;

namespace HoneydewExtractorsTests.CSharp.Metrics.Extraction.ClassLevel
{
    public class CSharpCyclomaticComplexityTests
    {
        private readonly CSharpFactExtractor _factExtractor;
        private readonly Mock<ILogger> _loggerMock = new();

        public CSharpCyclomaticComplexityTests()
        {
            var compositeVisitor = new CompositeVisitor();

            compositeVisitor.Add(new ClassSetterCompilationUnitVisitor(new List<ICSharpClassVisitor>
            {
                new BaseInfoClassVisitor(),
                new PropertySetterClassVisitor(new List<ICSharpPropertyVisitor>
                {
                    new PropertyInfoVisitor()
                }),
                new MethodSetterClassVisitor(new List<ICSharpMethodVisitor>
                {
                    new MethodInfoVisitor()
                }),
                new ConstructorSetterClassVisitor(new List<ICSharpConstructorVisitor>
                {
                    new ConstructorInfoVisitor()
                })
            }));
            compositeVisitor.Add(new DelegateSetterCompilationUnitVisitor(new List<ICSharpDelegateVisitor>
            {
                new BaseInfoDelegateVisitor()
            }));

            compositeVisitor.Accept(new LoggerSetterVisitor(_loggerMock.Object));

            _factExtractor = new CSharpFactExtractor(new CSharpSyntacticModelCreator(),
                new CSharpSemanticModelCreator(new CSharpCompilationMaker()), compositeVisitor);
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/ClassLevel/CSharpCyclomaticComplexity/ClassWithDelegateFieldConstructorAndMethod.txt")]
        public void Extract_ShouldHave1CyclomaticComplexity_WhenGivenClassWithMethodsAndPropertiesAndDelegate(
            string fileContent)
        {
            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(2, classTypes.Count);

            var classModel = (ClassModel)classTypes[0];
            Assert.Equal(1, classModel.Constructors[0].CyclomaticComplexity);
            Assert.Equal(1, classModel.Methods[0].CyclomaticComplexity);
            Assert.Equal(2, classModel.Properties[0].CyclomaticComplexity);
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/ClassLevel/CSharpCyclomaticComplexity/ClassWithDelegateFieldConstructorAndMethodsContainingWhile.txt")]
        public void
            Extract_ShouldCountCyclomaticComplexityFromWhiles_WhenGivenClassWithMethodsAndPropertiesAndDelegate(
                string fileContent)
        {
            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(2, classTypes.Count);

            var classModel = (ClassModel)classTypes[0];
            Assert.Equal(2, classModel.Constructors[0].CyclomaticComplexity);
            Assert.Equal(4, classModel.Methods[0].CyclomaticComplexity);
            Assert.Equal(4, classModel.Properties[0].CyclomaticComplexity);
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/ClassLevel/CSharpCyclomaticComplexity/ClassWithDelegateFieldConstructorAndMethodsContainingIf.txt")]
        public void
            Extract_ShouldCountCyclomaticComplexityFromIfs_WhenGivenClassWithMethodsAndProperties(string fileContent)
        {
            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes.Count);

            var classModel = (ClassModel)classTypes[0];
            Assert.Equal(3, classModel.Constructors[0].CyclomaticComplexity);
            Assert.Equal(4, classModel.Methods[0].CyclomaticComplexity);
            Assert.Equal(4, classModel.Properties[0].CyclomaticComplexity);
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/ClassLevel/CSharpCyclomaticComplexity/ClassWithMethodContainingFor.txt")]
        public void
            Extract_ShouldCountCyclomaticComplexityFromFors_WhenGivenClassWithMethods(string fileContent)
        {
            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes.Count);

            Assert.Equal(5, ((ClassModel)classTypes[0]).Methods[0].CyclomaticComplexity);
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/ClassLevel/CSharpCyclomaticComplexity/ClassWithMethodContainingUnaryExpressionsInConditions.txt")]
        public void
            Extract_ShouldCountCyclomaticComplexityFromUnaryExpression_WhenGivenClassWithMethods(string fileContent)
        {
            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes.Count);

            Assert.Equal(7, ((ClassModel)classTypes[0]).Methods[0].CyclomaticComplexity);
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/ClassLevel/CSharpCyclomaticComplexity/ClassWithComplexBinaryExpression.txt")]
        public void
            Extract_ShouldCountCyclomaticComplexityFromComplexBinaryExpression_WhenGivenClassWithMethods(
                string fileContent)
        {
            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes.Count);

            Assert.Equal(7, ((ClassModel)classTypes[0]).Methods[0].CyclomaticComplexity);
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/ClassLevel/CSharpCyclomaticComplexity/ClassWithMethodContainingBinaryExpressionWithIsAndOrNot.txt")]
        public void
            Extract_ShouldCountCyclomaticComplexityFromComplexBinaryExpressionWithIsAndOr_WhenGivenClassWithMethods(
                string fileContent)
        {
            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes.Count);

            Assert.Equal(6, ((ClassModel)classTypes[0]).Methods[0].CyclomaticComplexity);
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/ClassLevel/CSharpCyclomaticComplexity/ClassWithMethodContainingDoWhile.txt")]
        public void
            Extract_ShouldCountCyclomaticComplexityFromDoWhile_WhenGivenClassWithMethods(string fileContent)
        {
            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes.Count);

            Assert.Equal(2, ((ClassModel)classTypes[0]).Methods[0].CyclomaticComplexity);
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/ClassLevel/CSharpCyclomaticComplexity/ClassWithMethodContainingForeach.txt")]
        public void
            Extract_ShouldCountCyclomaticComplexityFromForeach_WhenGivenClassWithMethods(string fileContent)
        {
            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes.Count);

            Assert.Equal(2, ((ClassModel)classTypes[0]).Methods[0].CyclomaticComplexity);
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/ClassLevel/CSharpCyclomaticComplexity/ClassWithMethodContainingSwitch.txt")]
        public void
            Extract_ShouldCountCyclomaticComplexityFromSwitch_WhenGivenClassWithMethods(string fileContent)
        {
            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes.Count);

            Assert.Equal(7, ((ClassModel)classTypes[0]).Methods[0].CyclomaticComplexity);
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/ClassLevel/CSharpCyclomaticComplexity/ClassWithMethodContainingSwitchWithOperators.txt")]
        public void
            Extract_ShouldCountCyclomaticComplexityFromPatternSwitchWithOperators_WhenGivenClassWithMethods(
                string fileContent)
        {
            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes.Count);

            Assert.Equal(6, ((ClassModel)classTypes[0]).Methods[0].CyclomaticComplexity);
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/ClassLevel/CSharpCyclomaticComplexity/ClassWithMethodThatReturnsAStringWithSwitch.txt")]
        public void
            Extract_ShouldCountCyclomaticComplexityFromReturnSwitchWithStrings_WhenGivenClassWithMethods(
                string fileContent)
        {
            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes.Count);

            Assert.Equal(1, ((ClassModel)classTypes[0]).Methods[0].CyclomaticComplexity);
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/ClassLevel/CSharpCyclomaticComplexity/ClassWithMethodContainingPatternSwitch.txt")]
        public void
            Extract_ShouldCountCyclomaticComplexityFromPatternSwitchWithClassHierarchy_WhenGivenClassWithMethods(
                string fileContent)
        {
            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(5, classTypes.Count);

            Assert.Equal(5, ((ClassModel)classTypes[0]).Methods[0].CyclomaticComplexity);
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/ClassLevel/CSharpCyclomaticComplexity/ClassWithMethodContainingConditionalOperators.txt")]
        public void
            Extract_ShouldCountCyclomaticComplexityForConditionalOperators_WhenGivenClassWithMethods(string fileContent)
        {
            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes.Count);

            Assert.Equal(5, ((ClassModel)classTypes[0]).Methods[0].CyclomaticComplexity);
        }
    }
}
