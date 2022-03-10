using HoneydewScriptBeePlugin.Models;

namespace Honeydew.PostExtraction.ReferenceRelations;

public interface IReferenceModelVisitor
{
    public void Visit(ReferenceEntity referenceEntity);
}
