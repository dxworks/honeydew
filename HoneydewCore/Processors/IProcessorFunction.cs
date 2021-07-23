namespace HoneydewCore.Processors
{
    public interface IProcessorFunction<in T, out TR>
    {
        TR Process(T input);
    }
}
