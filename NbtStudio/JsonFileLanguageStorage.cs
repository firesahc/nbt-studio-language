using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NbtStudio
{
    /// <summary>
    /// SRP: Responsible solely for reading/writing language translation data from JSON files on disk,
    /// and caching loaded translations. No knowledge of UI, settings, or consumer workflows.
    ///
    /// OCP: New storage backends can be added by implementing ILanguageStorage without modifying this class.
    /// LSP: Implements ILanguageStorage and can be substituted transparently.
    /// </summary>
    internal sealed class JsonFileLanguageStorage : ILanguageStorage
    {
        // Cached translations keyed by language code (case-insensitive).
        private readonly Dictionary<string, IReadOnlyDictionary<string, string>> _cache =
            new(StringComparer.OrdinalIgnoreCase);

        private readonly object _syncLock = new();

        public JsonFileLanguageStorage(string languageDir)
        {
            Location = languageDir ?? throw new ArgumentNullException(nameof(languageDir));
        }

        public string Location { get; }

        public bool TryLoad(string langCode, out IReadOnlyDictionary<string, string> translations)
        {
            if (string.IsNullOrWhiteSpace(langCode))
                throw new ArgumentException("Language code must not be empty.", nameof(langCode));

            lock (_syncLock)
            {
                // Check cache first.
                if (_cache.TryGetValue(langCode, out var cached))
                {
                    translations = cached;
                    return true;
                }

                var result = TryLoadFromFile(langCode, out var loaded);
                if (result)
                    _cache[langCode] = loaded;

                translations = loaded;
                return result;
            }
        }

        public IEnumerable<string> GetAvailableLanguages()
        {
            try
            {
                if (!Directory.Exists(Location))
                    return Array.Empty<string>();

                return Directory.EnumerateFiles(Location, "*.json")
                    .Select(Path.GetFileNameWithoutExtension)
                    .Where(x => !string.IsNullOrEmpty(x));
            }
            catch
            {
                return Array.Empty<string>();
            }
        }

        public void EnsureDefaults(IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> defaults)
        {
            if (defaults is null || defaults.Count == 0)
                return;

            try
            {
                if (!Directory.Exists(Location))
                    Directory.CreateDirectory(Location);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Init language directory failed: {ex.Message}");
                return;
            }

            foreach (var (langCode, translations) in defaults)
            {
                try
                {
                    var filePath = Path.Combine(Location, $"{langCode}.json");
                    if (!File.Exists(filePath))
                    {
                        File.WriteAllText(
                            filePath,
                            JsonConvert.SerializeObject(translations, Formatting.Indented),
                            Encoding.UTF8);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Create default language file failed ({langCode}): {ex.Message}");
                }
            }
        }

        // ---- Private helpers ----

        private bool TryLoadFromFile(string langCode, out IReadOnlyDictionary<string, string> translations)
        {
            translations = null;

            try
            {
                var filePath = Path.Combine(Location, $"{langCode}.json");

                // Path traversal guard: ensure the resolved path is still under Location.
                if (!filePath.StartsWith(Location, StringComparison.OrdinalIgnoreCase) ||
                    !File.Exists(filePath))
                {
                    return false;
                }

                var json = File.ReadAllText(filePath, Encoding.UTF8);
                if (string.IsNullOrWhiteSpace(json))
                {
                    Debug.WriteLine($"Language file empty: {langCode}");
                    return false;
                }

                // Use JObject.Load with CommentHandling.Ignore to support // comments in JSON files.
                var loadSettings = new JsonLoadSettings
                {
                    CommentHandling = CommentHandling.Ignore,
                    LineInfoHandling = LineInfoHandling.Ignore
                };

                using var reader = new JsonTextReader(new StringReader(json)) { CloseInput = true };
                var jObject = JObject.Load(reader, loadSettings);

                var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foreach (var prop in jObject.Properties())
                {
                    // Convert all value types (string, int, bool) to their string representation
                    // to match the Dictionary<string, string> contract.
                    dict[prop.Name] = prop.Value.Type == JTokenType.String
                        ? (string)prop.Value
                        : prop.Value.ToString(Formatting.None, null);
                }

                translations = new ReadOnlyDictionary<string, string>(dict);
                return true;
            }
            catch (Exception ex) when (ex is JsonException || ex is IOException)
            {
                Debug.WriteLine($"Language load failed ({langCode}): {ex.Message}");
                return false;
            }
        }
    }
}
