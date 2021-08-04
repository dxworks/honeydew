using System.Collections.Generic;
using HoneydewExtractors.Core.Metrics.Extraction;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewExtractors.CSharp.Metrics.Extraction.ClassLevel.RelationMetric;
using Xunit;

namespace HoneydewExtractorsTests.CSharp.Metrics.Extraction.ClassLevel.RelationMetric
{
    public class CSharpExceptionsThrownRelationMetricTests
    {
        private readonly CSharpExceptionsThrownRelationMetric _sut;
        private readonly CSharpFactExtractor _factExtractor;

        public CSharpExceptionsThrownRelationMetricTests()
        {
            _sut = new CSharpExceptionsThrownRelationMetric();
            _factExtractor = new CSharpFactExtractor();
            _factExtractor.AddMetric<CSharpExceptionsThrownRelationMetric>();
        }

        [Fact]
        public void GetMetricType_ShouldReturnClassLevel()
        {
            Assert.Equal(ExtractionMetricType.ClassLevel, _sut.GetMetricType());
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

            var classModels = _factExtractor.Extract(fileContent);

            var optional = classModels[1].GetMetricValue<CSharpExceptionsThrownRelationMetric>();
            Assert.True(optional.HasValue);

            var dependencies = (IDictionary<string, int>) optional.Value;

            Assert.Empty(dependencies);
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

            var classModels = _factExtractor.Extract(fileContent);

            var optional = classModels[0].GetMetricValue<CSharpExceptionsThrownRelationMetric>();
            Assert.True(optional.HasValue);

            var dependencies = (IDictionary<string, int>) optional.Value;

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

            var classModels = _factExtractor.Extract(fileContent);

            var optional = classModels[3].GetMetricValue<CSharpExceptionsThrownRelationMetric>();
            Assert.True(optional.HasValue);

            var dependencies = (IDictionary<string, int>) optional.Value;

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

            var classModels = _factExtractor.Extract(fileContent);

            var optional = classModels[1].GetMetricValue<CSharpExceptionsThrownRelationMetric>();
            Assert.True(optional.HasValue);

            var dependencies = (IDictionary<string, int>) optional.Value;

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

            var classModels = _factExtractor.Extract(fileContent);

            var optional = classModels[0].GetMetricValue<CSharpExceptionsThrownRelationMetric>();
            Assert.True(optional.HasValue);

            var dependencies = (IDictionary<string, int>) optional.Value;

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

            var classModels = _factExtractor.Extract(fileContent);

            var optional = classModels[1].GetMetricValue<CSharpExceptionsThrownRelationMetric>();
            Assert.True(optional.HasValue);

            var dependencies = (IDictionary<string, int>) optional.Value;

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

            var classModels = _factExtractor.Extract(fileContent);

            var optional = classModels[0].GetMetricValue<CSharpExceptionsThrownRelationMetric>();
            Assert.True(optional.HasValue);

            var dependencies = (IDictionary<string, int>) optional.Value;

            Assert.Equal(1, dependencies.Count);
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

            var classModels = _factExtractor.Extract(fileContent);

            var optional = classModels[0].GetMetricValue<CSharpExceptionsThrownRelationMetric>();
            Assert.True(optional.HasValue);

            var dependencies = (IDictionary<string, int>) optional.Value;

            Assert.Equal(1, dependencies.Count);
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

            var classModels = _factExtractor.Extract(fileContent);

            var optional = classModels[0].GetMetricValue<CSharpExceptionsThrownRelationMetric>();
            Assert.True(optional.HasValue);

            var dependencies = (IDictionary<string, int>) optional.Value;

            Assert.Equal(1, dependencies.Count);
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

            var classModels = _factExtractor.Extract(fileContent);

            var optional = classModels[0].GetMetricValue<CSharpExceptionsThrownRelationMetric>();
            Assert.True(optional.HasValue);

            var dependencies = (IDictionary<string, int>) optional.Value;

            Assert.Equal(1, dependencies.Count);
            Assert.Equal(1, dependencies["System.InvalidCastException"]);
        }

        [Fact]
        public void GetRelations_ShouldHaveNoRelations_WhenClassHasNoFields()
        {
            var fileRelations = _sut.GetRelations(new Dictionary<string, int>());

            Assert.Empty(fileRelations);
        }

        [Fact]
        public void GetRelations_ShouldHaveNoRelations_WhenDependenciesAreOnlyPrimitiveTypes()
        {
            var fileRelations = _sut.GetRelations(new Dictionary<string, int>
            {
                {"int", 3},
                {"float", 2},
                {"string", 1}
            });

            Assert.Empty(fileRelations);
        }

        [Fact]
        public void GetRelations_Extract_ShouldHaveRelations_WhenThereAreNonPrimitiveDependencies()
        {
            var fileRelations = _sut.GetRelations(new Dictionary<string, int>
            {
                {"int", 3},
                {"IFactExtractor", 2},
                {"CSharpMetricExtractor", 1}
            });

            Assert.NotEmpty(fileRelations);
            Assert.Equal(2, fileRelations.Count);

            var fileRelation1 = fileRelations[0];
            Assert.Equal("IFactExtractor", fileRelation1.FileTarget);
            Assert.Equal(typeof(CSharpExceptionsThrownRelationMetric).FullName, fileRelation1.RelationType);
            Assert.Equal(2, fileRelation1.RelationCount);

            var fileRelation2 = fileRelations[1];
            Assert.Equal("CSharpMetricExtractor", fileRelation2.FileTarget);
            Assert.Equal(typeof(CSharpExceptionsThrownRelationMetric).FullName, fileRelation2.RelationType);
            Assert.Equal(1, fileRelation2.RelationCount);
        }
    }
}
