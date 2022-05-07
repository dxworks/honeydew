namespace Honeydew.Models.VisualBasic;

public static class VisualBasicConstants
{
    public const string DefaultClassAccessModifier = "Friend";
    public const string DefaultClassMethodAccessModifier = "Friend";
    public const string DefaultFieldAccessModifier = "Friend";
    public const string DefaultInterfaceMethodAccessModifier = "Public";
    public const string DefaultInterfaceMethodModifier = "MustOverride";

    public const string VarIdentifier = "var";

    private static readonly string[] AccessModifiers =
        { "Protected Friend", "Private Protected", "Public", "Private", "Protected", "Friend" };

    public static void SetModifiers(string allModifiers, ref string accessModifier, ref string modifier)
    {
        foreach (var m in AccessModifiers)
        {
            if (!allModifiers.Contains(m)) continue;

            accessModifier = m;
            modifier = allModifiers.Replace(m, "").Trim();
            return;
        }

        if (string.IsNullOrEmpty(modifier))
        {
            modifier = allModifiers;
        }
    }

    public static bool IsPrimitive(string type)
    {
        if (string.IsNullOrEmpty(type))
        {
            return true;
        }

        type = type.TrimEnd('?');

        type = ConvertSystemTypeToPrimitiveType(type);

        return type is "Byte" or "Char" or "Date" or "Decimal" or "Double" or "Integer" or "Long" or "Object" or "SByte"
            or "Short" or "Single" or "String" or "UInteger" or "ULong" or "UShort" or "Object" or "Void";
    }

    private static string ConvertSystemTypeToPrimitiveType(string type)
    {
        return type switch
        {
            "System.Byte" => "Byte",
            "System.Char" => "Char",
            "System.Date" => "Date",
            "System.Decimal" => "Decimal",
            "System.Double" => "Double",
            "System.Integer" => "Integer",
            "System.Long" => "Long",
            "System.Object" => "Object",
            "System.SByte" => "SByte",
            "System.Short" => "Short",
            "System.Single" => "Single",
            "System.String" => "String",
            "System.UInteger" => "UInteger",
            "System.ULong" => "ULong",
            "System.UShort" => "UShort",
            _ => type
        };
    }
}
