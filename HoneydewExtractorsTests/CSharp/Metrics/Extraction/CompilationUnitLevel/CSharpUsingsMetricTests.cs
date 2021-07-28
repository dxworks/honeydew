using System.Collections.Generic;
using HoneydewExtractors.Core.Metrics.Extraction;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewExtractors.CSharp.Metrics.Extraction.CompilationUnitLevel;
using Xunit;

// todo equal - aliasing
// todo static

namespace HoneydewExtractorsTests.CSharp.Metrics.Extraction.CompilationUnitLevel
{
    public class CSharpUsingsMetricTests
    {
        private readonly CSharpFactExtractor _factExtractor;

        public CSharpUsingsMetricTests()
        {
            _factExtractor = new CSharpFactExtractor();
            _factExtractor.AddMetric<CSharpUsingsMetric>();
        }

        [Fact]
        public void GetMetricType_ShouldReturnCompilationUnitLevel()
        {
            Assert.Equal(ExtractionMetricType.CompilationUnitLevel, new CSharpUsingsMetric().GetMetricType());
        }

        [Fact]
        public void PrettyPrint_ShouldReturnUsings()
        {
            Assert.Equal("Usings", new CSharpUsingsMetric().PrettyPrint());
        }

        [Theory]
        [InlineData("class")]
        [InlineData("record")]
        [InlineData("struct")]
        [InlineData("enum")]
        [InlineData("interface")]
        public void Extract_ShouldHaveUsings_WhenGivenOneClass(string classType)
        {
            var fileContent = $@"using System;
                                     using System.Collections.Generic;
                                     using System.Linq;
                                     using System.Text;
                                     using Microsoft.CodeAnalysis;
                                     using Microsoft.CodeAnalysis.CSharp;

                                     namespace TopLevel
                                     {{
                                         public {classType} Foo {{ }}                                        
                                     }}";

            var classModels = _factExtractor.Extract(fileContent);

            Assert.Equal(1, classModels.Count);

            var optional = classModels[0].GetMetricValue<CSharpUsingsMetric>();
            Assert.True(optional.HasValue);
            var usings = (IDictionary<string, ISet<string>>) optional.Value;

            Assert.Equal(1, usings.Count);
            var usingsList = usings["TopLevel.Foo"];

            Assert.Equal(6, usingsList.Count);
            Assert.True(usingsList.Contains("System"));
            Assert.True(usingsList.Contains("System.Collections.Generic"));
            Assert.True(usingsList.Contains("System.Linq"));
            Assert.True(usingsList.Contains("System.Text"));
            Assert.True(usingsList.Contains("Microsoft.CodeAnalysis"));
            Assert.True(usingsList.Contains("Microsoft.CodeAnalysis.CSharp"));
        }

        [Fact]
        public void Extract_ShouldHaveUsings_WhenGivenOneDelegate()
        {
            const string fileContent = @"using System;
                                     using System.Collections.Generic;
                                     using System.Linq;
                                     using System.Text;
                                     using Microsoft.CodeAnalysis;
                                     using Microsoft.CodeAnalysis.CSharp;

                                     namespace TopLevel
                                     {
                                         public delegate void Foo();                                        
                                     }";

            var classModels = _factExtractor.Extract(fileContent);

            Assert.Equal(1, classModels.Count);

            var optional = classModels[0].GetMetricValue<CSharpUsingsMetric>();
            Assert.True(optional.HasValue);
            var usings = (IDictionary<string, ISet<string>>) optional.Value;

            Assert.Equal(1, usings.Count);
            var usingsList = usings["TopLevel.Foo"];

            Assert.Equal(6, usingsList.Count);

            Assert.True(usingsList.Contains("System"));
            Assert.True(usingsList.Contains("System.Collections.Generic"));
            Assert.True(usingsList.Contains("System.Linq"));
            Assert.True(usingsList.Contains("System.Text"));
            Assert.True(usingsList.Contains("Microsoft.CodeAnalysis"));
            Assert.True(usingsList.Contains("Microsoft.CodeAnalysis.CSharp"));
        }

