﻿using System.Collections.Generic;
using HoneydewScriptBeePlugin.Models;

namespace Honeydew.PostExtraction.ReferenceRelations;

public interface IEntityModelVisitor
{
    public string Name { get; }

    public IDictionary<string, int> Visit(EntityModel entityModel);
}