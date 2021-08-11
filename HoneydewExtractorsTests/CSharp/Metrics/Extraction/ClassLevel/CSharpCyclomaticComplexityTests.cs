using HoneydewExtractors.CSharp.Metrics;
using HoneydewExtractors.CSharp.Metrics.Extraction.ClassLevel;
using Xunit;

namespace HoneydewExtractorsTests.CSharp.Metrics.Extraction.ClassLevel
{
    // todo add tests for ?:
    // todo add tests for ?.
    // todo add tests for ??
    // todo add tests for ??=
    // todo add tests for is and not or
    public class CSharpCyclomaticComplexityTests
    {
        private readonly CSharpFactExtractor _factExtractor;

        public CSharpCyclomaticComplexityTests()
        {
            _factExtractor = new CSharpFactExtractor();
            _factExtractor.AddMetric<CSharpFieldsInfoMetric>();
            _factExtractor.AddMetric<CSharpMethodInfoMetric>();
        }

        [Fact]
        public void Extract_ShouldHave0CyclomaticComplexity_WhenGivenClassWithMethodsAndPropertiesAndDelegate()
        {
            const string fileContent = @"class MyClass
{
    delegate void A();

    public int Value { get; set; }

    public MyClass(int a)
    {
    }
    
    public void Function()
    {
    }
}";

            var classModels = _factExtractor.Extract(fileContent);

            Assert.Equal(2, classModels.Count);

            Assert.Equal(0, classModels[0].Constructors[0].CyclomaticComplexity);
            Assert.Equal(0, classModels[0].Methods[0].CyclomaticComplexity);
            Assert.Equal(0, classModels[0].Properties[0].CyclomaticComplexity);

            Assert.Equal(0, classModels[1].Methods[0].CyclomaticComplexity);
        }

        [Fact]
        public void
            Extract_ShouldCountCyclomaticComplexityFromWhiles_WhenGivenClassWithMethodsAndPropertiesAndDelegate()
        {
            const string fileContent = @"class MyClass
{
    delegate void A();

    public int Value
    {
        get
        {
            int x = 0;
            while (x < 51)
            {
                x++;
            }

            throw new System.NotImplementedException();
        }
        set
        {
            int z = 10;
            while (true)
            {
                z++;
                break;
            }
            throw new System.NotImplementedException();
        }
    }

    public MyClass(int a)
    {
        while (true)
        {
            break;
        }
    }

    public void Function()
    {
        int i = 0;
        while (i < 5)
        {
            i++;
        }

        while (i % 2 == 0 && i < 10)
        {
            i--;
        }
    }
}";

            var classModels = _factExtractor.Extract(fileContent);

            Assert.Equal(2, classModels.Count);

            Assert.Equal(1, classModels[0].Constructors[0].CyclomaticComplexity);
            Assert.Equal(3, classModels[0].Methods[0].CyclomaticComplexity);
            Assert.Equal(2, classModels[0].Properties[0].CyclomaticComplexity);

            Assert.Equal(0, classModels[1].Methods[0].CyclomaticComplexity);
        }
        
        [Fact]
        public void
            Extract_ShouldCountCyclomaticComplexityFromIfs_WhenGivenClassWithMethodsAndProperties()
        {
            const string fileContent = @"class MyClass
{    
    public int Value
    {
        get
        {
            int x = 0;
            if (x < 51)
            {
                x++;
            }

            throw new System.NotImplementedException();
        }
        set
        {
            int z = 10;
            if (true)
            {
                z++;
                break;
            }
            throw new System.NotImplementedException();
        }
    }

    public MyClass(int a)
    {
        if (a > 0)
        {
            Function();
        }
        else if (a >5)    Function();
    }

    public void Function()
    {
        int i = 0;
        if (i < 5)
        {
            i++;
        }

        if (i % 2 == 0 && i < 10)
        {
            i--;
        }
    }
}";

            var classModels = _factExtractor.Extract(fileContent);

            Assert.Equal(1, classModels.Count);

            Assert.Equal(2, classModels[0].Constructors[0].CyclomaticComplexity);
            Assert.Equal(3, classModels[0].Methods[0].CyclomaticComplexity);
            Assert.Equal(2, classModels[0].Properties[0].CyclomaticComplexity);
        }
        
        [Fact]
        public void
            Extract_ShouldCountCyclomaticComplexityFromFors_WhenGivenClassWithMethods()
        {
            const string fileContent = @"class MyClass
{    
    public void Function(int a)
    {
        var sum = 0;
        for (var i = 0; i < a; i++)
        {
            sum += i;
        }

        for (var i = 0; i < sum && a < sum; i++)
        {
            sum--;
        }

        for (int i = 0; ; i++)
        {
            break;
        }
    }
}";

            var classModels = _factExtractor.Extract(fileContent);

            Assert.Equal(1, classModels.Count);
            
            Assert.Equal(4, classModels[0].Methods[0].CyclomaticComplexity);
        }
        
        [Fact]
        public void
            Extract_ShouldCountCyclomaticComplexityFromUnaryExpression_WhenGivenClassWithMethods()
        {
            const string fileContent = @"class MyClass
{    
    public void Function(bool b, bool a)
    {
        while (Is())
        {
            if (!b)
            {
                
            }
            else if (a)
            {
                if (a && b && !Is())
                {
                    
                }
            }
        }
    }

    private bool Is()
    {
        return false;
    }
}";

            var classModels = _factExtractor.Extract(fileContent);

            Assert.Equal(1, classModels.Count);
            
            Assert.Equal(6, classModels[0].Methods[0].CyclomaticComplexity);
        }
        
        [Fact]
        public void
            Extract_ShouldCountCyclomaticComplexityFromComplexBinaryExpression_WhenGivenClassWithMethods()
        {
            const string fileContent = @"class MyClass
{    
    public void Function(int a, int b)
    {
        var s = 0;
        while ((a < 0 && b > 6) || (a > 0 && (s < 6 || s > 8) && b > 7))
        {
            s++;
        }
    }
}";

            var classModels = _factExtractor.Extract(fileContent);

            Assert.Equal(1, classModels.Count);
            
            Assert.Equal(6, classModels[0].Methods[0].CyclomaticComplexity);
        }
    }
}