        [Fact]
        public void Extract_ShouldHaveUsings_WhenGivenMultipleClassesAndDelegatesWithTheSameUsings()
        {
            const string fileContent = @"using System;
                                     using System.Collections.Generic;
                                     using System.Linq;
                                     using System.Text;
                                     using Microsoft.CodeAnalysis;
                                     using Microsoft.CodeAnalysis.CSharp;

                                     namespace TopLevel
                                     {
                                         class Class1{}
                                         record Record1 {}
                                         public  struct Struct1{}
                                         public interface Interface1 {}
                                         public delegate void Foo();                                        
                                     }";

            var classModels = _factExtractor.Extract(fileContent);

            Assert.Equal(5, classModels.Count);

            var optional = classModels[0].GetMetricValue<CSharpUsingsMetric>();
            Assert.True(optional.HasValue);
            var usings = (IDictionary<string, ISet<string>>) optional.Value;

            Assert.Equal(5, usings.Count);
            Assert.True(usings.ContainsKey("TopLevel.Class1"));
            Assert.True(usings.ContainsKey("TopLevel.Record1"));
            Assert.True(usings.ContainsKey("TopLevel.Struct1"));
            Assert.True(usings.ContainsKey("TopLevel.Interface1"));
            Assert.True(usings.ContainsKey("TopLevel.Foo"));

            foreach (var (_, usingsList) in usings)
            {
                Assert.Equal(6, usingsList.Count);

                Assert.True(usingsList.Contains("System"));
                Assert.True(usingsList.Contains("System.Collections.Generic"));
                Assert.True(usingsList.Contains("System.Linq"));
                Assert.True(usingsList.Contains("System.Text"));
                Assert.True(usingsList.Contains("Microsoft.CodeAnalysis"));
                Assert.True(usingsList.Contains("Microsoft.CodeAnalysis.CSharp"));
            }
        }

        [Fact]
        public void
            Extract_ShouldHaveUsings_WhenGivenMultipleClassesAndDelegatesWithTheSameUsings_ButUsingsAreDeclaredInsideTheNamespace()
        {
            const string fileContent = @"
                                     namespace TopLevel
                                     {
                                         using System;
                                         using System.Collections.Generic;
                                         using System.Linq;
                                         using System.Text;
                                         using Microsoft.CodeAnalysis;
                                         using Microsoft.CodeAnalysis.CSharp;

                                         class Class1{}
                                         record Record1 {}
                                         public struct Struct1{}
                                         public interface Interface1 {}
                                         public delegate void Foo();                                        
                                     }";

            var classModels = _factExtractor.Extract(fileContent);

            Assert.Equal(5, classModels.Count);

            var optional = classModels[0].GetMetricValue<CSharpUsingsMetric>();
            Assert.True(optional.HasValue);
            var usings = (IDictionary<string, ISet<string>>) optional.Value;

            Assert.Equal(5, usings.Count);
            Assert.True(usings.ContainsKey("TopLevel.Class1"));
            Assert.True(usings.ContainsKey("TopLevel.Record1"));
            Assert.True(usings.ContainsKey("TopLevel.Struct1"));
            Assert.True(usings.ContainsKey("TopLevel.Interface1"));
            Assert.True(usings.ContainsKey("TopLevel.Foo"));

            foreach (var (_, usingsList) in usings)
            {
                Assert.Equal(6, usingsList.Count);

                Assert.True(usingsList.Contains("System"));
                Assert.True(usingsList.Contains("System.Collections.Generic"));
                Assert.True(usingsList.Contains("System.Linq"));
                Assert.True(usingsList.Contains("System.Text"));
                Assert.True(usingsList.Contains("Microsoft.CodeAnalysis"));
                Assert.True(usingsList.Contains("Microsoft.CodeAnalysis.CSharp"));
            }
        }

        [Fact]
        public void Extract_ShouldHaveUsings_WhenGivenMultipleClassesAndDelegatesInMultipleNamespacesWithSharedUsings()
        {
            const string fileContent = @"
using System;
using System.Collections.Generic;

namespace TopLevel
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;


    class Class1
    {
    }
    record Record1 {}
    public struct Struct1{}
}

namespace Analyzers
{
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    
    public interface Interface1 {}
    public delegate void Foo();
}";

