using System.Collections.Generic;
using System.Linq;
using HoneydewExtractors.Core.Metrics.Extraction;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewExtractors.CSharp.Metrics.Extraction.CompilationUnitLevel;
using HoneydewModels.CSharp;
using Xunit;

// todo equal - aliasing

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
            var usings = (IDictionary<string, ISet<UsingModel>>) optional.Value;

            Assert.Equal(1, usings.Count);
            var usingsList = usings["TopLevel.Foo"];

            Assert.Equal(6, usingsList.Count);

            Assert.NotNull(usingsList.SingleOrDefault(model => model.Name == "System"));
            Assert.NotNull(usingsList.SingleOrDefault(model => model.Name == "System.Collections.Generic"));
            Assert.NotNull(usingsList.SingleOrDefault(model => model.Name == "System.Linq"));
            Assert.NotNull(usingsList.SingleOrDefault(model => model.Name == "System.Text"));
            Assert.NotNull(usingsList.SingleOrDefault(model => model.Name == "Microsoft.CodeAnalysis"));
            Assert.NotNull(usingsList.SingleOrDefault(model => model.Name == "Microsoft.CodeAnalysis.CSharp"));
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
            var usings = (IDictionary<string, ISet<UsingModel>>) optional.Value;

            Assert.Equal(1, usings.Count);
            var usingsList = usings["TopLevel.Foo"];

            Assert.Equal(6, usingsList.Count);

            Assert.NotNull(usingsList.SingleOrDefault(model => model.Name == "System"));
            Assert.NotNull(usingsList.SingleOrDefault(model => model.Name == "System.Collections.Generic"));
            Assert.NotNull(usingsList.SingleOrDefault(model => model.Name == "System.Linq"));
            Assert.NotNull(usingsList.SingleOrDefault(model => model.Name == "System.Text"));
            Assert.NotNull(usingsList.SingleOrDefault(model => model.Name == "Microsoft.CodeAnalysis"));
            Assert.NotNull(usingsList.SingleOrDefault(model => model.Name == "Microsoft.CodeAnalysis.CSharp"));
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
            var usings = (IDictionary<string, ISet<UsingModel>>) optional.Value;

            Assert.Equal(5, usings.Count);
            Assert.True(usings.ContainsKey("TopLevel.Class1"));
            Assert.True(usings.ContainsKey("TopLevel.Record1"));
            Assert.True(usings.ContainsKey("TopLevel.Struct1"));
            Assert.True(usings.ContainsKey("TopLevel.Interface1"));
            Assert.True(usings.ContainsKey("TopLevel.Foo"));

            foreach (var (_, usingsList) in usings)
            {
                Assert.Equal(6, usingsList.Count);

                Assert.NotNull(usingsList.SingleOrDefault(model => model.Name == "System"));
                Assert.NotNull(usingsList.SingleOrDefault(model => model.Name == "System"));
                Assert.NotNull(usingsList.SingleOrDefault(model => model.Name == "System.Collections.Generic"));
                Assert.NotNull(usingsList.SingleOrDefault(model => model.Name == "System.Linq"));
                Assert.NotNull(usingsList.SingleOrDefault(model => model.Name == "System.Text"));
                Assert.NotNull(usingsList.SingleOrDefault(model => model.Name == "Microsoft.CodeAnalysis"));
                Assert.NotNull(usingsList.SingleOrDefault(model => model.Name == "Microsoft.CodeAnalysis.CSharp"));
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
            var usings = (IDictionary<string, ISet<UsingModel>>) optional.Value;

            Assert.Equal(5, usings.Count);
            Assert.True(usings.ContainsKey("TopLevel.Class1"));
            Assert.True(usings.ContainsKey("TopLevel.Record1"));
            Assert.True(usings.ContainsKey("TopLevel.Struct1"));
            Assert.True(usings.ContainsKey("TopLevel.Interface1"));
            Assert.True(usings.ContainsKey("TopLevel.Foo"));

            foreach (var (_, usingsList) in usings)
            {
                Assert.Equal(6, usingsList.Count);

                Assert.NotNull(usingsList.SingleOrDefault(model => model.Name == "System"));
                Assert.NotNull(usingsList.SingleOrDefault(model => model.Name == "System.Collections.Generic"));
                Assert.NotNull(usingsList.SingleOrDefault(model => model.Name == "System.Linq"));
                Assert.NotNull(usingsList.SingleOrDefault(model => model.Name == "System.Text"));
                Assert.NotNull(usingsList.SingleOrDefault(model => model.Name == "Microsoft.CodeAnalysis"));
                Assert.NotNull(usingsList.SingleOrDefault(model => model.Name == "Microsoft.CodeAnalysis.CSharp"));
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
            var usings = (IDictionary<string, ISet<UsingModel>>) optional.Value;

            Assert.Equal(5, usings.Count);
            Assert.True(usings.ContainsKey("TopLevel.Class1"));
            Assert.True(usings.ContainsKey("TopLevel.Record1"));
            Assert.True(usings.ContainsKey("TopLevel.Struct1"));
            Assert.True(usings.ContainsKey("Analyzers.Interface1"));
            Assert.True(usings.ContainsKey("Analyzers.Foo"));

            foreach (var (_, usingsList) in usings)
            {
                Assert.NotNull(usingsList.SingleOrDefault(model => model.Name == "System"));
                Assert.NotNull(usingsList.SingleOrDefault(model => model.Name == "System.Collections.Generic"));
            }

            var class1Usings = usings["TopLevel.Class1"];
            Assert.Equal(4, class1Usings.Count);
            Assert.Contains(class1Usings, model => model.Name == "System.Linq");
            Assert.Contains(class1Usings, model => model.Name == "System.Text");

            var recordUsings = usings["TopLevel.Record1"];
            Assert.Equal(4, recordUsings.Count);
            Assert.Contains(recordUsings, model => model.Name == "System.Linq");
            Assert.Contains(recordUsings, model => model.Name == "System.Text");

            var structUsings = usings["TopLevel.Struct1"];
            Assert.Equal(4, structUsings.Count);
            Assert.Contains(structUsings, model => model.Name == "System.Linq");
            Assert.Contains(structUsings, model => model.Name == "System.Text");

            var interfaceUsings = usings["Analyzers.Interface1"];
            Assert.Equal(5, interfaceUsings.Count);
            Assert.Contains(interfaceUsings, model => model.Name == "System.Linq");
            Assert.Contains(interfaceUsings, model => model.Name == "Microsoft.CodeAnalysis");
            Assert.Contains(interfaceUsings, model => model.Name == "Microsoft.CodeAnalysis.CSharp");

            var delegateUsings = usings["Analyzers.Foo"];
            Assert.Equal(5, delegateUsings.Count);
            Assert.Contains(delegateUsings, model => model.Name == "System.Linq");
            Assert.Contains(delegateUsings, model => model.Name == "Microsoft.CodeAnalysis");
            Assert.Contains(delegateUsings, model => model.Name == "Microsoft.CodeAnalysis.CSharp");
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
            var usings = (IDictionary<string, ISet<UsingModel>>) optional.Value;

            Assert.Equal(4, usings.Count);
            Assert.True(usings.ContainsKey("TopLevel.Analyzers.Interface1"));
            Assert.True(usings.ContainsKey("TopLevel.Analyzers.MyDelegates.Foo"));
            Assert.True(usings.ContainsKey("TopLevel.Analyzers.MyDelegates.Structs.Struct1"));
            Assert.True(usings.ContainsKey("TopLevel.Analyzers.My.Records.Record1"));

            foreach (var (_, usingsList) in usings)
            {
                Assert.NotNull(usingsList.SingleOrDefault(model => model.Name == "System"));
                Assert.NotNull(usingsList.SingleOrDefault(model => model.Name == "System.Collections.Generic"));
                Assert.NotNull(usingsList.SingleOrDefault(model => model.Name == "System.Linq"));
                Assert.NotNull(usingsList.SingleOrDefault(model => model.Name == "System.Text"));
                Assert.NotNull(usingsList.SingleOrDefault(model => model.Name == "Microsoft.CodeAnalysis"));
            }

            var interfaceUsings = usings["TopLevel.Analyzers.Interface1"];
            Assert.Equal(5, interfaceUsings.Count);

            var delegateUsings = usings["TopLevel.Analyzers.MyDelegates.Foo"];
            Assert.Equal(6, delegateUsings.Count);
            Assert.Contains(delegateUsings, model => model.Name == "Microsoft.CodeAnalysis.CSharp");

            var structUsings = usings["TopLevel.Analyzers.MyDelegates.Structs.Struct1"];
            Assert.Equal(7, structUsings.Count);
            Assert.Contains(structUsings, model => model.Name == "Microsoft.CodeAnalysis.CSharp");
            Assert.Contains(structUsings, model => model.Name == "MyLib");

            var recordUsings = usings["TopLevel.Analyzers.My.Records.Record1"];
            Assert.Equal(6, recordUsings.Count);
            Assert.Contains(recordUsings, model => model.Name == "MyLib.Records");
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
            var usings = (IDictionary<string, ISet<UsingModel>>) optional.Value;

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
                Assert.NotNull(usingsList.SingleOrDefault(model => model.Name == "System"));
                Assert.NotNull(usingsList.SingleOrDefault(model => model.Name == "System.Collections.Generic"));
                Assert.NotNull(usingsList.SingleOrDefault(model => model.Name == "System.Linq"));
                Assert.NotNull(usingsList.SingleOrDefault(model => model.Name == "System.Text"));
                Assert.Null(usingsList.SingleOrDefault(model => model.Name == "MyLib"));
            }

            Assert.Equal(4, usings["TopLevel.Class1"].Count);
            Assert.Equal(4, usings["TopLevel.Class1.Foo"].Count);

            var class2Usings = usings["TopLevel.Analyzers.Class2"];
            Assert.Equal(5, class2Usings.Count);
            Assert.Contains(class2Usings, model => model.Name == "Microsoft.CodeAnalysis.CSharp");

            var innerClass1Usings = usings["TopLevel.Analyzers.Class2.InnerClass1"];
            Assert.Equal(5, innerClass1Usings.Count);
            Assert.Contains(innerClass1Usings, model => model.Name == "Microsoft.CodeAnalysis.CSharp");

            var innerClass2Usings = usings["TopLevel.Analyzers.Class2.InnerClass1.InnerClass2"];
            Assert.Equal(5, innerClass2Usings.Count);
            Assert.Contains(innerClass2Usings, model => model.Name == "Microsoft.CodeAnalysis.CSharp");

            var interface1Usings = usings["TopLevel.Analyzers.Interface1"];
            Assert.Equal(5, interface1Usings.Count);
            Assert.Contains(interface1Usings, model => model.Name == "Microsoft.CodeAnalysis.CSharp");

            var innerInterfaceUsings = usings["TopLevel.Analyzers.Interface1.IInnerInterface"];
            Assert.Equal(5, innerInterfaceUsings.Count);
            Assert.Contains(innerInterfaceUsings, model => model.Name == "Microsoft.CodeAnalysis.CSharp");
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
            var usings = (IDictionary<string, ISet<UsingModel>>) optional.Value;

            Assert.Equal(3, usings.Count);
            Assert.True(usings.ContainsKey("Class2"));
            Assert.True(usings.ContainsKey("Class2.InnerClass1"));
            Assert.True(usings.ContainsKey("Class2.InnerClass1.InnerClass2"));

            foreach (var (_, usingsList) in usings)
            {
                Assert.Equal(4, usingsList.Count);
                Assert.NotNull(usingsList.SingleOrDefault(model => model.Name == "System"));
                Assert.NotNull(usingsList.SingleOrDefault(model => model.Name == "System.Collections.Generic"));
                Assert.NotNull(usingsList.SingleOrDefault(model => model.Name == "System.Linq"));
                Assert.NotNull(usingsList.SingleOrDefault(model => model.Name == "MyLib"));
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
                Assert.NotNull(classModel.Usings.SingleOrDefault(model => model.Name == "System"));
                Assert.NotNull(classModel.Usings.SingleOrDefault(model => model.Name == "System"));
                Assert.NotNull(classModel.Usings.SingleOrDefault(model => model.Name == "System.Collections.Generic"));
                Assert.NotNull(classModel.Usings.SingleOrDefault(model => model.Name == "System.Linq"));
                Assert.NotNull(classModel.Usings.SingleOrDefault(model => model.Name == "System.Text"));
                Assert.NotNull(classModel.Usings.SingleOrDefault(model => model.Name == "Microsoft.CodeAnalysis"));
            }

            Assert.Equal(5, classModels[0].Usings.Count);

            var structUsings = classModels[1].Usings;
            Assert.Equal(7, structUsings.Count);
            Assert.Contains(structUsings, model => model.Name == "Microsoft.CodeAnalysis.CSharp");
            Assert.Contains(structUsings, model => model.Name == "MyLib");

            var recordUsings = classModels[2].Usings;
            Assert.Equal(6, recordUsings.Count);
            Assert.Contains(recordUsings, model => model.Name == "MyLib.Records");

            var classUsings = classModels[3].Usings;
            Assert.Equal(6, classUsings.Count);
            Assert.Contains(classUsings, model => model.Name == "MyLib.Records");

            var delegateUsings = classModels[4].Usings;
            Assert.Equal(6, delegateUsings.Count);
            Assert.Contains(delegateUsings, model => model.Name == "Microsoft.CodeAnalysis.CSharp");
        }

        [Fact]
        public void Extract_ShouldHaveUsingsInClassModels_WhenGivenStaticUsings()
        {
            const string fileContent = @"
    using System;
    using System.Collections.Generic;
    using static System.Math;

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
                using static Microsoft.CodeAnalysis.CSharp;
            
                public delegate void Foo();
            
                namespace Structs
                {
                    using System.Text;
                    using static MyLib;

                    public struct Struct1{}   
                }
            }

            namespace My.Records
            {
                using static MyLib.Records;
                public class Class1 {
                public record Record1{}}
            }
        }
    }";

            var classModels = _factExtractor.Extract(fileContent);

            Assert.Equal(5, classModels.Count);

            foreach (var classModel in classModels)
            {
                Assert.NotNull(classModel.Usings.SingleOrDefault(model => model.Name == "System" && !model.IsStatic));
                Assert.NotNull(classModel.Usings.SingleOrDefault(model =>
                    model.Name == "System.Collections.Generic" && !model.IsStatic));
                Assert.NotNull(
                    classModel.Usings.SingleOrDefault(model => model.Name == "System.Linq" && !model.IsStatic));
                Assert.NotNull(classModel.Usings.SingleOrDefault(model => model.Name == "System.Text" && !model.IsStatic));
                Assert.NotNull(classModel.Usings.SingleOrDefault(model =>
                    model.Name == "Microsoft.CodeAnalysis" && !model.IsStatic));
                
                Assert.NotNull(classModel.Usings.SingleOrDefault(model => model.Name == "System.Math" && model.IsStatic));
            }

            Assert.Equal(6, classModels[0].Usings.Count);

            var structUsings = classModels[1].Usings;
            Assert.Equal(8, structUsings.Count);
            Assert.Contains(structUsings, model => model.Name == "Microsoft.CodeAnalysis.CSharp" && model.IsStatic);
            Assert.Contains(structUsings, model => model.Name == "MyLib" && model.IsStatic);

            var recordUsings = classModels[2].Usings;
            Assert.Equal(7, recordUsings.Count);
            Assert.Contains(recordUsings, model => model.Name == "MyLib.Records" && model.IsStatic);

            var classUsings = classModels[3].Usings;
            Assert.Equal(7, classUsings.Count);
            Assert.Contains(classUsings, model => model.Name == "MyLib.Records" && model.IsStatic);

            var delegateUsings = classModels[4].Usings;
            Assert.Equal(7, delegateUsings.Count);
            Assert.Contains(delegateUsings, model => model.Name == "Microsoft.CodeAnalysis.CSharp" && model.IsStatic);
        }
    }
}
