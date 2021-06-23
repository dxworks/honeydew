using System;

namespace HoneydewCore.Processors
{
    public class ProcessorChain
    {
        private IProcessable _processable;

        public ProcessorChain(IProcessable processable)
        {
            _processable = processable;
        }

        public ProcessorChain Process(Func<IProcessable, IProcessable> func)
        {
            _processable = func.Invoke(_processable);
            return this;
        }

        public ProcessorChain Peek<T>(Action<T> action)
        {
            action.Invoke(((Processable<T>) _processable).Value);
            return this;
        }

        public ProcessorChain Process<T, TR>(Func<Processable<T>, Processable<TR>> func)
        {
            _processable = func.Invoke((Processable<T>) _processable);
            return this;
        }

        public ProcessorChain Process<T, TR>(IProcessorFunction<T, TR> func)
        {
            return Process(func.GetFunction());
        }

        public IProcessable Finish()
        {
            return _processable;
        }

        public Processable<T> Finish<T>()
        {
            return (Processable<T>) _processable;
        }
    }
}