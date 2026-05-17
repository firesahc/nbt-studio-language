using System.Collections.Generic;

namespace NbtStudio
{
    /// <summary>
    /// Provides localized text and language switching capabilities.
    /// ISP: Consumers only depend on the methods they need (GetText, loading, enumeration).
    /// DIP: Higher-level modules depend on this abstraction, not on concrete storage.
    /// </summary>
    public interface ILanguageProvider
    {
        /// <summary>Retrieve a localized string by key. Falls back to defaultValue or the key itself.</summary>
        string GetText(string key, string defaultValue = null, params object[] args);

        /// <summary>Load a specific language by its code (e.g. "en-US", "zh-CN"). Returns true on success.</summary>
        bool TryLoadLanguage(string langCode);

        /// <summary>Load a language; if null, loads the persisted setting or "en-US" as fallback.</summary>
        void LoadLanguage(string langCode = null);

        /// <summary>Enumerate all available language codes found in storage.</summary>
        IEnumerable<string> GetAvailableLanguages();

        /// <summary>The currently active language code (e.g. "en-US").</summary>
        string CurrentLanguage { get; }
    }
}
