using Honeydew.Models.Types;

namespace Honeydew.Models.VisualBasic;

public static class VisualBasicFullTypeNameBuilder
{
    public static VisualBasicEntityTypeModel CreateEntityTypeModel(string? name, bool isExternType = false)
    {
        name ??= "";

        try
        {
            return new VisualBasicEntityTypeModel
            {
                Name = name,
                FullType = GetFullType(name),
                IsExtern = isExternType
            };
        }
        catch (Exception)
        {
            return new VisualBasicEntityTypeModel
            {
                Name = name,
                FullType = new GenericType
                {
                    Name = name
                },
                IsExtern = isExternType
            };
        }
    }

    private static GenericType GetFullType(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return new GenericType
            {
                Name = name
            };
        }

        var isNullable = false;

        if (name.EndsWith('?'))
        {
            isNullable = true;
            name = name[..^1];
        }

        ReadOnlySpan<char> span = name;

        var fullType = GetFullType(span);
        fullType.IsNullable = isNullable;

        return fullType;
    }

    private static GenericType GetFullType(ReadOnlySpan<char> name)
    {
        if (!name.Contains('('))
        {
            var trimmedName = name.ToString().Trim();
            var isNullable = false;
            if (trimmedName.EndsWith('?'))
            {
                isNullable = true;
                trimmedName = trimmedName[..^1];
            }

            return new GenericType
            {
                Name = trimmedName.Replace("Of ", ""),
                IsNullable = isNullable
            };
        }

        var genericType = new GenericType
        {
            IsNullable = name[^1] == '?'
        };

        var genericStart = name.IndexOf("(Of");
        var genericEnd = name.LastIndexOf(')');

        genericType.Name = name[..genericStart].ToString().Trim();


        var commaIndices = new List<int>
        {
            genericStart
        };

        var angleBracketCount = 0;

        for (var i = genericStart + 1; i < genericEnd; i++)
        {
            switch (name[i])
            {
                case '(':
                    angleBracketCount++;
                    break;
                case ')':
                    angleBracketCount--;
                    break;
                case ',':
                {
                    if (angleBracketCount == 0)
                    {
                        commaIndices.Add(i);
                    }

                    break;
                }
            }
        }

        commaIndices.Add(genericEnd);

        for (var i = 0; i < commaIndices.Count - 1; i++)
        {
            var part = name.Slice(commaIndices[i] + 1, commaIndices[i + 1] - commaIndices[i] - 1);
            genericType.ContainedTypes.Add(GetFullType(part));
        }

        return genericType;
    }
}
