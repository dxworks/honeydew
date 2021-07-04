using System.Text.RegularExpressions;
using HoneydewCore.Extractors;
using HoneydewCore.IO.Writers.JSON;
using HoneydewCore.Models;
using HoneydewCore.Models.Representations.ReferenceModel;
using HoneydewCore.Processors;
using Xunit;

namespace HoneydewCoreTest.IO.Writers.JSON
{
    public class JsonReferenceSolutionModelSerializerTests
    {
        [Fact]
        public void Serialize_ShouldReturnCorrectString_WhenGivenAReferenceSolutionModel()
        {
            const string modelsNamespaceFileContent = @"

namespace Project1.Models
{
    public class MyModel {
        public int Value;
        public int Value2;   
    }
    
    public class MyChildModel : MyModel { }
    
    public record MyRecord {}
}";

            const string servicesNamespaceFileContent = @"
using Project1.Models;
namespace Project1.Services
{
    public interface IService1{}

    public interface IService 
    {
        MyModel Get();
        MyModel Set(int value, int value);
    }
    
    public class MyService : IService1, IService  
    {
        public MyModel Get()
        {
            Set(1,1);
            return Set(0,0);
        }

        public MyModel Set(int value, int value)
        {
            return new MyChildModel{Value=value};
        }
    }
}";
            var extractor = new CSharpClassFactExtractor();

            var solutionModel = new SolutionModel
            {
                Projects =
                {
                    new ProjectModel
                    {
                        Name = "Project1",
                        Namespaces =
                        {
                            {
                                "Project1.Models", new NamespaceModel
                                {
                                    Name = "Project1.Models",
                                    ClassModels = extractor.Extract(modelsNamespaceFileContent),
                                }
                            },
                            {
                                "Project1.Services", new NamespaceModel
                                {
                                    Name = "Project1.Services",
                                    ClassModels = extractor.Extract(servicesNamespaceFileContent),
                                }
                            }
                        }
                    },
                    new ProjectModel
                    {
                        Name = "Project2"
                    },
                }
            };

            solutionModel.Projects[0].Namespaces["Project1.Models"].ClassModels[0].Metrics.Add(new ClassMetric
            {
                ExtractorName = "SomeExtractor",
                Value = "some_metric",
                ValueType = "string"
            });
            
            solutionModel.Projects[0].Namespaces["Project1.Models"].ClassModels[0].Metrics.Add(new ClassMetric
            {
                ExtractorName = "SomeOtherExtractor",
                Value = 0,
                ValueType = "int"
            });

            var referenceSolutionModel = new ProcessorChain(IProcessable.Of(solutionModel))
                .Process(new FullNameModelProcessor())
                .Process(new SolutionModelToReferenceSolutionModelProcessor())
                .Finish<ReferenceSolutionModel>().Value;


            const string expectedStringWithWhitespaces = @"
{
    ""entities"":[
        {   
            ""Id"":0,
            ""Name"":""Project1"",
            ""Type"":""project"",
            ""Container"":null  
        }, 
        {
            ""Id"":1,
            ""Name"":""Project1.Models"",
            ""Type"":""namespace"",
            ""Container"":0
        },
        {
            ""Id"":2,
            ""Name"":""Project1.Models.MyModel"",
            ""Type"":""class"",
            ""Container"":1
        },
        {
            ""Id"":3,
            ""Name"":""Value"",
            ""Type"":""field"",
            ""Container"":2
        },
        {
            ""Id"":4,
            ""Name"":""Value2"",
            ""Type"":""field"",
            ""Container"":2
        },
        {
            ""Id"":5,
            ""Name"":""Project1.Models.MyChildModel"",
            ""Type"":""class"",
            ""Container"":1
        },
        {
            ""Id"":6,
            ""Name"":""Project1.Models.MyRecord"",
            ""Type"":""class"",
            ""Container"":1
        },
        {
            ""Id"":7,
            ""Name"":""Project1.Services"",
            ""Type"":""namespace"",
            ""Container"":0
        },
        {
            ""Id"":8,
            ""Name"":""Project1.Services.IService1"",
            ""Type"":""class"",
            ""Container"":7
        },
        {
            ""Id"":9,
            ""Name"":""Project1.Services.IService"",
            ""Type"":""class"",
            ""Container"":7
        },
        {
            ""Id"":10,
            ""Name"":""Get"",
            ""Type"":""method"",
            ""Container"":9
        },
        {
            ""Id"":11,
            ""Name"":""Set"",
            ""Type"":""method"",
            ""Container"":9
        },
        {
            ""Id"":12,
            ""Name"":""Project1.Services.MyService"",
            ""Type"":""class"",
            ""Container"":7
        },
        {
            ""Id"":13,
            ""Name"":""Get"",
            ""Type"":""method"",
            ""Container"":12
        },
        {
            ""Id"":14,
            ""Name"":""Set"",
            ""Type"":""method"",
            ""Container"":12
        },
        {   
            ""Id"":15,
            ""Name"":""Project2"",
            ""Type"":""project"",
            ""Container"":null  
        }, 
        {
            ""Id"":16,
            ""Name"":""object"",
            ""Type"":""other-class"",
            ""Container"":null
        },
        {
            ""Id"":17,
            ""Name"":""int"",
            ""Type"":""other-class"",
            ""Container"":null
        }
    ],
    ""model"": 
    {
        ""Projects"" : [{
            ""Name"":""Project1"",
            ""Namespaces"":[ 
                {
                    ""Name"":""Project1.Models"",
                    ""ReferenceProjectModel"":0,
                    ""ClassModels"":[
                    {
                        ""Name"":""Project1.Models.MyModel"",
                        ""ClassType"":""class"",        
                        ""FilePath"":"""",        
                        ""AccessModifier"":""public"",
                        ""Modifier"":"""",        
                        ""NamespaceReference"":1,
                        ""BaseClass"":16,
                        ""BaseInterfaces"":[],
                        ""Fields"":[{
                            ""Name"":""Value"",
                            ""ContainingClass"":2,
                            ""Type"":17,
                            ""Modifier"":"""",
                            ""AccessModifier"":""public"",
                            ""IsEvent"":false
                        },{
                            ""Name"":""Value2"",
                            ""ContainingClass"":2,
                            ""Type"":17,
                            ""Modifier"":"""",
                            ""AccessModifier"":""public"",
                            ""IsEvent"":false
                        }],
                        ""Methods"":[],
                        ""Metrics"":[
                         {
                            ""ExtractorName"":""SomeExtractor"",
                            ""Value"":""some_metric"",
                            ""ValueType"":""string""
                        },
                        {
                            ""ExtractorName"":""SomeOtherExtractor"",
                            ""Value"":""0"",
                            ""ValueType"":""int""
                        }
                        ]                    
                    },
                     {
                        ""Name"":""Project1.Models.MyChildModel"",
                        ""ClassType"":""class"",        
                        ""FilePath"":"""",        
                        ""AccessModifier"":""public"",
                        ""Modifier"":"""",        
                        ""NamespaceReference"":1,
                        ""BaseClass"":2,
                        ""BaseInterfaces"":[],
                        ""Fields"":[],
                        ""Methods"":[],
                        ""Metrics"":[]                    
                    },
                    {
                        ""Name"":""Project1.Models.MyRecord"",
                        ""ClassType"":""record"",        
                        ""FilePath"":"""",        
                        ""AccessModifier"":""public"",
                        ""Modifier"":"""",        
                        ""NamespaceReference"":1,
                        ""BaseClass"":null,
                        ""BaseInterfaces"":[],
                        ""Fields"":[],
                        ""Methods"":[],
                        ""Metrics"":[]                    
                    }
                    ]
                },
                {
                    ""Name"":""Project1.Services"",
                    ""ReferenceProjectModel"":0,
                    ""ClassModels"":[
                    {
                        ""Name"":""Project1.Services.IService1"",
                        ""ClassType"":""interface"",        
                        ""FilePath"":"""",        
                        ""AccessModifier"":""public"",
                        ""Modifier"":"""",        
                        ""NamespaceReference"":7,
                        ""BaseClass"":null,
                        ""BaseInterfaces"":[],
                        ""Fields"":[],
                        ""Methods"":[],
                        ""Metrics"":[]                    
                    },
                    {
                        ""Name"":""Project1.Services.IService"",
                        ""ClassType"":""interface"",        
                        ""FilePath"":"""",        
                        ""AccessModifier"":""public"",
                        ""Modifier"":"""",        
                        ""NamespaceReference"":7,
                        ""BaseClass"":null,
                        ""BaseInterfaces"":[],
                        ""Fields"":[],
                        ""Methods"":[{
                            ""Name"":""Get"",
                            ""ContainingClass"":9,
                            ""Modifier"":""abstract"",
                            ""AccessModifier"":""public"",
                            ""ReturnTypeReferenceClassModel"":2,
                            ""ParameterTypes"":[],
                            ""CalledMethods"":[]
                        },
                        {
                         ""Name"":""Set"",
                         ""ContainingClass"":9,
                         ""Modifier"":""abstract"",
                         ""AccessModifier"":""public"",
                         ""ReturnTypeReferenceClassModel"":2,
                         ""ParameterTypes"":[17,17],
                         ""CalledMethods"":[]
                        }],
                        ""Metrics"":[]                    
                    },
                     {
                        ""Name"":""Project1.Services.MyService"",
                        ""ClassType"":""class"",        
                        ""FilePath"":"""",        
                        ""AccessModifier"":""public"",
                        ""Modifier"":"""",        
                        ""NamespaceReference"":7,
                        ""BaseClass"":16,
                        ""BaseInterfaces"":[8,9],
                        ""Fields"":[],
                        ""Methods"":[{
                            ""Name"":""Get"",
                            ""ContainingClass"":12,
                            ""Modifier"":"""",
                            ""AccessModifier"":""public"",
                            ""ReturnTypeReferenceClassModel"":2,
                            ""ParameterTypes"":[],
                            ""CalledMethods"":[14,14]
                        },
                        {
                         ""Name"":""Set"",
                         ""ContainingClass"":12,
                         ""Modifier"":"""",
                         ""AccessModifier"":""public"",
                         ""ReturnTypeReferenceClassModel"":2,
                         ""ParameterTypes"":[17,17],
                         ""CalledMethods"":[]
                        }],
                        ""Metrics"":[]                    
                    }
                    ]
                }
            ]
        },
        {
        ""Name"":""Project2"",
        ""Namespaces"":[]
        }]
    }
}
";

            var expectedString = Regex.Replace(expectedStringWithWhitespaces, @"\s+", "");

            var actualString = JsonReferenceSolutionModelSerializer.Serialize(referenceSolutionModel);

            Assert.Equal(expectedString, actualString);
        }
    }
}