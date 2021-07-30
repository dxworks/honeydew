namespace A
{
    namespace B
    {
        class MyClass
        {
            public void M(){}
        }
    }

    namespace C
    {
    }
}

namespace X
{
    namespace Y
    {
        using K = A.B;


        class Z
        {
            public K.MyClass Method(K.MyClass z)
            {
                z.M();

                return z;
            }
        }
    }
}
