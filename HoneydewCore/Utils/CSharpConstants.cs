namespace HoneydewCore.Utils
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
        public const string VarIdentifier = "var";

        private static readonly string[] AccessModifiers =
            {"private protected", "protected internal", "public", "private", "protected", "internal"};

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
            return type is "object" or "string" or "bool" or "byte" or "char" or "decimal" or "double" or "short"
                or "int" or "long" or "sbyte" or "float" or "ushort" or "uint" or "ulong" or "void";
        }
    }
}