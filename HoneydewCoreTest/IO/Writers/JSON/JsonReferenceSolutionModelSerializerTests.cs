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

        public MyModel() {}

        public MyModel(int a) {}
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
        MyModel Set(int value1, int value=2);
    }
    
    public class MyService : IService1, IService  
    {
        public MyModel Get()
        {
            Set(1,1);
            return Set(0,0);
        }

        public MyModel Set(int value1, int value=2)
        {
            return new MyChildModel{Value=value1};
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
                ValueType = "System.String"
            });

            solutionModel.Projects[0].Namespaces["Project1.Models"].ClassModels[0].Metrics.Add(new ClassMetric
            {
                ExtractorName = "SomeOtherExtractor",
                Value = 0,
                ValueType = "System.Int32"
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
            ""Name"":""MyModel"",
            ""Type"":""constructor"",
            ""Container"":2
        },
        {
            ""Id"":6,
            ""Name"":""MyModel_int"",
            ""Type"":""constructor"",
            ""Container"":2
        },
        {
            ""Id"":7,
            ""Name"":""Project1.Models.MyChildModel"",
            ""Type"":""class"",
            ""Container"":1
        },
        {
            ""Id"":8,
            ""Name"":""Project1.Models.MyRecord"",
            ""Type"":""class"",
            ""Container"":1
        },
        {
            ""Id"":9,
            ""Name"":""Project1.Services"",
            ""Type"":""namespace"",
            ""Container"":0
        },
        {
            ""Id"":10,
            ""Name"":""Project1.Services.IService1"",
            ""Type"":""class"",
            ""Container"":9
        },
        {
            ""Id"":11,
            ""Name"":""Project1.Services.IService"",
            ""Type"":""class"",
            ""Container"":9
        },
        {
            ""Id"":12,
            ""Name"":""Get"",
            ""Type"":""method"",
            ""Container"":11
        },
        {
            ""Id"":13,
            ""Name"":""Set_int_int"",
            ""Type"":""method"",
            ""Container"":11
        },
        {
            ""Id"":14,
            ""Name"":""Project1.Services.MyService"",
            ""Type"":""class"",
            ""Container"":9
        },
        {
            ""Id"":15,
            ""Name"":""Get"",
            ""Type"":""method"",
            ""Container"":14
        },
        {
            ""Id"":16,
            ""Name"":""Set_int_int"",
            ""Type"":""method"",
            ""Container"":14
        },
        {   
            ""Id"":17,
            ""Name"":""Project2"",
            ""Type"":""project"",
            ""Container"":null  
        }, 
        {
            ""Id"":18,
            ""Name"":""object"",
            ""Type"":""other-class"",
            ""Container"":null
        },
        {
            ""Id"":19,
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
                        ""BaseClass"":18,
                        ""BaseInterfaces"":[],
                        ""Fields"":[{
                            ""Name"":""Value"",
                            ""ContainingClass"":2,
                            ""Type"":19,
                            ""Modifier"":"""",
                            ""AccessModifier"":""public"",
                            ""IsEvent"":false
                         },
                         {
                            ""Name"":""Value2"",
                            ""ContainingClass"":2,
                            ""Type"":19,
                            ""Modifier"":"""",
                            ""AccessModifier"":""public"",
                            ""IsEvent"":false
                        }],
                        ""Constructors"":[
                        {
                            ""Name"":""MyModel"",
                            ""IsConstructor"":true,
                            ""ContainingClass"":2,
                            ""Modifier"":"""",
                            ""AccessModifier"":""public"",
                            ""ReturnTypeReferenceClassModel"":null,
                            ""ParameterTypes"":[],
                            ""CalledMethods"":[]
                        },
                        {
                         ""Name"":""MyModel_int"",
                        ""IsConstructor"":true,
                         ""ContainingClass"":2,
                         ""Modifier"":"""",
                         ""AccessModifier"":""public"",
                         ""ReturnTypeReferenceClassModel"":null,
                         ""ParameterTypes"":[ 
                            {
                                ""Type"":19,
                                ""Modifier"":"""",
                                ""DefaultValue"":null
                            }],
                         ""CalledMethods"":[]
                        }
                        ],
                        ""Methods"":[],
                        ""Metrics"":[
                         {
                            ""ExtractorName"":""SomeExtractor"",
                            ""Value"":""some_metric"",
                            ""ValueType"":""System.String""
                        },
                        {
                            ""ExtractorName"":""SomeOtherExtractor"",
                            ""Value"":""0"",
                            ""ValueType"":""System.Int32""
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
                        ""Constructors"":[],
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
                        ""Constructors"":[],
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
                        ""NamespaceReference"":9,
                        ""BaseClass"":null,
                        ""BaseInterfaces"":[],
                        ""Fields"":[],
                        ""Constructors"":[],
                        ""Methods"":[],
                        ""Metrics"":[]                    
                    },
                    {
                        ""Name"":""Project1.Services.IService"",
                        ""ClassType"":""interface"",        
                        ""FilePath"":"""",        
                        ""AccessModifier"":""public"",
                        ""Modifier"":"""",        
                        ""NamespaceReference"":9,
                        ""BaseClass"":null,
                        ""BaseInterfaces"":[],
                        ""Fields"":[],
                        ""Constructors"":[],
                        ""Methods"":[{
                            ""Name"":""Get"",
                            ""IsConstructor"":false,
                            ""ContainingClass"":11,
                            ""Modifier"":""abstract"",
                            ""AccessModifier"":""public"",
                            ""ReturnTypeReferenceClassModel"":2,
                            ""ParameterTypes"":[],
                            ""CalledMethods"":[]
                        },
                        {
                         ""Name"":""Set_int_int"",
                         ""IsConstructor"":false,
                         ""ContainingClass"":11,
                         ""Modifier"":""abstract"",
                         ""AccessModifier"":""public"",
                         ""ReturnTypeReferenceClassModel"":2,
                         ""ParameterTypes"":[ 
                            {
                                ""Type"":19,
                                ""Modifier"":"""",
                                ""DefaultValue"":null
                            },
                            {
                                ""Type"":19,
                                ""Modifier"":"""",
                                ""DefaultValue"":""2""
                            }],
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
                        ""NamespaceReference"":9,
                        ""BaseClass"":18,
                        ""BaseInterfaces"":[10,11],
                        ""Fields"":[],
                        ""Constructors"":[],
                        ""Methods"":[{
                            ""Name"":""Get"",
                            ""IsConstructor"":false,
                            ""ContainingClass"":14,
                            ""Modifier"":"""",
                            ""AccessModifier"":""public"",
                            ""ReturnTypeReferenceClassModel"":2,
                            ""ParameterTypes"":[],
                            ""CalledMethods"":[16,16]
                        },
                        {
                         ""Name"":""Set_int_int"",
                         ""IsConstructor"":false,
                         ""ContainingClass"":14,
                         ""Modifier"":"""",
                         ""AccessModifier"":""public"",
                         ""ReturnTypeReferenceClassModel"":2,
                         ""ParameterTypes"":[
                            {
                                ""Type"":19,
                                ""Modifier"":"""",
                                ""DefaultValue"":null
                            },
                            {
                                ""Type"":19,
                                ""Modifier"":"""",
                                ""DefaultValue"":""2""
                            }],
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