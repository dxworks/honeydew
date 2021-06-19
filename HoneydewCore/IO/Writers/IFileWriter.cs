namespace HoneydewCore.IO.Writers
{
    public interface IFileWriter
    {
        void WriteFile(string filePath, string fileContent);
    }
}