namespace Honeydew.Models.VisualBasic;

public static class VisualBasicConstants
{
    public const string DefaultClassAccessModifier = "Friend";

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
}
