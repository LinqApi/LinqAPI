namespace LinqApi.Localization
{
    using System.Text.RegularExpressions;

    namespace LinqApi.Localization.Extensions
    {
        public static class SlugHelper
        {
            public static string Slugify(string text)
            {
                if (string.IsNullOrWhiteSpace(text)) return "";

                var replacements = new Dictionary<char, string>
                {
                    ['ç'] = "c",
                    ['Ç'] = "c",
                    ['ğ'] = "g",
                    ['Ğ'] = "g",
                    ['ı'] = "i",
                    ['İ'] = "i",
                    ['ö'] = "o",
                    ['Ö'] = "o",
                    ['ş'] = "s",
                    ['Ş'] = "s",
                    ['ü'] = "u",
                    ['Ü'] = "u"
                };

                text = string.Concat(text.Select(c => replacements.ContainsKey(c) ? replacements[c] : c.ToString()));
                text = Regex.Replace(text.ToLowerInvariant(), @"[^a-z0-9\s-]", "");
                text = Regex.Replace(text, @"\s+", "-").Trim('-');

                return text;
            }

            public static string SlugifyWithTitle(string text)
            {
                var slug = Slugify(text);
                var shortGuid = Guid.NewGuid().ToString("N")[..6]; // 6 karakterli suffix
                return $"{slug}-{shortGuid}";
            }
        }



    }
}