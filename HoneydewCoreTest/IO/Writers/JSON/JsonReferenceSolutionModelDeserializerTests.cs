using System.Linq;
using System.Text.RegularExpressions;
using HoneydewCore.IO.Writers.JSON;
using Xunit;

namespace HoneydewCoreTest.IO.Writers.JSON
{
    public class JsonReferenceSolutionModelDeserializerTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("Not json")]
        [InlineData(@"{""some"":""other""}")]
        [InlineData(@"{""entities"":""other""}")]
        [InlineData(@"{""entities"":""other"", ""model"":{}}")]
        [InlineData(@"{""entities"":[{""Type"":""class""}], ""model"":{}}")]
        public void Deserialize_ShouldReturnEmptyModel_WhenGivenAnInvalidJsonString(string content)
        {
            var referenceSolutionModel = JsonReferenceSolutionModelDeserializer.Deserialize(content);
            Assert.NotNull(referenceSolutionModel);
            Assert.Empty(referenceSolutionModel.Projects);
            Assert.Empty(referenceSolutionModel.GetAllCreatedReferences());
        }
        
        [Fact]
        public void Deserialize_ShouldReturnCorrectReferenceSolutionModel_WhenGivenAJsonString()
        {
            const string jsonContentRaw = @"
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
            ""Name"":""Set_int_int"",
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
            ""Name"":""Set_int_int"",
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
                         ""Name"":""Set_int_int"",
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
                         ""Name"":""Set_int_int"",
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

            var jsonContent = Regex.Replace(jsonContentRaw, @"\s+", "");

            var referenceSolutionModel = JsonReferenceSolutionModelDeserializer.Deserialize(jsonContent);

            var allCreatedReferences = referenceSolutionModel.GetAllCreatedReferences();
            var objectType = allCreatedReferences.First(model => model.Name == "object");
            var intType = allCreatedReferences.First(model => model.Name == "int");

            Assert.Equal("object", objectType.Name);
            Assert.Equal("int", intType.Name);
            
            Assert.Equal(2, referenceSolutionModel.Projects.Count);

            var referenceProjectModel = referenceSolutionModel.Projects[0];
            Assert.Equal("Project1", referenceProjectModel.Name);
            Assert.Equal(referenceSolutionModel, referenceProjectModel.SolutionReference);
            Assert.Equal(2, referenceProjectModel.Namespaces.Count);

            var modelsNamespace = referenceProjectModel.Namespaces[0];
            Assert.Equal("Project1.Models", modelsNamespace.Name);
            Assert.Equal(referenceProjectModel, modelsNamespace.ProjectReference);
            Assert.Equal(3, modelsNamespace.ClassModels.Count);

            var myModelReference = modelsNamespace.ClassModels[0];
            Assert.Equal("Project1.Models.MyModel", myModelReference.Name);
            Assert.Equal(modelsNamespace, myModelReference.NamespaceReference);
            Assert.Equal(objectType, myModelReference.BaseClass);
            Assert.Empty(myModelReference.BaseInterfaces);
            Assert.Equal("", myModelReference.Modifier);
            Assert.Equal("public", myModelReference.AccessModifier);
            Assert.Equal("class", myModelReference.ClassType);
            Assert.Equal("", myModelReference.FilePath);
            Assert.Empty(myModelReference.Methods);
            Assert.Equal(2, myModelReference.Fields.Count);

            Assert.Equal("Value", myModelReference.Fields[0].Name);
            Assert.Equal(myModelReference, myModelReference.Fields[0].ContainingClass);
            Assert.Equal("public", myModelReference.Fields[0].AccessModifier);
            Assert.Equal("", myModelReference.Fields[0].Modifier);
            Assert.Equal(intType, myModelReference.Fields[0].Type);
            Assert.False(myModelReference.Fields[0].IsEvent);

            Assert.Equal("Value2", myModelReference.Fields[1].Name);
            Assert.Equal(myModelReference, myModelReference.Fields[1].ContainingClass);
            Assert.Equal("public", myModelReference.Fields[1].AccessModifier);
            Assert.Equal("", myModelReference.Fields[1].Modifier);
            Assert.Equal(intType, myModelReference.Fields[1].Type);
            Assert.False(myModelReference.Fields[1].IsEvent);

            Assert.Equal(2, myModelReference.Metrics.Count);

            Assert.Equal("SomeExtractor", myModelReference.Metrics[0].ExtractorName);
            Assert.Equal("System.String", myModelReference.Metrics[0].ValueType);
            Assert.Equal("some_metric", myModelReference.Metrics[0].Value);

            Assert.Equal("SomeOtherExtractor", myModelReference.Metrics[1].ExtractorName);
            Assert.Equal("System.Int32", myModelReference.Metrics[1].ValueType);
            Assert.Equal(0, myModelReference.Metrics[1].Value);


            var myChildModelReference = modelsNamespace.ClassModels[1];
            Assert.Equal("Project1.Models.MyChildModel", myChildModelReference.Name);
            Assert.Equal(modelsNamespace, myChildModelReference.NamespaceReference);
            Assert.Equal(myModelReference, myChildModelReference.BaseClass);
            Assert.Empty(myChildModelReference.BaseInterfaces);
            Assert.Equal("", myChildModelReference.Modifier);
            Assert.Equal("public", myChildModelReference.AccessModifier);
            Assert.Equal("class", myChildModelReference.ClassType);
            Assert.Equal("", myChildModelReference.FilePath);
            Assert.Empty(myChildModelReference.Methods);
            Assert.Empty(myChildModelReference.Fields);
            Assert.Empty(myChildModelReference.Metrics);


            var myRecordReference = modelsNamespace.ClassModels[2];
            Assert.Equal("Project1.Models.MyRecord", myRecordReference.Name);
            Assert.Equal(modelsNamespace, myRecordReference.NamespaceReference);
            Assert.Null(myRecordReference.BaseClass);
            Assert.Empty(myRecordReference.BaseInterfaces);
            Assert.Equal("", myRecordReference.Modifier);
            Assert.Equal("public", myRecordReference.AccessModifier);
            Assert.Equal("record", myRecordReference.ClassType);
            Assert.Equal("", myRecordReference.FilePath);
            Assert.Empty(myRecordReference.Methods);
            Assert.Empty(myRecordReference.Fields);
            Assert.Empty(myRecordReference.Metrics);


            var servicesNamespace = referenceProjectModel.Namespaces[1];
            Assert.Equal("Project1.Services", servicesNamespace.Name);
            Assert.Equal(referenceProjectModel, servicesNamespace.ProjectReference);


            var iService1Reference = servicesNamespace.ClassModels[0];
            Assert.Equal("Project1.Services.IService1", iService1Reference.Name);
            Assert.Equal(servicesNamespace, iService1Reference.NamespaceReference);
            Assert.Null(iService1Reference.BaseClass);
            Assert.Empty(iService1Reference.BaseInterfaces);
            Assert.Equal("", iService1Reference.Modifier);
            Assert.Equal("public", iService1Reference.AccessModifier);
            Assert.Equal("interface", iService1Reference.ClassType);
            Assert.Equal("", iService1Reference.FilePath);
            Assert.Empty(iService1Reference.Methods);
            Assert.Empty(iService1Reference.Fields);
            Assert.Empty(iService1Reference.Metrics);


            var iServiceReference = servicesNamespace.ClassModels[1];
            Assert.Equal("Project1.Services.IService", iServiceReference.Name);
            Assert.Equal(servicesNamespace, iServiceReference.NamespaceReference);
            Assert.Null(iServiceReference.BaseClass);
            Assert.Empty(iServiceReference.BaseInterfaces);
            Assert.Equal("", iServiceReference.Modifier);
            Assert.Equal("public", iServiceReference.AccessModifier);
            Assert.Equal("interface", iServiceReference.ClassType);
            Assert.Equal("", iServiceReference.FilePath);
            Assert.Empty(iServiceReference.Fields);
            Assert.Empty(iServiceReference.Metrics);
            Assert.Equal(2, iServiceReference.Methods.Count);

            var getMethodInterface = iServiceReference.Methods[0];
            Assert.Equal("Get", getMethodInterface.Name);
            Assert.Equal(iServiceReference, getMethodInterface.ContainingClass);
            Assert.Equal("abstract", getMethodInterface.Modifier);
            Assert.Equal("public", getMethodInterface.AccessModifier);
            Assert.Equal(myModelReference, getMethodInterface.ReturnTypeReferenceClassModel);
            Assert.Empty(getMethodInterface.ParameterTypes);
            Assert.Empty(getMethodInterface.CalledMethods);

            var setMethodInterface = iServiceReference.Methods[1];
            Assert.Equal("Set", setMethodInterface.Name);
            Assert.Equal(iServiceReference, setMethodInterface.ContainingClass);
            Assert.Equal("abstract", setMethodInterface.Modifier);
            Assert.Equal("public", setMethodInterface.AccessModifier);
            Assert.Equal(myModelReference, setMethodInterface.ReturnTypeReferenceClassModel);
            Assert.Empty(setMethodInterface.CalledMethods);
            Assert.Equal(2, setMethodInterface.ParameterTypes.Count);
            Assert.Equal(intType, setMethodInterface.ParameterTypes[0]);
            Assert.Equal(intType, setMethodInterface.ParameterTypes[1]);

            var myServiceReference = servicesNamespace.ClassModels[2];
            Assert.Equal("Project1.Services.MyService", myServiceReference.Name);
            Assert.Equal(servicesNamespace, myServiceReference.NamespaceReference);
            Assert.Equal(objectType, myServiceReference.BaseClass);
            Assert.Equal(2, myServiceReference.BaseInterfaces.Count);
            Assert.Equal(iService1Reference, myServiceReference.BaseInterfaces[0]);
            Assert.Equal(iServiceReference, myServiceReference.BaseInterfaces[1]);
            Assert.Equal("", myServiceReference.Modifier);
            Assert.Equal("public", myServiceReference.AccessModifier);
            Assert.Equal("class", myServiceReference.ClassType);
            Assert.Equal("", myServiceReference.FilePath);
            Assert.Empty(myServiceReference.Fields);
            Assert.Empty(myServiceReference.Metrics);
            Assert.Equal(2, myServiceReference.Methods.Count);

            var setMethodClass = myServiceReference.Methods[1];
            Assert.Equal("Set", setMethodClass.Name);
            Assert.Equal(myServiceReference, setMethodClass.ContainingClass);
            Assert.Equal("", setMethodClass.Modifier);
            Assert.Equal("public", setMethodClass.AccessModifier);
            Assert.Equal(myModelReference, setMethodClass.ReturnTypeReferenceClassModel);
            Assert.Empty(setMethodClass.CalledMethods);
            Assert.Equal(2, setMethodClass.ParameterTypes.Count);
            Assert.Equal(intType, setMethodClass.ParameterTypes[0]);
            Assert.Equal(intType, setMethodClass.ParameterTypes[1]);

            var getMethodClass = myServiceReference.Methods[0];
            Assert.Equal("Get", getMethodClass.Name);
            Assert.Equal(myServiceReference, getMethodClass.ContainingClass);
            Assert.Equal("", getMethodClass.Modifier);
            Assert.Equal("public", getMethodClass.AccessModifier);
            Assert.Equal(myModelReference, getMethodClass.ReturnTypeReferenceClassModel);
            Assert.Empty(getMethodClass.ParameterTypes);
            Assert.Equal(2, getMethodClass.CalledMethods.Count);
            Assert.Equal(setMethodClass, getMethodClass.CalledMethods[0]);
            Assert.Equal(setMethodClass, getMethodClass.CalledMethods[1]);


            Assert.Equal("Project2", referenceSolutionModel.Projects[1].Name);
            Assert.Equal(referenceSolutionModel, referenceSolutionModel.Projects[1].SolutionReference);
            Assert.Empty(referenceSolutionModel.Projects[1].Namespaces);
        }
    }
}