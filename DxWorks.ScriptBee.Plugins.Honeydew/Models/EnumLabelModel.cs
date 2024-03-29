﻿namespace DxWorks.ScriptBee.Plugins.Honeydew.Models;

public class EnumLabelModel : ReferenceEntity
{
    public string Name { get; set; } = "";

    public IList<AttributeModel> Attributes { get; set; } = new List<AttributeModel>();
}