            var classModels = _factExtractor.Extract(fileContent);

            Assert.Equal(5, classModels.Count);

            var optional = classModels[0].GetMetricValue<CSharpUsingsMetric>();
            Assert.True(optional.HasValue);
            var usings = (IDictionary<string, ISet<string>>) optional.Value;

            Assert.Equal(5, usings.Count);
            Assert.True(usings.ContainsKey("TopLevel.Class1"));
            Assert.True(usings.ContainsKey("TopLevel.Record1"));
            Assert.True(usings.ContainsKey("TopLevel.Struct1"));
            Assert.True(usings.ContainsKey("Analyzers.Interface1"));
            Assert.True(usings.ContainsKey("Analyzers.Foo"));

            foreach (var (_, usingsList) in usings)
            {
                Assert.True(usingsList.Contains("System"));
                Assert.True(usingsList.Contains("System.Collections.Generic"));
            }

            var class1Usings = usings["TopLevel.Class1"];
            Assert.Equal(4, class1Usings.Count);
            Assert.True(class1Usings.Contains("System.Linq"));
            Assert.True(class1Usings.Contains("System.Text"));

            var recordUsings = usings["TopLevel.Record1"];
            Assert.Equal(4, recordUsings.Count);
            Assert.True(recordUsings.Contains("System.Linq"));
            Assert.True(recordUsings.Contains("System.Text"));

            var structUsings = usings["TopLevel.Struct1"];
            Assert.Equal(4, structUsings.Count);
            Assert.True(structUsings.Contains("System.Linq"));
            Assert.True(structUsings.Contains("System.Text"));

            var interfaceUsings = usings["Analyzers.Interface1"];
            Assert.Equal(5, interfaceUsings.Count);
            Assert.True(interfaceUsings.Contains("System.Linq"));
            Assert.True(interfaceUsings.Contains("Microsoft.CodeAnalysis"));
            Assert.True(interfaceUsings.Contains("Microsoft.CodeAnalysis.CSharp"));

            var delegateUsings = usings["Analyzers.Foo"];
            Assert.Equal(5, delegateUsings.Count);
            Assert.True(delegateUsings.Contains("System.Linq"));
            Assert.True(delegateUsings.Contains("Microsoft.CodeAnalysis"));
            Assert.True(delegateUsings.Contains("Microsoft.CodeAnalysis.CSharp"));
        }

        [Fact]
        public void Extract_ShouldHaveUsings_WhenGivenMultipleClassesAndDelegatesInImbricatedNamespaces()
        {
            const string fileContent = @"
    using System;
    using System.Collections.Generic;

    namespace TopLevel
    {
        using System.Collections.Generic;
        using System.Linq;
        using System.Text;
    
        namespace Analyzers
        {
            using System.Linq;
            using Microsoft.CodeAnalysis;

            public interface Interface1 {}

            namespace MyDelegates
            {
                using Microsoft.CodeAnalysis.CSharp;
            
                public delegate void Foo();
            
                namespace Structs
                {
                    using System.Text;
                    using MyLib;

                    public struct Struct1{}   
                }
            }

            namespace My.Records
            {
                using MyLib.Records;
                public record Record1{}
            }
        }
    }";

            var classModels = _factExtractor.Extract(fileContent);

            Assert.Equal(4, classModels.Count);

            var optional = classModels[0].GetMetricValue<CSharpUsingsMetric>();
            Assert.True(optional.HasValue);
            var usings = (IDictionary<string, ISet<string>>) optional.Value;

            Assert.Equal(4, usings.Count);
            Assert.True(usings.ContainsKey("TopLevel.Analyzers.Interface1"));
            Assert.True(usings.ContainsKey("TopLevel.Analyzers.MyDelegates.Foo"));
            Assert.True(usings.ContainsKey("TopLevel.Analyzers.MyDelegates.Structs.Struct1"));
            Assert.True(usings.ContainsKey("TopLevel.Analyzers.My.Records.Record1"));

            foreach (var (_, usingsList) in usings)
            {
                Assert.True(usingsList.Contains("System"));
                Assert.True(usingsList.Contains("System.Collections.Generic"));
                Assert.True(usingsList.Contains("System.Linq"));
                Assert.True(usingsList.Contains("System.Text"));
                Assert.True(usingsList.Contains("Microsoft.CodeAnalysis"));
            }

            var interfaceUsings = usings["TopLevel.Analyzers.Interface1"];
            Assert.Equal(5, interfaceUsings.Count);

            var delegateUsings = usings["TopLevel.Analyzers.MyDelegates.Foo"];
            Assert.Equal(6, delegateUsings.Count);
            Assert.True(delegateUsings.Contains("Microsoft.CodeAnalysis.CSharp"));

            var structUsings = usings["TopLevel.Analyzers.MyDelegates.Structs.Struct1"];
            Assert.Equal(7, structUsings.Count);
            Assert.True(structUsings.Contains("Microsoft.CodeAnalysis.CSharp"));
            Assert.True(structUsings.Contains("MyLib"));

            var recordUsings = usings["TopLevel.Analyzers.My.Records.Record1"];
            Assert.Equal(6, recordUsings.Count);
            Assert.True(recordUsings.Contains("MyLib.Records"));
        }

