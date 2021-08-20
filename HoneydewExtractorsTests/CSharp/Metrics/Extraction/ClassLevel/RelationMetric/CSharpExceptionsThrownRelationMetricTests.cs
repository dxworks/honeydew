using System.Collections.Generic;
using HoneydewCore.ModelRepresentations;
using HoneydewExtractors.Core.Metrics.Extraction.Class;
using HoneydewExtractors.Core.Metrics.Extraction.Class.Relations;
using HoneydewExtractors.Core.Metrics.Extraction.CompilationUnit;
using HoneydewExtractors.Core.Metrics.Extraction.ModelCreators;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewExtractors.CSharp.Metrics;
using Xunit;

namespace HoneydewExtractorsTests.CSharp.Metrics.Extraction.ClassLevel.RelationMetric
{
    public class CSharpExceptionsThrownRelationMetricTests
    {
        private readonly CSharpFactExtractor _factExtractor;
        private readonly ExceptionsThrownRelationVisitor _sut;

        public CSharpExceptionsThrownRelationMetricTests()
        {
            _sut = new ExceptionsThrownRelationVisitor(new RelationMetricHolder());

            var visitorList = new VisitorList();
            visitorList.Add(new ClassSetterCompilationUnitVisitor(new CSharpClassModelCreator(
                new List<ICSharpClassVisitor>
                {
                    new BaseInfoClassVisitor(),
                    _sut
                })));
            _factExtractor = new CSharpFactExtractor(new CSharpSyntacticModelCreator(),
                new CSharpSemanticModelCreator(new CSharpCompilationMaker()), visitorList);
        }

        [Fact]
        public void PrettyPrint_ShouldReturnReturnValueDependency()
        {
            Assert.Equal("Exceptions Thrown Dependency", _sut.PrettyPrint());
        }

        [Fact]
        public void Extract_ShouldHaveNoExceptionsThrown_WhenProvidedWithClassThatDoesntThrowExceptions()
        {
            const string fileContent = @"using System;
                                     namespace App
                                     {                
                                         class C {}
                       
                                         class MyClass
                                         {                                           
                                            private C _c = new C();
                                            private C _c2 = new();

                                            public C MyC {get;set;} = new C();
                                            public C ComputedC => new();
                                            public C MyC2
                                            {
                                                get { return new C(); }
                                            }

                                            public MyClass() {
                                                new C();
                                                C c = new();
                                            }

                                            public C Method() {
                                                var c = new C();
                                                return c;
                                            }
                                         }
                                     }";

            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes[1].Metrics.Count);
            Assert.Equal("HoneydewExtractors.Core.Metrics.Extraction.Class.Relations.ExceptionsThrownRelationVisitor",
                classTypes[1].Metrics[0].ExtractorName);
            Assert.Equal(
                "System.Collections.Generic.Dictionary`2[HoneydewCore.ModelRepresentations.IRelationMetric,System.Collections.Generic.IDictionary`2[System.String,System.Int32]]",
                classTypes[1].Metrics[0].ValueType);
            Assert.Empty((Dictionary<IRelationMetric, IDictionary<string, int>>)classTypes[1].Metrics[0].Value);
        }

        [Fact]
        public void Extract_ShouldHaveSystemExceptionsThrown_WhenProvidedWithClassThatThrowsSystemExceptions()
        {
            const string fileContent = @"using System;
namespace Throw2
{
    public class NumberGenerator
    {
        int[] numbers = {2, 4, 6, 8, 10, 12, 14, 16, 18, 20};

        public int GetNumber(string indexString)
        {
            if (string.IsNullOrEmpty(indexString))
            {
                throw new ArgumentNullException();
            }

            var index = int.Parse(indexString);
            
            if (index == 2)
            {
                throw new ArgumentException();
            }
            
            if (index < 0 || index >= numbers.Length)
            {
                throw new IndexOutOfRangeException();
            }

            return numbers[index];
        }
    }
}";

            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes[0].Metrics.Count);
            Assert.Equal("HoneydewExtractors.Core.Metrics.Extraction.Class.Relations.ExceptionsThrownRelationVisitor",
                classTypes[0].Metrics[0].ExtractorName);
            Assert.Equal(
                "System.Collections.Generic.Dictionary`2[HoneydewCore.ModelRepresentations.IRelationMetric,System.Collections.Generic.IDictionary`2[System.String,System.Int32]]",
                classTypes[0].Metrics[0].ValueType);

