using HoneydewCore.Extractors;
using HoneydewExtractors.Metrics;
using HoneydewExtractors.Metrics.Extraction.ClassLevel.CSharp;
using HoneydewExtractors.Metrics.Extraction.CompilationUnitLevel.CSharp;
using Xunit;

namespace HoneydewExtractorsTests.Metrics
{
    public class CSharpMetricsTests // tests with multiple metrics
    {
//         private CSharpClassFactExtractor _factExtractor;
//
//         [Fact]
//         public void
//             Extract_ShouldHaveUsingsCountMetricAndCorrectClassNamesWithNamespaces_WhenGivenMultipleUsingsAtMultipleLevels()
//         {
//             const string fileContent = @"using System;
//                                     using System.Collections.Generic;
//                                     using System.Linq;
//                                     using Microsoft.CodeAnalysis.CSharp;
//
//                                     namespace TopLevel
//                                     {
//                                         using Microsoft;
//                                         using System.ComponentModel;
//
//                                         namespace Child1
//                                         {
//                                             using Microsoft.Win32;
//                                             using System.Runtime.InteropServices;
//
//                                             public class Foo { }
//                                         }
//
//                                         namespace Child2
//                                         {
//                                             using System.CodeDom;
//                                             using Microsoft.CSharp;
//
//                                             public class Bar { public void b(){} }
//                                         }
//                                     }";
//
//             _factExtractor = new CSharpClassFactExtractor();
//             _factExtractor.AddMetric<CSharpUsingsCountMetric>();
//
//             var classModels = _factExtractor.Extract(fileContent);
//
//             Assert.Equal(2, classModels.Count);
//
//             var foo = classModels[0];
//             Assert.Equal("TopLevel.Child1.Foo", foo.FullName);
//             Assert.Equal("TopLevel.Child1", foo.Namespace);
//             Assert.Equal(1, foo.Metrics.Count);
//             var optional1 = foo.GetMetricValue<CSharpUsingsCountMetric>();
//             Assert.True(optional1.HasValue);
//             Assert.Equal(10, (int) optional1.Value);
//
//
//             var bar = classModels[1];
//             Assert.Equal("TopLevel.Child2.Bar", bar.FullName);
//             Assert.Equal("TopLevel.Child2", bar.Namespace);
//             Assert.Equal(1, bar.Metrics.Count);
//             var optional2 = bar.GetMetricValue<CSharpUsingsCountMetric>();
//             Assert.True(optional2.HasValue);
//             Assert.Equal(10, (int) optional2.Value);
//         }
//
//         [Fact]
//         public void Extract_ShouldNotHaveMetric_WhenGivenWrongMetricName()
//         {
//             const string fileContent = @"using System;
//                                     using System.Collections.Generic;
//                                     using System.Linq;
//                                     using Microsoft.CodeAnalysis.CSharp;
//
//                                     namespace TopLevel
//                                     {
//                                         using Microsoft;
//                                         using System.ComponentModel;
//
//                                         namespace Child1
//                                         {
//                                             using Microsoft.Win32;
//                                             using System.Runtime.InteropServices;
//
//                                             public class Foo { }
//                                         }
//
//                                         namespace Child2
//                                         {
//                                             using System.CodeDom;
//                                             using Microsoft.CSharp;
//
//                                             public class Bar { public void b(){} }
//                                         }
//                                     }";
//
//             _factExtractor = new CSharpClassFactExtractor();
//             _factExtractor.AddMetric<CSharpBaseClassMetric>();
//             _factExtractor.AddMetric<CSharpUsingsCountMetric>();
//
//             var classModels = _factExtractor.Extract(fileContent);
//
//             Assert.Equal(2, classModels.Count);
//             var optional = classModels[0].GetMetricValue<IMetric>();
//             Assert.False(optional.HasValue);
//
//             var optional1 = classModels[1].GetMetricValue<IMetric>();
//             Assert.False(optional1.HasValue);
//         }
     }
}
