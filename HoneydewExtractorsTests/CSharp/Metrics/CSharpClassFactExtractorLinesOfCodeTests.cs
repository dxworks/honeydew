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

        [Fact]
        public void Extract_ShouldHaveLinesOfCode_WhenProvidedWithClassWithMethodsAndProperties()
        {
            const string fileContent = @"using System;
using HoneydewCore.Extractors;

// here is the namespace
namespace TopLevel
{
    // some code
    public class Foo
    {
        // this is a field
        private int _f;

        public int Field
         {
            get
            {
                // should return calculated

                return Calc(_f);
            }
         }


        // this method calculates
        public int Calc(int a)
        {
 
            // calculate double

            var d = a*2;
            return d;
           // return a*2;
        }        
    }                                    
}";
            var classModels = _sut.Extract(fileContent);

            var classModel = classModels[0];
            Assert.Equal(21, classModel.Loc.SourceLines);
            Assert.Equal(7, classModel.Loc.EmptyLines);
            Assert.Equal(7, classModel.Loc.CommentedLines);

            Assert.Equal(5, classModel.Methods[0].Loc.SourceLines);
            Assert.Equal(2, classModel.Methods[0].Loc.CommentedLines);
            Assert.Equal(2, classModel.Methods[0].Loc.EmptyLines);

            Assert.Equal(7, classModel.Properties[0].Loc.SourceLines);
            Assert.Equal(1, classModel.Properties[0].Loc.CommentedLines);
            Assert.Equal(1, classModel.Properties[0].Loc.EmptyLines);
        }

        [Fact]
        public void Extract_ShouldHaveLinesOfCode_WhenProvidedWithClassAndDelegate()
        {
            const string fileContent = @"using System;
using HoneydewCore.Extractors;

// here is the namespace
namespace TopLevel
{
    // some code
    public class Foo
    {
        // this is a field
        private int _f;

        public int Field
         {
            get
            {
                // should return calculated

                return Calc(_f);
            }
         }


        // this method calculates
        public int Calc(int a)
        {
 
            // calculate double

            var d = a*2;
            return d;
           // return a*2;
        }        
    }

    public delegate void A();                                        
}";
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
