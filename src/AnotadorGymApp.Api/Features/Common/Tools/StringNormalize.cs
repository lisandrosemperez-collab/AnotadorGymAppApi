using System.Globalization;
using System.Text;

namespace AnotadorGymAppApi.Features.Common.Tools
{
    public class StringNormalize
    {
        public static string Normalize(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            // pasar a minúsculas
            input = input.Trim().ToLowerInvariant();

            // quitar tildes / acentos
            var normalized = input.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();

            foreach (var c in normalized)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);

                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(c);
                }
            }

            var result = sb.ToString().Normalize(NormalizationForm.FormC);

            // limpiar espacios múltiples
            result = string.Join(" ", result.Split(' ', StringSplitOptions.RemoveEmptyEntries));

            return result;
        }
    }
}
