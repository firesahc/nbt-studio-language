using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace NbtStudio
{
    /// <summary>
    /// SRP: Contains the canonical set of default/first-run translation entries,
    /// separated from storage and provider logic so defaults can evolve independently.
    /// OCP: New language defaults can be added without touching storage or provider code.
    /// </summary>
    internal static class LanguageDefaults
    {
        public static readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> Entries;

        static LanguageDefaults()
        {
            var defaults = new Dictionary<string, IReadOnlyDictionary<string, string>>
            {
                ["en-US"] = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>
                {
                    // Menu roots
                    { "MenuFile",         "File" },
                    { "MenuEdit",         "Edit" },
                    { "MenuSearch",       "Find" },
                    { "MenuHelp",         "Help" },
                    // Common UI
                    { "OK",               "OK" },
                    { "Cancel",           "Cancel" },
                    // Language window
                    { "Select_Language",  "Select Language" },
                    { "LanguageWindow_Title", "Select Language" },
                    { "Restart_Required", "Restart Required" },
                    { "Restart_Required_Detail", "Some text changes require a restart to take effect. Restart now?" },
                    // Error fallbacks
                    { "Invalid_Key",      "[INVALID_KEY]" },
                    { "Format_Error",     "[FORMAT_ERROR]" },
                    { "Language_Load_Failed", "Language Error" },
                    { "Language_Load_Failed_Detail", "Failed to load language '{0}'." },
                }),

                ["zh-CN"] = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>
                {
                    // Menu roots
                    { "MenuFile",         "文件" },
                    { "MenuEdit",         "编辑" },
                    { "MenuSearch",       "查找" },
                    { "MenuHelp",         "帮助" },
                    // Common UI
                    { "OK",               "确认" },
                    { "Cancel",           "取消" },
                    // Language window
                    { "Select_Language",  "选择语言" },
                    { "LanguageWindow_Title", "选择语言" },
                    { "Restart_Required", "提示" },
                    { "Restart_Required_Detail", "部分文本需要重启应用以应用更改，是否立即重启？" },
                    // Error fallbacks
                    { "Invalid_Key",      "[INVALID_KEY]" },
                    { "Format_Error",     "[FORMAT_ERROR]" },
                    { "Language_Load_Failed", "语言错误" },
                    { "Language_Load_Failed_Detail", "无法加载语言 '{0}'。" },
                }),
            };

            Entries = new ReadOnlyDictionary<string, IReadOnlyDictionary<string, string>>(defaults);
        }
    }
}