            var dependencies =
                ((Dictionary<IRelationMetric, IDictionary<string, int>>)classTypes[0].Metrics[0].Value)[_sut];

            Assert.Equal(3, dependencies.Count);
            Assert.Equal(1, dependencies["System.ArgumentNullException"]);
            Assert.Equal(1, dependencies["System.ArgumentException"]);
            Assert.Equal(1, dependencies["System.IndexOutOfRangeException"]);
        }


        [Fact]
        public void Extract_ShouldHaveExceptionsThrown_WhenProvidedWithClassThatThrowsCustomExceptions()
        {
            const string fileContent = @"using System;
namespace Throwing
{
    class MyArgumentNullException : Exception { }
    
    class MyArgumentException : Exception { }
    
    class MyIndexOutOfRangeException : Exception { }
    
    
    public class NumberGenerator
    {
        int[] numbers = {2, 4, 6, 8, 10, 12, 14, 16, 18, 20};

        public int GetNumber(string indexString)
        {
            if (string.IsNullOrEmpty(indexString))
            {
                throw new MyArgumentNullException();
            }

            var index = int.Parse(indexString);
            
            if (index == 2)
            {
                throw new MyArgumentException();
            }
            
            if (index < 0 || index >= numbers.Length)
            {
                throw new MyIndexOutOfRangeException();
            }

            return numbers[index];
        }
    }
}";

            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes[3].Metrics.Count);
            Assert.Equal("HoneydewExtractors.Core.Metrics.Extraction.Class.Relations.ExceptionsThrownRelationVisitor",
                classTypes[3].Metrics[0].ExtractorName);
            Assert.Equal(
                "System.Collections.Generic.Dictionary`2[HoneydewCore.ModelRepresentations.IRelationMetric,System.Collections.Generic.IDictionary`2[System.String,System.Int32]]",
                classTypes[3].Metrics[0].ValueType);

            var dependencies =
                ((Dictionary<IRelationMetric, IDictionary<string, int>>)classTypes[3].Metrics[0].Value)[_sut];

            Assert.Equal(3, dependencies.Count);
            Assert.Equal(1, dependencies["Throwing.MyArgumentNullException"]);
            Assert.Equal(1, dependencies["Throwing.MyArgumentException"]);
            Assert.Equal(1, dependencies["Throwing.MyIndexOutOfRangeException"]);
        }

        [Fact]
        public void Extract_ShouldHaveExceptionsThrown_WhenProvidedWithClassThatRethrowsExplicitExceptions()
        {
            const string fileContent = @"namespace Throwing
{
    using System;

    class MyArgumentException : Exception { }


    public class NumberGenerator
    {
        int[] numbers = {2, 4, 6, 8, 10, 12, 14, 16, 18, 20};

        public int GetNumber(string indexString)
        {
            try
            {
                var index = int.Parse(indexString);

                if (index == 2)
                {
                    throw new MyArgumentException();
                }

                try
                {
                    return numbers[index];
                }
                catch (IndexOutOfRangeException)
                {
                    throw new NullReferenceException();
                }
            }
            catch (Exception e)
            {
                throw new MyArgumentException();
            }
        }
    }
}";

            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes[1].Metrics.Count);
            Assert.Equal("HoneydewExtractors.Core.Metrics.Extraction.Class.Relations.ExceptionsThrownRelationVisitor",
                classTypes[1].Metrics[0].ExtractorName);
            Assert.Equal(
                "System.Collections.Generic.Dictionary`2[HoneydewCore.ModelRepresentations.IRelationMetric,System.Collections.Generic.IDictionary`2[System.String,System.Int32]]",
                classTypes[1].Metrics[0].ValueType);

            var dependencies =
                ((Dictionary<IRelationMetric, IDictionary<string, int>>)classTypes[1].Metrics[0].Value)[_sut];

            Assert.Equal(2, dependencies.Count);
            Assert.Equal(2, dependencies["Throwing.MyArgumentException"]);
            Assert.Equal(1, dependencies["System.NullReferenceException"]);
        }

        [Fact]
        public void Extract_ShouldHaveExceptionsThrown_WhenProvidedWithClassThatRethrowsImplicitExceptions()
        {
            const string fileContent = @"using System;
namespace Throwing
{
    public class NumberGenerator
    {
        int[] numbers = {2, 4, 6, 8, 10, 12, 14, 16, 18, 20};

        public int GetNumber(string indexString)
        {
            try
            {
                var index = int.Parse(indexString);

                try
                {
                    return numbers[index];
                }
                catch (IndexOutOfRangeException)
                {
                    throw;
                }
            }
            catch (NullReferenceException e)
            {
                throw;
            }
        }
    }
}";

            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes[0].Metrics.Count);
            Assert.Equal("HoneydewExtractors.Core.Metrics.Extraction.Class.Relations.ExceptionsThrownRelationVisitor",
                classTypes[0].Metrics[0].ExtractorName);
            Assert.Equal(
                "System.Collections.Generic.Dictionary`2[HoneydewCore.ModelRepresentations.IRelationMetric,System.Collections.Generic.IDictionary`2[System.String,System.Int32]]",
                classTypes[0].Metrics[0].ValueType);

            var dependencies =
                ((Dictionary<IRelationMetric, IDictionary<string, int>>)classTypes[0].Metrics[0].Value)[_sut];

            Assert.Equal(2, dependencies.Count);
            Assert.Equal(1, dependencies["System.IndexOutOfRangeException"]);
            Assert.Equal(1, dependencies["System.NullReferenceException"]);
        }

        [Fact]
        public void
            Extract_ShouldHaveExceptionsThrown_WhenProvidedWithClassThatTrowsExceptionsUsingVariablesParametersFieldsPropertiesAndMethodCalls()
        {
            const string fileContent = @"using System;
namespace Throwing
{
    class MyException : Exception
    {
    }

    public class NumberGenerator
    {
        private readonly NullReferenceException _nullReference = new();

        public IndexOutOfRangeException IndexOutOfRange { get; set; }

        public void GetNumber(double number, Exception exception)
        {
            MyException myException = new MyException();
            var myVarException = new MyException();
            switch (number)
            {
                case < 0:
                    throw exception;
                case < 2:
                    throw _nullReference;
                case < 5:
                    throw IndexOutOfRange;
                case < 6:
                    throw myException;
                case <7:
                    throw myVarException;
                default:
                    throw GetException();
            }
        }

        public NotSupportedException GetException()
        {
            return new();
        }
    }
}";

            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes[1].Metrics.Count);
            Assert.Equal("HoneydewExtractors.Core.Metrics.Extraction.Class.Relations.ExceptionsThrownRelationVisitor",
                classTypes[1].Metrics[0].ExtractorName);
            Assert.Equal(
                "System.Collections.Generic.Dictionary`2[HoneydewCore.ModelRepresentations.IRelationMetric,System.Collections.Generic.IDictionary`2[System.String,System.Int32]]",
                classTypes[1].Metrics[0].ValueType);

            var dependencies =
                ((Dictionary<IRelationMetric, IDictionary<string, int>>)classTypes[1].Metrics[0].Value)[_sut];

            Assert.Equal(5, dependencies.Count);
            Assert.Equal(1, dependencies["System.IndexOutOfRangeException"]);
            Assert.Equal(1, dependencies["System.NullReferenceException"]);
            Assert.Equal(1, dependencies["System.Exception"]);
            Assert.Equal(1, dependencies["System.NotSupportedException"]);
            Assert.Equal(2, dependencies["Throwing.MyException"]);
        }

        [Fact]
        public void Extract_ShouldHaveExternalExceptionsThrown_WhenProvidedWithClassThatTrowsExternalExceptions()
        {
            const string fileContent = @"
namespace Throwing
{
    public class NumberGenerator
    {
        private readonly ExternException _nullReference = new();

        public ExternException IndexOutOfRange { get; set; }

        public void GetNumber(double number, ExternException exception)
        {
            ExternException myException = new ExternException();
            var myVarException = new ExternException();
            switch (number)
            {
                case < 0:
                    throw exception;
                case < 2:
                    throw _nullReference;
                case < 5:
                    throw IndexOutOfRange;
                case < 6:
                    throw myException;
                case <7:
                    throw myVarException;
                case < 8:
                    throw new ExternException();
                default:
                    throw GetException();
            }

            try 
            {
                OtherClass.Call();
            }
            catch (ExternException)
            {
                throw;
            }
        }

        public ExternException GetException()
        {
            return new();
        }
    }
}";

            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes[0].Metrics.Count);
            Assert.Equal("HoneydewExtractors.Core.Metrics.Extraction.Class.Relations.ExceptionsThrownRelationVisitor",
                classTypes[0].Metrics[0].ExtractorName);
            Assert.Equal(
                "System.Collections.Generic.Dictionary`2[HoneydewCore.ModelRepresentations.IRelationMetric,System.Collections.Generic.IDictionary`2[System.String,System.Int32]]",
                classTypes[0].Metrics[0].ValueType);

            var dependencies =
                ((Dictionary<IRelationMetric, IDictionary<string, int>>)classTypes[0].Metrics[0].Value)[_sut];

            Assert.Single(dependencies);
            Assert.Equal(8, dependencies["ExternException"]);
        }

        [Fact]
        public void Extract_ShouldHaveExceptionsThrown_WhenProvidedWithConditionalOperatorWithThrowException()
        {
            const string fileContent = @"using System;
namespace Throwing
{
    public class NumberGenerator
    {
        private static void DisplayFirstNumber(string[] args)
        {
            string arg = args.Length >= 1 ? args[0] :
                              throw new ArgumentException(""You must supply an argument"");
            if (Int64.TryParse(arg, out var number))
                Console.WriteLine($""You entered {number:F0}"");
            else
                Console.WriteLine($""{arg} is not a number."");
        }
    }
}";

            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes[0].Metrics.Count);
            Assert.Equal("HoneydewExtractors.Core.Metrics.Extraction.Class.Relations.ExceptionsThrownRelationVisitor",
                classTypes[0].Metrics[0].ExtractorName);
            Assert.Equal(
                "System.Collections.Generic.Dictionary`2[HoneydewCore.ModelRepresentations.IRelationMetric,System.Collections.Generic.IDictionary`2[System.String,System.Int32]]",
                classTypes[0].Metrics[0].ValueType);

            var dependencies =
                ((Dictionary<IRelationMetric, IDictionary<string, int>>)classTypes[0].Metrics[0].Value)[_sut];

            Assert.Single(dependencies);
            Assert.Equal(1, dependencies["System.ArgumentException"]);
        }

        [Fact]
        public void Extract_ShouldHaveExceptionsThrown_WhenProvidedWithNullCoalescingOperatorWithThrowException()
        {
            const string fileContent = @"using System;
namespace Throwing
{
    public class NumberGenerator
    {
        public string Name
        {
            get => name;
            set => name = value ??
                throw new ArgumentNullException(paramName: nameof(value), message: ""Name cannot be null"");
        }
    }
}";

            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes[0].Metrics.Count);
            Assert.Equal("HoneydewExtractors.Core.Metrics.Extraction.Class.Relations.ExceptionsThrownRelationVisitor",
                classTypes[0].Metrics[0].ExtractorName);
            Assert.Equal(
                "System.Collections.Generic.Dictionary`2[HoneydewCore.ModelRepresentations.IRelationMetric,System.Collections.Generic.IDictionary`2[System.String,System.Int32]]",
                classTypes[0].Metrics[0].ValueType);

            var dependencies =
                ((Dictionary<IRelationMetric, IDictionary<string, int>>)classTypes[0].Metrics[0].Value)[_sut];

            Assert.Single(dependencies);
            Assert.Equal(1, dependencies["System.ArgumentNullException"]);
        }

        [Fact]
        public void Extract_ShouldHaveExceptionsThrown_WhenProvidedWithLambdaThathrowsException()
        {
            const string fileContent = @"using System;
namespace Throwing
{
    public class NumberGenerator
    {
        public void Method()
        {
            DateTime ToDateTime(IFormatProvider provider) =>
                throw new InvalidCastException(""Conversion to a DateTime is not supported."");
        }
    }
}";

            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes[0].Metrics.Count);
            Assert.Equal("HoneydewExtractors.Core.Metrics.Extraction.Class.Relations.ExceptionsThrownRelationVisitor",
                classTypes[0].Metrics[0].ExtractorName);
            Assert.Equal(
                "System.Collections.Generic.Dictionary`2[HoneydewCore.ModelRepresentations.IRelationMetric,System.Collections.Generic.IDictionary`2[System.String,System.Int32]]",
                classTypes[0].Metrics[0].ValueType);

            var dependencies =
                ((Dictionary<IRelationMetric, IDictionary<string, int>>)classTypes[0].Metrics[0].Value)[_sut];

            Assert.Single(dependencies);
            Assert.Equal(1, dependencies["System.InvalidCastException"]);
        }
    }
}
