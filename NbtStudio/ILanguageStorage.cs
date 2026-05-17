using System.Collections.Generic;

namespace NbtStudio
{
    /// <summary>
    /// Abstracts how language translation data is stored and retrieved.
    /// DIP + OCP: The provider depends on this abstraction; new storage backends
    /// (database, embedded resource, web API) can be added without modifying the provider.
    /// </summary>
    public interface ILanguageStorage
    {
        /// <summary>Attempt to load translations for a language code. Returns false if not found.</summary>
        bool TryLoad(string langCode, out IReadOnlyDictionary<string, string> translations);

        /// <summary>Enumerate language codes available in this storage.</summary>
        IEnumerable<string> GetAvailableLanguages();

        /// <summary>
        /// Ensure a set of default translations exists in storage (used for first-run bootstrap).
        /// Implementations should be idempotent — only create files/entries that don't already exist.
        /// </summary>
        void EnsureDefaults(IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> defaults);

        /// <summary>The root directory or connection string used by this storage.</summary>
        string Location { get; }
    }
}
