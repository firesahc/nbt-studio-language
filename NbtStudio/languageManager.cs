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
                Debug.WriteLine($"��ʼ�����ػ�Ŀ¼ʧ��: {ex.Message}");
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
                    {"MenuFile", "�ļ�"},
                    {"MenuEdit", "�༭"},
                    {"MenuSearch", "����"},
                    {"MenuHelp", "����"},
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
                    Debug.WriteLine($"����Ĭ�������ļ�ʧ�� ({langCode}): {ex.Message}");
                }
            }
        }

        public static void LoadLanguage(string langCode = null)
        {
            langCode ??= Settings.Default.Language ?? "en-US";
            if (!TryLoadLanguage(langCode) && !TryLoadLanguage("en-US"))
            {
                _currentLanguage.Clear();
                Debug.WriteLine("�޷������κ������ļ�");
            }
        }

        public static bool TryLoadLanguage(string langCode)
        {
            lock (_syncLock)
            {
                // ���Դ�����ע����ȡ
                if (_languageRegistry.TryGetValue(langCode, out var cached))
                {
                    _currentLanguage = cached;
                    return true;
                }

                try
                {
                    var filePath = Path.Combine(_languageDir, $"{langCode}.json");

                    // ·����ȫ���
                    if (!filePath.StartsWith(_languageDir, StringComparison.OrdinalIgnoreCase) ||
                        !File.Exists(filePath))
                    {
                        return false;
                    }

                    var json = File.ReadAllText(filePath, Encoding.UTF8);
                    if (string.IsNullOrWhiteSpace(json))
                    {
                        Debug.WriteLine($"�����ļ�Ϊ��: {langCode}");
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
                    Debug.WriteLine($"��������ʧ�� ({langCode}): {ex.Message}");
                    return false;
                }
            }
        }

        public static string GetText(string key, string defaultValue = null, params object[] args)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                Debug.WriteLine($"��Ч�ı��ػ�������: [{key}]");
                return "[INVALID_KEY]";
            }

            string text = defaultValue ?? key;

            // ���Ի�ȡ����
            if (!_currentLanguage.TryGetValue(key, out var translation))
            {
                Debug.WriteLine($"���ػ���ȱʧ: {key}");
            }
            else if (!string.IsNullOrWhiteSpace(translation))
            {
                text = translation;
            }

            // ��ȫ��ʽ��
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
                Debug.WriteLine($"��ʽ��ʧ��: {key} - {ex.Message}");
                return $"[FORMAT_ERROR:{key}]";
            }
        }
    }
}