        [Fact]
        public void Extract_ShouldHaveUsings_WhenGivenMultipleClassesWithInnerClasses()
        {
            const string fileContent = @"
using System;
using System.Collections.Generic;
using System.Linq;

namespace TopLevel
{
    using System.Text;
    class Class1
    {
        public delegate void Foo();
    }
    
    namespace Analyzers
    {
        using Microsoft.CodeAnalysis.CSharp;
        
        class Class2
        {
            class InnerClass1
            {
                class InnerClass2{}
            }   
        }

        namespace EmptyNamespace
        {
            using MyLib;
        }
        public interface Interface1
        {
            interface IInnerInterface{}
        }
    }
}";

            var classModels = _factExtractor.Extract(fileContent);

            Assert.Equal(7, classModels.Count);

            var optional = classModels[0].GetMetricValue<CSharpUsingsMetric>();
            Assert.True(optional.HasValue);
            var usings = (IDictionary<string, ISet<string>>) optional.Value;

            Assert.Equal(7, usings.Count);
            Assert.True(usings.ContainsKey("TopLevel.Class1"));
            Assert.True(usings.ContainsKey("TopLevel.Class1.Foo"));
            Assert.True(usings.ContainsKey("TopLevel.Analyzers.Class2"));
            Assert.True(usings.ContainsKey("TopLevel.Analyzers.Class2.InnerClass1"));
            Assert.True(usings.ContainsKey("TopLevel.Analyzers.Class2.InnerClass1.InnerClass2"));
            Assert.True(usings.ContainsKey("TopLevel.Analyzers.Interface1"));
            Assert.True(usings.ContainsKey("TopLevel.Analyzers.Interface1.IInnerInterface"));

            foreach (var (_, usingsList) in usings)
            {
                Assert.True(usingsList.Contains("System"));
                Assert.True(usingsList.Contains("System.Collections.Generic"));
                Assert.True(usingsList.Contains("System.Linq"));
                Assert.True(usingsList.Contains("System.Text"));
                Assert.False(usingsList.Contains("MyLib"));
            }

            Assert.Equal(4, usings["TopLevel.Class1"].Count);
            Assert.Equal(4, usings["TopLevel.Class1.Foo"].Count);

            var class2Usings = usings["TopLevel.Analyzers.Class2"];
            Assert.Equal(5, class2Usings.Count);
            Assert.True(class2Usings.Contains("Microsoft.CodeAnalysis.CSharp"));

            var innerClass1Usings = usings["TopLevel.Analyzers.Class2.InnerClass1"];
            Assert.Equal(5, innerClass1Usings.Count);
            Assert.True(innerClass1Usings.Contains("Microsoft.CodeAnalysis.CSharp"));

            var innerClass2Usings = usings["TopLevel.Analyzers.Class2.InnerClass1.InnerClass2"];
            Assert.Equal(5, innerClass2Usings.Count);
            Assert.True(innerClass2Usings.Contains("Microsoft.CodeAnalysis.CSharp"));

            var interface1Usings = usings["TopLevel.Analyzers.Interface1"];
            Assert.Equal(5, interface1Usings.Count);
            Assert.True(interface1Usings.Contains("Microsoft.CodeAnalysis.CSharp"));

            var innerInterfaceUsings = usings["TopLevel.Analyzers.Interface1.IInnerInterface"];
            Assert.Equal(5, innerInterfaceUsings.Count);
            Assert.True(innerInterfaceUsings.Contains("Microsoft.CodeAnalysis.CSharp"));
        }

