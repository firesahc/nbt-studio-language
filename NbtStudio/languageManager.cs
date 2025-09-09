using System;
using System.IO;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.Text;
using System.Diagnostics;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using NbtStudio.Properties;

namespace NbtStudio
{
    public static class languageManager
    {
        private static ConcurrentDictionary<string, string> _currentLanguage = new(StringComparer.OrdinalIgnoreCase);
        private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, string>> _languageRegistry = new();
        private static readonly object _syncLock = new();
        private static readonly string _languageDir = Path.Combine(Application.StartupPath, "Language");

        static languageManager()
        {
            EnsureLanguageDirectory();
            LoadLanguage();
        }

        private static void EnsureLanguageDirectory()
        {
            try
            {
                if (!Directory.Exists(_languageDir))
                {
                    Directory.CreateDirectory(_languageDir);
                    CreateDefaultLanguageFiles();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"初始化本地化目录失败: {ex.Message}");
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
                    {"MenuFile", "文件"},
                    {"MenuEdit", "编辑"},
                    {"MenuSearch", "查找"},
                    {"MenuHelp", "帮助"},
                }
            };

            foreach (var (langCode, translations) in defaultLanguages)
            {
                try
                {
                    var filePath = Path.Combine(_languageDir, $"{langCode}.json");
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
                    Debug.WriteLine($"创建默认语言文件失败 ({langCode}): {ex.Message}");
                }
            }
        }

        public static void LoadLanguage(string langCode = null)
        {
            langCode ??= Settings.Default.Language ?? "en-US";
            if (!TryLoadLanguage(langCode) && !TryLoadLanguage("en-US"))
            {
                _currentLanguage.Clear();
                Debug.WriteLine("无法加载任何语言文件");
            }
        }

        public static bool TryLoadLanguage(string langCode)
        {
            lock (_syncLock)
            {
                // 尝试从语言注册表获取
                if (_languageRegistry.TryGetValue(langCode, out var cached))
                {
                    _currentLanguage = cached;
                    return true;
                }

                try
                {
                    var filePath = Path.Combine(_languageDir, $"{langCode}.json");

                    // 路径安全检查
                    if (!filePath.StartsWith(_languageDir, StringComparison.OrdinalIgnoreCase) ||
                        !File.Exists(filePath))
                    {
                        return false;
                    }

                    var json = File.ReadAllText(filePath, Encoding.UTF8);
                    if (string.IsNullOrWhiteSpace(json))
                    {
                        Debug.WriteLine($"语言文件为空: {langCode}");
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
                    var concurrentDict = new ConcurrentDictionary<string, string>(
                        strings ?? new Dictionary<string, string>(),
                        StringComparer.OrdinalIgnoreCase
                    );

                    _languageRegistry[langCode] = concurrentDict;
                    _currentLanguage = concurrentDict;
                    return true;
                }
                catch (Exception ex) when (ex is JsonException || ex is IOException)
                {
                    Debug.WriteLine($"加载语言失败 ({langCode}): {ex.Message}");
                    return false;
                }
            }
        }

        public static string GetText(string key, string defaultValue = null, params object[] args)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                Debug.WriteLine($"无效的本地化键请求: [{key}]");
                return "[INVALID_KEY]";
            }

            string text = defaultValue ?? key;

            // 尝试获取翻译
            if (!_currentLanguage.TryGetValue(key, out var translation))
            {
                Debug.WriteLine($"本地化键缺失: {key}");
            }
            else if (!string.IsNullOrWhiteSpace(translation))
            {
                text = translation;
            }

            // 安全格式化
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
                Debug.WriteLine($"格式化失败: {key} - {ex.Message}");
                return $"[FORMAT_ERROR:{key}]";
            }
        }
    }
}