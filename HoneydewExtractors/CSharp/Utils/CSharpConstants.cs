namespace HoneydewExtractors.CSharp.Utils
{
    public static class CSharpConstants
    {
        public const string DefaultClassAccessModifier = "private";
        public const string DefaultFieldAccessModifier = "private";
        public const string DefaultClassMethodAccessModifier = "private";
        public const string DefaultInterfaceMethodAccessModifier = "public";
        public const string DefaultInterfaceMethodModifier = "abstract";

        public const string AbstractIdentifier = "abstract";
        public const string BaseClassIdentifier = "base";
        public const string ObjectIdentifier = "object";
        public const string ClassIdentifier = "class";
        public const string DelegateIdentifier = "delegate";
        public const string SystemDelegate = "System.Delegate";
        public const string SystemObject = "System.Object";
        public const string VarIdentifier = "var";
        public const string RefIdentifier = "ref";
        public const string RefReadonlyIdentifier = "ref readonly";

        private static readonly string[] AccessModifiers =
            { "private protected", "protected internal", "public", "private", "protected", "internal" };


        public static void SetModifiers(string allModifiers, ref string accessModifier, ref string modifier)
        {
            foreach (var m in AccessModifiers)
            {
                if (!allModifiers.Contains(m)) continue;

                accessModifier = m;
                modifier = allModifiers.Replace(m, "").Trim();
                break;
            }
        }

        public static bool IsPrimitive(string type)
        {
            type = ConvertSystemTypeToPrimitiveType(type);

            if (type.EndsWith('?'))
            {
                type = type[..^1];
            }
            
            return type is "object" or "string" or "bool" or "byte" or "char" or "decimal" or "double" or "short"or "int" or "long" or "sbyte" or "float" or "ushort" or "uint" or "ulong" or "void";
        }

        private static string ConvertSystemTypeToPrimitiveType(string type)
        {
            return type switch
            {
                "System.Byte" => "byte",
                "System.SByte" => "sbyte",
                "System.Int32" => "int",
                "System.UInt32" => "uint",
                "System.Int16" => "short",
                "System.UInt16" => "ushort",
                "System.Int64" => "long",
                "System.UInt64" => "ulong",
                "System.Single" => "float",
                "System.Double" => "double",
                "System.Char" => "char",
                "System.Boolean" => "bool",
                "System.Object" => "object",
                "System.String" => "string",
                "System.Decimal" => "decimal",
                "System.DateTime" => "DateTime",
                _ => type
            };
        }

        public static string ConvertPrimitiveTypeToSystemType(string type)
        {
            return type switch
            {
                "byte" => "System.Byte",
                "sbyte" => "System.SByte",
                "int" => "System.Int32",
                "uint" => "System.UInt32",
                "short" => "System.Int16",
                "ushort" => "System.UInt16",
                "long" => "System.Int64",
                "ulong" => "System.UInt64",
                "float" => "System.Single",
                "double" => "System.Double",
                "char" => "System.Char",
                "bool" => "System.Boolean",
                "object" => "System.Object",
                "string" => "System.String",
                "decimal" => "System.Decimal",
                "DateTime" => "System.DateTime",
                _ => type
            };
        }
    }
}
