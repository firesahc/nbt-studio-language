using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using NbtStudio.Properties;

namespace NbtStudio
{
    public static class languageManager
    {
        private static Dictionary<string, string> _currentLanguage;
        private static readonly Dictionary<string, Dictionary<string, string>> _languageRegistry = new(StringComparer.OrdinalIgnoreCase);
        private static readonly object _syncLock = new();
        private static string _languageDir;
        private static bool _initialized;

        private static string LanguageDir
        {
            get
            {
                if (_languageDir is null)
                    _languageDir = Path.Combine(Application.StartupPath, "Language");
                return _languageDir;
            }
        }

        private static void EnsureInitialized()
        {
            if (_initialized)
                return;

            lock (_syncLock)
            {
                if (_initialized)
                    return;

                EnsureLanguageDirectory();
                LoadLanguage();
                _initialized = true;
            }
        }

        private static void EnsureLanguageDirectory()
        {
            try
            {
                string dir = LanguageDir;
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                    CreateDefaultLanguageFiles();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Init language directory failed: {ex.Message}");
            }
        }

        private static void CreateDefaultLanguageFiles()
        {
            var defaultLanguages = new Dictionary<string, Dictionary<string, string>>
            {
                ["en-US"] = new()
                {
                    {"MenuFile", "File"},
                    {"MenuEdit", "Edit"},
                    {"MenuSearch", "Find"},
                    {"MenuHelp", "Help"},
                },
                ["zh-CN"] = new()
                {
                    {"MenuFile", "File"},
                    {"MenuEdit", "Edit"},
                    {"MenuSearch", "Find"},
                    {"MenuHelp", "Help"},
                }
            };

            foreach (var (langCode, translations) in defaultLanguages)
            {
                try
                {
                    var filePath = Path.Combine(LanguageDir, $"{langCode}.json");
                    if (!File.Exists(filePath))
                    {
                        File.WriteAllText(
                            filePath,
                            JsonConvert.SerializeObject(translations, Formatting.Indented),
                            Encoding.UTF8
                        );
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Create default language file failed ({langCode}): {ex.Message}");
                }
            }
        }

        public static void LoadLanguage(string langCode = null)
        {
            langCode ??= Settings.Default.Language ?? "en-US";

            lock (_syncLock)
            {
                if (!TryLoadLanguageInternal(langCode) && langCode != "en-US" && !TryLoadLanguageInternal("en-US"))
                {
                    _currentLanguage = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    Debug.WriteLine("Cannot load any language file");
                }
            }
        }

        private static bool TryLoadLanguageInternal(string langCode)
        {
            if (_languageRegistry.TryGetValue(langCode, out var cached))
            {
                _currentLanguage = cached;
                return true;
            }

            try
            {
                var filePath = Path.Combine(LanguageDir, $"{langCode}.json");

                if (!filePath.StartsWith(LanguageDir, StringComparison.OrdinalIgnoreCase) ||
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

                var settings = new JsonSerializerSettings
                {
                    MissingMemberHandling = MissingMemberHandling.Ignore,
                    NullValueHandling = NullValueHandling.Ignore,
                    MaxDepth = 10,
                    Error = (_, args) => args.ErrorContext.Handled = true
                };

                var strings = JsonConvert.DeserializeObject<Dictionary<string, string>>(json, settings);
                var dict = new Dictionary<string, string>(
                    strings ?? new Dictionary<string, string>(),
                    StringComparer.OrdinalIgnoreCase
                );

                _languageRegistry[langCode] = dict;
                _currentLanguage = dict;
                return true;
            }
            catch (Exception ex) when (ex is JsonException || ex is IOException)
            {
                Debug.WriteLine($"Language load failed ({langCode}): {ex.Message}");
                return false;
            }
        }

        public static bool TryLoadLanguage(string langCode)
        {
            lock (_syncLock)
            {
                EnsureInitialized();
                if (TryLoadLanguageInternal(langCode))
                    return true;
                if (langCode != "en-US")
                    return TryLoadLanguageInternal("en-US");
                return _currentLanguage is not null && _currentLanguage.Count > 0;
            }
        }

        public static string GetText(string key, string defaultValue = null, params object[] args)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                Debug.WriteLine($"Invalid localization key: [{key}]");
                return "[INVALID_KEY]";
            }

            if (!_initialized)
                EnsureInitialized();

            Dictionary<string, string> lang;

            lock (_syncLock)
            {
                lang = _currentLanguage;
            }

            if (lang is null)
                return defaultValue ?? key;

            string text = defaultValue ?? key;

            if (!lang.TryGetValue(key, out var translation))
            {
                Debug.WriteLine($"Localization key missing: {key}");
            }
            else if (!string.IsNullOrWhiteSpace(translation))
            {
                text = translation;
            }

            return FormatSafe(text, args, key);
        }

        private static string FormatSafe(string format, object[] args, string key)
        {
            if (args is null || args.Length == 0)
                return format;

            try
            {
                return string.Format(format, args);
            }
            catch (FormatException ex)
            {
                Debug.WriteLine($"Format failed: {key} - {ex.Message}");
                return $"[FORMAT_ERROR:{key}]";
            }
        }

        public static IEnumerable<string> GetAvailableLanguages()
        {
            try
            {
                string dir = LanguageDir;
                if (!Directory.Exists(dir))
                    return Array.Empty<string>();

                return Directory.EnumerateFiles(dir, "*.json")
                    .Select(Path.GetFileNameWithoutExtension)
                    .Where(x => !string.IsNullOrEmpty(x));
            }
            catch
            {
                return Array.Empty<string>();
            }
        }
    }
}
