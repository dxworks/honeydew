using HoneydewExtractors.Core.Metrics.Extraction;
using HoneydewExtractors.CSharp.Metrics.Extraction;
using Xunit;

namespace HoneydewExtractorsTests.CSharp.Metrics.Extraction
{
    public class LinesOfCodeCounterTests
    {
        private readonly ILinesOfCodeCounter _sut;

        public LinesOfCodeCounterTests()
        {
            _sut = new CSharpLinesOfCodeCounter();
        }

        [Fact]
        public void Count_ShouldReturnSourceLinesCount_WhenProvidedWithContentWithOnlySourceLines()
        {
            const string fileContent = @"using System;
namespace TopLevel
{
    public class File
    {
        private int _f;
        public int Field
        {
            get { return Calc(_f); }
        }        
        public int Calc(int a)
        {
           return a*2;
        }        
    }                                        
}";
            var linesOfCode = _sut.Count(fileContent);

            Assert.Equal(16, linesOfCode.SourceLines);
            Assert.Equal(0, linesOfCode.EmptyLines);
            Assert.Equal(0, linesOfCode.CommentedLines);
        }

        [Fact]
        public void Count_ShouldReturnCommentLinesCount_WhenProvidedWithContentWithOnlySingleLineComments()
        {
            const string fileContent = @"//using System;
//namespace TopLevel
//{
//    public class File
//    {
//        private int _f;
//        public int Field
//        {
//            get { return Calc(_f); }
//        }        
//        public int Calc(int a)
//        {
//           return a*2;
//        }       
//    }                                        
//}";
            var linesOfCode = _sut.Count(fileContent);

            Assert.Equal(0, linesOfCode.SourceLines);
            Assert.Equal(0, linesOfCode.EmptyLines);
            Assert.Equal(16, linesOfCode.CommentedLines);
        }

        [Fact]
        public void Count_ShouldReturnCommentLinesCount_WhenProvidedWithContentWithOnlyMultiLineComments()
        {
            const string fileContent = @"/*using System;
namespace TopLevel
{
    public class File
    {*/
/*        private int _f;
        public int Field
        {
            get { return Calc(_f); }
        }
*/
/*        
        public int Calc(int a)
        {
           return a*2;
        }       
    }                                        
}*/";
            var linesOfCode = _sut.Count(fileContent);

            Assert.Equal(0, linesOfCode.SourceLines);
            Assert.Equal(0, linesOfCode.EmptyLines);
            Assert.Equal(18, linesOfCode.CommentedLines);
        }

        [Fact]
        public void Count_ShouldReturnWhitespaceLinesCount_WhenProvidedWithContentWithSourceAndWhitespaceLines()
        {
            const string fileContent = @"using System;

namespace TopLevel
{

    public class File
    {

        private int _f;

        public int Field
        {
            get {
    
             return Calc(_f); 
            }
        }
      
        public int Calc(int a)
        {
           return a*2;
        }       

    }                                        
}";
            var linesOfCode = _sut.Count(fileContent);

            Assert.Equal(18, linesOfCode.SourceLines);
            Assert.Equal(7, linesOfCode.EmptyLines);
        }
        
        [Fact]
        public void Count_ShouldReturnLinesOfCodeCount_WhenProvidedWithContentWithSourceAndCommentedAndWhitespaceLines()
        {
            const string fileContent = @"using System;
using HoneydewCore.Extractors;

// here is the namespace
namespace TopLevel
{
    // some code
    public class File
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
        //


        /* this method calculates */
        public int Calc(int a)
        {
 
            // calculate double

            var d = a*2;
            return d;
           // return a*2;
        }        
    }                                        
}";
            var linesOfCode = _sut.Count(fileContent);

            Assert.Equal(21, linesOfCode.SourceLines);
            Assert.Equal(7, linesOfCode.EmptyLines);
            Assert.Equal(8, linesOfCode.CommentedLines);
        }
        
        [Fact]
        public void Count_ShouldReturnLinesOfCodeCount_WhenProvidedWithContentWithSourceAndCommentedOnTheSameLine()
        {
            const string fileContent = @"namespace TopLevel {
    public class File3 {        
        public int Calc(int a) {
             int x = 5;    /* aaa             

             ggg
             */
            /* cc*/ int y = 55; // comm
        }        
    }                                        
}";
            var linesOfCode = _sut.Count(fileContent);

            Assert.Equal(8, linesOfCode.SourceLines);
            Assert.Equal(0, linesOfCode.EmptyLines);
            Assert.Equal(3, linesOfCode.CommentedLines);
        }
        
        [Fact]
        public void Count_ShouldReturnLinesOfCodeCount_WhenProvidedWithContentWithSourceAndImbricatedMultilineComments()
        {
            const string fileContent = @"namespace TopLevel {
    public class File3 {        
        /*
        public int Calc(int a) {
             int x = 5;    /* aaa             

             ggg
             */
           /* cc*/  int y = 55; // comm
        } /*      */ /* 
    }       */                                 
}";
            var linesOfCode = _sut.Count(fileContent);

            Assert.Equal(5, linesOfCode.SourceLines);
            Assert.Equal(0, linesOfCode.EmptyLines);
            Assert.Equal(7, linesOfCode.CommentedLines);
        }
        
        [Fact]
        public void Count_ShouldReturnLinesOfCodeCount_WhenProvidedWithContentWithSourceAndMultilineCommentsOnTheSameLine()
        {
            const string fileContent = @"namespace TopLevel {
    public class File3 {        
        public int Calc(int a) {
            /* */ /* */ // /* comment
            /* */ /* */ /* */
             /* xx*/ int /* aa*/ x = /*aa */ 5;    /* aaa */
        }
    }                                 
}";
            var linesOfCode = _sut.Count(fileContent);

            Assert.Equal(7, linesOfCode.SourceLines);
            Assert.Equal(0, linesOfCode.EmptyLines);
            Assert.Equal(2, linesOfCode.CommentedLines);
        }
        [Fact]
        public void Count_ShouldReturnLinesOfCodeCount_WhenProvidedWithContentWithSingleLineAndMultiLineComments()
        {
            const string fileContent = @"namespace TopLevel {
    public class File3 {        
        public int Calc(int a) {

            /* */ /* */ /* comment

            // */ int x=5; // /* comment
            // /*                                   
        }
    }                                 
}";
            var linesOfCode = _sut.Count(fileContent);

            Assert.Equal(7, linesOfCode.SourceLines);
            Assert.Equal(1, linesOfCode.EmptyLines);
            Assert.Equal(3, linesOfCode.CommentedLines);
        }
    }
}
