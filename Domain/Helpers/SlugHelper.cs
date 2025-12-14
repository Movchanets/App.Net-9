using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Domain.Helpers;

public static class SlugHelper
{
	public static string GenerateSlug(string value)
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			throw new ArgumentException("Slug source cannot be empty", nameof(value));
		}

		var lower = value.Trim().ToLowerInvariant();
		var normalized = RemoveDiacritics(lower);
		var cleaned = Regex.Replace(normalized, @"[^a-z0-9\s-]", string.Empty);
		var condensed = Regex.Replace(cleaned, @"\s+", "-");
		condensed = Regex.Replace(condensed, "-{2,}", "-").Trim('-');

		return string.IsNullOrEmpty(condensed) ? "n-a" : condensed;
	}

	public static string RemoveDiacritics(string text)
	{
		var normalized = text.Normalize(NormalizationForm.FormD);
		var builder = new StringBuilder(normalized.Length);

		foreach (var character in normalized)
		{
			var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(character);
			if (unicodeCategory != UnicodeCategory.NonSpacingMark)
			{
				builder.Append(character);
			}
		}

		return builder.ToString().Normalize(NormalizationForm.FormC);
	}
}
