﻿using System.IO;

namespace HoneydewCore.IO.Writers.Exporters;

public class TextFileExporter
{
    public void Export(string filePath, string content)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(filePath));
        
        File.WriteAllText(filePath, content);
    }
}
