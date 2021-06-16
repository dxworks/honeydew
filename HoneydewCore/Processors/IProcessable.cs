namespace HoneydewCore.Processors
{
    public interface IProcessable
    {
        public static Processable<T> Of<T>(T value)
        {
            return new(value);
        }
    }

    public class Processable<T> : IProcessable
    {
        public T Value { get; }

        public Processable() : this(default)
        {
        }

        public Processable(T value)
        {
            Value = value;
        }
    }
}