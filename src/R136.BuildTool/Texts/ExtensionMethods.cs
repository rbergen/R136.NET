using R136.Entities.General;
using System.Collections.Generic;
using System.Linq;

namespace R136.BuildTool.Texts
{
    static class ExtensionMethods
    {
        public static IEnumerable<TypedTextsMap<int>.Initializer>? ToInitializers(this TypeTexts typeTexts)
            => typeTexts.Texts?.Select(pair => new TypedTextsMap<int>.Initializer()
        {
            Type = typeTexts.Type,
            ID = pair.Key,
            Texts = pair.Value
        });

        public static IEnumerable<TypedTextsMap<int>.Initializer>? ToInitializers(this IEnumerable<TypeTexts>? typeTextsSet)
            => typeTextsSet?.SelectMany(typeTexts => typeTexts.ToInitializers() ?? Enumerable.Empty<TypedTextsMap<int>.Initializer>());
    }
}
