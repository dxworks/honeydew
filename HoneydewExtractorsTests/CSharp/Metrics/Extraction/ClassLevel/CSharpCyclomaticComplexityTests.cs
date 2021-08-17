using HoneydewExtractors.CSharp.Metrics;
using HoneydewExtractors.CSharp.Metrics.Extraction.ClassLevel;
using Xunit;

namespace HoneydewExtractorsTests.CSharp.Metrics.Extraction.ClassLevel
{
    public class CSharpCyclomaticComplexityTests
    {
        private readonly CSharpFactExtractor _factExtractor;

        public CSharpCyclomaticComplexityTests()
        {
            _factExtractor = new CSharpFactExtractor();
            _factExtractor.AddMetric<CSharpFieldsInfoMetric>();
            _factExtractor.AddMetric<CSharpMethodInfoMetric>();
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/ClassLevel/CSharpCyclomaticComplexity/ClassWithDelegateFieldConstructorAndMethod.txt")]
        public void Extract_ShouldHave1CyclomaticComplexity_WhenGivenClassWithMethodsAndPropertiesAndDelegate(
            string fileContent)
        {
            var classModels = _factExtractor.Extract(fileContent);

            Assert.Equal(2, classModels.Count);

            Assert.Equal(1, classModels[0].Constructors[0].CyclomaticComplexity);
            Assert.Equal(1, classModels[0].Methods[0].CyclomaticComplexity);
            Assert.Equal(1, classModels[0].Properties[0].CyclomaticComplexity);

            Assert.Equal(0, classModels[1].Methods[0].CyclomaticComplexity);
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/ClassLevel/CSharpCyclomaticComplexity/ClassWithDelegateFieldConstructorAndMethodsContainingWhile.txt")]
        public void
            Extract_ShouldCountCyclomaticComplexityFromWhiles_WhenGivenClassWithMethodsAndPropertiesAndDelegate(
                string fileContent)
        {
            var classModels = _factExtractor.Extract(fileContent);

            Assert.Equal(2, classModels.Count);

            Assert.Equal(2, classModels[0].Constructors[0].CyclomaticComplexity);
            Assert.Equal(4, classModels[0].Methods[0].CyclomaticComplexity);
            Assert.Equal(3, classModels[0].Properties[0].CyclomaticComplexity);

            Assert.Equal(0, classModels[1].Methods[0].CyclomaticComplexity);
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/ClassLevel/CSharpCyclomaticComplexity/ClassWithDelegateFieldConstructorAndMethodsContainingIf.txt")]
        public void
            Extract_ShouldCountCyclomaticComplexityFromIfs_WhenGivenClassWithMethodsAndProperties(string fileContent)
        {
            var classModels = _factExtractor.Extract(fileContent);

            Assert.Equal(1, classModels.Count);

            Assert.Equal(3, classModels[0].Constructors[0].CyclomaticComplexity);
            Assert.Equal(4, classModels[0].Methods[0].CyclomaticComplexity);
            Assert.Equal(3, classModels[0].Properties[0].CyclomaticComplexity);
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/ClassLevel/CSharpCyclomaticComplexity/ClassWithMethodContainingFor.txt")]
        public void
            Extract_ShouldCountCyclomaticComplexityFromFors_WhenGivenClassWithMethods(string fileContent)
        {
            var classModels = _factExtractor.Extract(fileContent);

            Assert.Equal(1, classModels.Count);

            Assert.Equal(5, classModels[0].Methods[0].CyclomaticComplexity);
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/ClassLevel/CSharpCyclomaticComplexity/ClassWithMethodContainingUnaryExpressionsInConditions.txt")]
        public void
            Extract_ShouldCountCyclomaticComplexityFromUnaryExpression_WhenGivenClassWithMethods(string fileContent)
        {
            var classModels = _factExtractor.Extract(fileContent);

            Assert.Equal(1, classModels.Count);

            Assert.Equal(7, classModels[0].Methods[0].CyclomaticComplexity);
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/ClassLevel/CSharpCyclomaticComplexity/ClassWithComplexBinaryExpression.txt")]
        public void
            Extract_ShouldCountCyclomaticComplexityFromComplexBinaryExpression_WhenGivenClassWithMethods(
                string fileContent)
        {
            var classModels = _factExtractor.Extract(fileContent);

            Assert.Equal(1, classModels.Count);

            Assert.Equal(7, classModels[0].Methods[0].CyclomaticComplexity);
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/ClassLevel/CSharpCyclomaticComplexity/ClassWithMethodContainingBinaryExpressionWithIsAndOrNot.txt")]
        public void
            Extract_ShouldCountCyclomaticComplexityFromComplexBinaryExpressionWithIsAndOr_WhenGivenClassWithMethods(
                string fileContent)
        {
            var classModels = _factExtractor.Extract(fileContent);

            Assert.Equal(1, classModels.Count);

            Assert.Equal(6, classModels[0].Methods[0].CyclomaticComplexity);
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/ClassLevel/CSharpCyclomaticComplexity/ClassWithMethodContainingDoWhile.txt")]
        public void
            Extract_ShouldCountCyclomaticComplexityFromDoWhile_WhenGivenClassWithMethods(string fileContent)
        {
            var classModels = _factExtractor.Extract(fileContent);

            Assert.Equal(1, classModels.Count);

            Assert.Equal(2, classModels[0].Methods[0].CyclomaticComplexity);
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/ClassLevel/CSharpCyclomaticComplexity/ClassWithMethodContainingForeach.txt")]
        public void
            Extract_ShouldCountCyclomaticComplexityFromForeach_WhenGivenClassWithMethods(string fileContent)
        {
            var classModels = _factExtractor.Extract(fileContent);

            Assert.Equal(1, classModels.Count);

            Assert.Equal(2, classModels[0].Methods[0].CyclomaticComplexity);
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/ClassLevel/CSharpCyclomaticComplexity/ClassWithMethodContainingSwitch.txt")]
        public void
            Extract_ShouldCountCyclomaticComplexityFromSwitch_WhenGivenClassWithMethods(string fileContent)
        {
            var classModels = _factExtractor.Extract(fileContent);

            Assert.Equal(1, classModels.Count);

            Assert.Equal(7, classModels[0].Methods[0].CyclomaticComplexity);
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/ClassLevel/CSharpCyclomaticComplexity/ClassWithMethodContainingSwitchWithOperators.txt")]
        public void
            Extract_ShouldCountCyclomaticComplexityFromPatternSwitchWithOperators_WhenGivenClassWithMethods(
                string fileContent)
        {
            var classModels = _factExtractor.Extract(fileContent);

            Assert.Equal(1, classModels.Count);

            Assert.Equal(6, classModels[0].Methods[0].CyclomaticComplexity);
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/ClassLevel/CSharpCyclomaticComplexity/ClassWithMethodThatReturnsAStringWithSwitch.txt")]
        public void
            Extract_ShouldCountCyclomaticComplexityFromReturnSwitchWithStrings_WhenGivenClassWithMethods(
                string fileContent)
        {
            var classModels = _factExtractor.Extract(fileContent);

            Assert.Equal(1, classModels.Count);

            Assert.Equal(1, classModels[0].Methods[0].CyclomaticComplexity);
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/ClassLevel/CSharpCyclomaticComplexity/ClassWithMethodContainingPatternSwitch.txt")]
        public void
            Extract_ShouldCountCyclomaticComplexityFromPatternSwitchWithClassHierarchy_WhenGivenClassWithMethods(
                string fileContent)
        {
            var classModels = _factExtractor.Extract(fileContent);

            Assert.Equal(5, classModels.Count);

            Assert.Equal(5, classModels[0].Methods[0].CyclomaticComplexity);
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/ClassLevel/CSharpCyclomaticComplexity/ClassWithMethodContainingConditionalOperators.txt")]
        public void
            Extract_ShouldCountCyclomaticComplexityForConditionalOperators_WhenGivenClassWithMethods(string fileContent)
        {
            var classModels = _factExtractor.Extract(fileContent);

            Assert.Equal(1, classModels.Count);

            Assert.Equal(5, classModels[0].Methods[0].CyclomaticComplexity);
        }
    }
}
