using System;

namespace HoneydewCore.Processors
{
    public interface IProcessorFunction<T, TR>
    {
        Func<Processable<T>, Processable<TR>> GetFunction();
    }
}