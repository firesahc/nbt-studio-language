using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using NbtStudio.Properties;

namespace NbtStudio
{
    /// <summary>
    /// Static facade that provides localized text and language switching.
    ///
    /// SRP: This class is now a thin orchestration layer. File I/O lives in
    ///      <see cref="JsonFileLanguageStorage"/>; defaults live in <see cref="LanguageDefaults"/>.
    /// DIP: Depends on <see cref="ILanguageStorage"/> (abstraction), not on concrete file/JSON details.
    /// ISP: Exposes <see cref="ILanguageProvider"/> so consumers only depend on what they use.
    /// OCP: New storage backends can be injected without modifying this class.
    /// </summary>
    public static class languageManager
    {
        private static ILanguageStorage _storage;
        private static IReadOnlyDictionary<string, string> _currentLanguage;
        private static readonly object _syncLock = new();
        private static bool _initialized;
        private static string _currentLangCode = "en-US";

        // ── Public API (backward-compatible) ──────────────────────────

        /// <inheritdoc cref="ILanguageProvider.GetText"/>
        public static string GetText(string key, string defaultValue = null, params object[] args)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                Debug.WriteLine($"Invalid localization key: [{key}]");
                return GetErrorText("Invalid_Key", "[INVALID_KEY]");
            }

            EnsureInitialized();

            IReadOnlyDictionary<string, string> lang;
            lock (_syncLock)
            {
                lang = _currentLanguage;
            }

            if (lang is null)
                return SafeFormat(defaultValue ?? key, args, key);

            if (!lang.TryGetValue(key, out var translation) || string.IsNullOrWhiteSpace(translation))
            {
                Debug.WriteLine($"Localization key missing: {key}");
                return SafeFormat(defaultValue ?? key, args, key);
            }

            return SafeFormat(translation, args, key);
        }

        /// <inheritdoc cref="ILanguageProvider.TryLoadLanguage"/>
        public static bool TryLoadLanguage(string langCode)
        {
            if (string.IsNullOrWhiteSpace(langCode))
                throw new ArgumentException("Language code must not be empty.", nameof(langCode));

            lock (_syncLock)
            {
                EnsureInitialized();
                if (TryLoadCore(langCode))
                    return true;
                // Fallback to en-US for unknown languages.
                if (!langCode.Equals("en-US", StringComparison.OrdinalIgnoreCase))
                    return TryLoadCore("en-US");
                return _currentLanguage is not null && _currentLanguage.Count > 0;
            }
        }

        /// <inheritdoc cref="ILanguageProvider.LoadLanguage"/>
        public static void LoadLanguage(string langCode = null)
        {
            langCode ??= Settings.Default.Language ?? "en-US";

            lock (_syncLock)
            {
                if (!TryLoadCore(langCode) &&
                    !langCode.Equals("en-US", StringComparison.OrdinalIgnoreCase) &&
                    !TryLoadCore("en-US"))
                {
                    _currentLanguage = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    _currentLangCode = "en-US";
                    Debug.WriteLine("Cannot load any language file");
                }
            }
        }

        /// <inheritdoc cref="ILanguageProvider.GetAvailableLanguages"/>
        public static IEnumerable<string> GetAvailableLanguages()
        {
            EnsureStorage();
            return _storage.GetAvailableLanguages();
        }

        /// <summary>The currently active language code.</summary>
        public static string CurrentLanguage
        {
            get
            {
                lock (_syncLock) { return _currentLangCode; }
            }
        }

        // ── Dependency injection (OCP entry point) ────────────────────

        /// <summary>
        /// Inject a custom storage backend. If never called, defaults to
        /// <see cref="JsonFileLanguageStorage"/> rooted at the Language directory.
        /// Must be called before the first call to GetText / TryLoadLanguage / LoadLanguage.
        /// </summary>
        public static void SetStorage(ILanguageStorage storage)
        {
            lock (_syncLock)
            {
                if (_initialized)
                    throw new InvalidOperationException(
                        "Cannot change storage after languageManager has been initialized.");
                _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            }
        }

        // ── Initialization ────────────────────────────────────────────

        private static void EnsureInitialized()
        {
            if (_initialized)
                return;

            lock (_syncLock)
            {
                if (_initialized)
                    return;

                EnsureStorage();
                _storage.EnsureDefaults(LanguageDefaults.Entries);
                LoadLanguage();
                _initialized = true;
            }
        }

        private static void EnsureStorage()
        {
            if (_storage is null)
            {
                var languageDir = Path.Combine(Application.StartupPath, "Language");
                _storage = new JsonFileLanguageStorage(languageDir);
            }
        }

        // ── Core language loading ─────────────────────────────────────

        private static bool TryLoadCore(string langCode)
        {
            if (_storage.TryLoad(langCode, out var translations))
            {
                _currentLanguage = translations;
                _currentLangCode = langCode;
                return true;
            }
            return false;
        }

        // ── Helpers ───────────────────────────────────────────────────

        /// <summary>
        /// Attempts to localize an error/fallback string using the current language.
        /// Falls back to the hardcoded default if the language system is unavailable.
        /// </summary>
        private static string GetErrorText(string key, string fallback)
        {
            IReadOnlyDictionary<string, string> lang;
            lock (_syncLock) { lang = _currentLanguage; }

            if (lang is not null && lang.TryGetValue(key, out var translated) && !string.IsNullOrWhiteSpace(translated))
                return translated;

            return fallback;
        }

        private static string SafeFormat(string format, object[] args, string key)
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
                return GetErrorText("Format_Error", $"[FORMAT_ERROR:{key}]");
            }
        }
    }
}
