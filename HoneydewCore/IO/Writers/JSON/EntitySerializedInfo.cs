﻿namespace HoneydewCore.IO.Writers.JSON
{
    internal record EntitySerializedInfo
    {
        public int Id { get; init; }
        public string Name { get; init; }
        public string Type { get; init; }
        public int? Container { get; init; }
    }
}