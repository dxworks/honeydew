﻿namespace HoneydewModels.CSharp.ReferenceModel
{
    public record ReferenceParameterModel
    {
        public ReferenceClassModel Type { get; init; }

        public string Modifier { get; init; } = "";

        public string DefaultValue { get; init; }
    }
}