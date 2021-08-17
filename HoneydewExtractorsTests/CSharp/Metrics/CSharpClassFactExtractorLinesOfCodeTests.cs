using HoneydewExtractors.CSharp.Metrics;
using Xunit;

namespace HoneydewExtractorsTests.CSharp.Metrics
{
    public class CSharpClassFactExtractorLinesOfCodeTests
    {
        private readonly CSharpFactExtractor _sut;

        public CSharpClassFactExtractorLinesOfCodeTests()
        {
            _sut = new CSharpFactExtractor();
        }

        [Theory]
        [FileData("TestData/CSharp/Metrics/CSharpLinesOfCode/ClassWithCommentsWithPropertyAndMethod.txt")]
        public void Extract_ShouldHaveLinesOfCode_WhenProvidedWithClassWithMethodsAndProperties(string fileContent)
        {
            var classModels = _sut.Extract(fileContent);

            var classModel = classModels[0];
            Assert.Equal(21, classModel.Loc.SourceLines);
            Assert.Equal(7, classModel.Loc.EmptyLines);
            Assert.Equal(8, classModel.Loc.CommentedLines);

            Assert.Equal(5, classModel.Methods[0].Loc.SourceLines);
            Assert.Equal(2, classModel.Methods[0].Loc.CommentedLines);
            Assert.Equal(2, classModel.Methods[0].Loc.EmptyLines);

            Assert.Equal(7, classModel.Properties[0].Loc.SourceLines);
            Assert.Equal(1, classModel.Properties[0].Loc.CommentedLines);
            Assert.Equal(1, classModel.Properties[0].Loc.EmptyLines);
        }

        [Theory]
        [FileData("TestData/CSharp/Metrics/CSharpLinesOfCode/ClassWithPropertyAndMethodAndDelegateWithComments.txt")]
        public void Extract_ShouldHaveLinesOfCode_WhenProvidedWithClassAndDelegate(string fileContent)
        {
            var classModels = _sut.Extract(fileContent);

            var classModel = classModels[0];
            Assert.Equal(16, classModel.Loc.SourceLines);
            Assert.Equal(6, classModel.Loc.EmptyLines);
            Assert.Equal(5, classModel.Loc.CommentedLines);

            Assert.Equal(5, classModel.Methods[0].Loc.SourceLines);
            Assert.Equal(2, classModel.Methods[0].Loc.CommentedLines);
            Assert.Equal(2, classModel.Methods[0].Loc.EmptyLines);

            Assert.Equal(7, classModel.Properties[0].Loc.SourceLines);
            Assert.Equal(1, classModel.Properties[0].Loc.CommentedLines);
            Assert.Equal(1, classModel.Properties[0].Loc.EmptyLines);

            var delegateModel = classModels[1];
            Assert.Equal(1, delegateModel.Loc.SourceLines);
            Assert.Equal(0, delegateModel.Loc.EmptyLines);
            Assert.Equal(0, delegateModel.Loc.CommentedLines);
        }
    }
}