        [Fact]
        public void Extract_ShouldHaveUsings_WhenGivenMultipleClassesWithInnerClasses_ButNoNamespace()
        {
            const string fileContent = @"
using System;
using System.Collections.Generic;
using System.Linq;
using MyLib;

class Class2
{
    class InnerClass1
    {
        class InnerClass2{}
    }   
}";

            var classModels = _factExtractor.Extract(fileContent);

            Assert.Equal(3, classModels.Count);

            var optional = classModels[0].GetMetricValue<CSharpUsingsMetric>();
            Assert.True(optional.HasValue);
            var usings = (IDictionary<string, ISet<string>>) optional.Value;

            Assert.Equal(3, usings.Count);
            Assert.True(usings.ContainsKey("Class2"));
            Assert.True(usings.ContainsKey("Class2.InnerClass1"));
            Assert.True(usings.ContainsKey("Class2.InnerClass1.InnerClass2"));

            foreach (var (_, usingsList) in usings)
            {
                Assert.Equal(4, usingsList.Count);
                Assert.True(usingsList.Contains("System"));
                Assert.True(usingsList.Contains("System.Collections.Generic"));
                Assert.True(usingsList.Contains("System.Linq"));
                Assert.True(usingsList.Contains("MyLib"));
            }
        }


        [Fact]
        public void Extract_ShouldHaveUsingsInClassModels_WhenGivenText()
        {
            const string fileContent = @"
    using System;
    using System.Collections.Generic;

    namespace TopLevel
    {
        using System.Collections.Generic;
        using System.Linq;
        using System.Text;
    
        namespace Analyzers
        {
            using System.Linq;
            using Microsoft.CodeAnalysis;

            public interface Interface1 {}

            namespace MyDelegates
            {
                using Microsoft.CodeAnalysis.CSharp;
            
                public delegate void Foo();
            
                namespace Structs
                {
                    using System.Text;
                    using MyLib;

                    public struct Struct1{}   
                }
            }

            namespace My.Records
            {
                using MyLib.Records;
                public class Class1 {
                public record Record1{}}
            }
        }
    }";

            var classModels = _factExtractor.Extract(fileContent);

            Assert.Equal(5, classModels.Count);
            
            foreach (var classModel in classModels)
            {
                Assert.True(classModel.Usings.Contains("System"));
                Assert.True(classModel.Usings.Contains("System.Collections.Generic"));
                Assert.True(classModel.Usings.Contains("System.Linq"));
                Assert.True(classModel.Usings.Contains("System.Text"));
                Assert.True(classModel.Usings.Contains("Microsoft.CodeAnalysis"));
            }
            
            Assert.Equal(5, classModels[0].Usings.Count);

            var structUsings = classModels[1].Usings;
            Assert.Equal(7, structUsings.Count);
            Assert.True(structUsings.Contains("Microsoft.CodeAnalysis.CSharp"));
            Assert.True(structUsings.Contains("MyLib"));

            var recordUsings = classModels[2].Usings;
            Assert.Equal(6, recordUsings.Count);
            Assert.True(recordUsings.Contains("MyLib.Records"));

            var classUsings = classModels[3].Usings;
            Assert.Equal(6, classUsings.Count);
            Assert.True(classUsings.Contains("MyLib.Records"));
            
            var delegateUsings = classModels[4].Usings;
            Assert.Equal(6, delegateUsings.Count);
            Assert.True(delegateUsings.Contains("Microsoft.CodeAnalysis.CSharp"));
        }
    }
}
