using System;
using System.IO;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.Text;
using System.Diagnostics;
using System.Collections.Concurrent;
using NbtStudio.Properties;
using System.Collections.Generic;

public static class LocalizationManager
{
    public static string CurrentLanguage { get; private set; } = "en-US";
    private static ConcurrentDictionary<string, string> _currentStrings = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    private static readonly ConcurrentDictionary<string, string> _defaultStrings = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    private static readonly object _syncLock = new object();

    public static void LoadLanguageFile()
    {
        var savedLang = Settings.Default.Language ?? "en-US";
        if (!LoadLanguage(savedLang)) LoadLanguage("en-US");
    }

    public static bool LoadLanguage(string langCode)
    {
        lock (_syncLock)
        {
            try
            {
                var basePath = Path.Combine(Application.StartupPath, "Localization");
                var fullPath = Path.GetFullPath(Path.Combine(basePath, $"{langCode}.json"));

                // 验证路径安全性
                if (!fullPath.StartsWith(basePath, StringComparison.OrdinalIgnoreCase))
                {
                    ShowError("无效的语言文件路径");
                    return false;
                }

                if (!File.Exists(fullPath))
                {
                    ShowError($"语言文件 {langCode}.json 未找到");
                    return false;
                }

                string json;
                using (var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    json = reader.ReadToEnd();
                }

                if (string.IsNullOrWhiteSpace(json))
                {
                    ShowError($"语言文件为空: {langCode}.json");
                    return false;
                }

                // 安全反序列化设置
                var settings = new JsonSerializerSettings
                {
                    MissingMemberHandling = MissingMemberHandling.Ignore,
                    NullValueHandling = NullValueHandling.Ignore,
                    MaxDepth = 10
                };

                var newStrings = JsonConvert.DeserializeObject<ConcurrentDictionary<string, string>>(json, settings);
                _currentStrings = new ConcurrentDictionary<string, string>(newStrings, StringComparer.OrdinalIgnoreCase);

                CurrentLanguage = langCode;

                if (langCode.Equals("en-US", StringComparison.OrdinalIgnoreCase))
                {
                    _defaultStrings.Clear();
                    foreach (var pair in _currentStrings)
                    {
                        _defaultStrings[pair.Key] = pair.Value;
                    }
                }

                return true;
            }
            catch (JsonException ex)
            {
                ShowError($"JSON解析错误: {ex.Message}", "格式错误");
                return false;
            }
            catch (IOException ex)
            {
                ShowError($"文件访问错误: {ex.Message}", "IO错误");
                return false;
            }
            catch (Exception ex)
            {
                ShowError($"加载语言失败: {ex.GetType().Name} - {ex.Message}");
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

        // 获取原始文本
        if (!TryGetLocalizedText(key, out var text, defaultValue))
        {
            Debug.WriteLine($"本地化键缺失: {key}");
            text = defaultValue ?? key;
        }

        // 安全格式化参数
        return FormatText(text, args, key);
    }

    private static bool TryGetLocalizedText(string key, out string text, string defaultValue)
    {
        if (_currentStrings.TryGetValue(key, out text)) return true;
        if (_defaultStrings.TryGetValue(key, out text)) return true;

        text = defaultValue;
        return false;
    }

    private static string FormatText(string text, object[] args, string key)
    {
        if (args == null || args.Length == 0) return text;

        try
        {
            return string.Format(text, args);
        }
        catch (FormatException ex)
        {
            Debug.WriteLine($"格式化失败: {key} - {ex.Message}");
            return $"[FORMAT_ERROR:{key}]";
        }
        catch (ArgumentNullException)
        {
            Debug.WriteLine($"空参数异常: {key}");
            return $"[NULL_ARGUMENT:{key}]";
        }
    }

    private static void ShowError(string message, string title = "错误")
    {
        try
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"错误显示失败: {ex.Message}");
        }
    }

    // 添加默认语言配置
    private static readonly Dictionary<string, Dictionary<string, string>> DefaultLanguages = new Dictionary<string, Dictionary<string, string>>
    {
        {
            "en-US", new Dictionary<string, string>
            {
                {"MenuFile", "File"},
                {"MenuEdit", "Edit"},
                {"MenuSearch", "Find"},
                {"MenuHelp", "Help"},
            }
        },
        {
            "zh-CN", new Dictionary<string, string>
            {
                {"MenuFile", "文件"},
                {"MenuEdit", "编辑"},
                {"MenuSearch", "查找"},
                {"MenuHelp", "帮助"},
            }
        }
    };

    public static void InitializeLocalizationFiles()
    {
        try
        {
            var localizationDir = Path.Combine(Application.StartupPath, "Localization");

            // 创建文件夹（如果不存在）
            if (!Directory.Exists(localizationDir))
            {
                Directory.CreateDirectory(localizationDir);
                Debug.WriteLine($"已创建本地化目录：{localizationDir}");
            }

            // 创建默认语言文件
            foreach (var lang in DefaultLanguages)
            {
                var filePath = Path.Combine(localizationDir, $"{lang.Key}.json");

                if (!File.Exists(filePath))
                {
                    CreateLanguageFile(filePath, lang.Value);
                }
                else
                {
                    ValidateLanguageFile(filePath, lang.Value);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"初始化本地化文件失败: {ex.Message}");
        }
    }

    private static void CreateLanguageFile(string path, Dictionary<string, string> defaultContent)
    {
        try
        {
            var json = JsonConvert.SerializeObject(defaultContent, Formatting.Indented);
            File.WriteAllText(path, json, Encoding.UTF8);
            Debug.WriteLine($"已创建默认语言文件： {Path.GetFileName(path)}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"创建语言文件失败 {path}: {ex.Message}");
        }
    }

    private static void ValidateLanguageFile(string path, Dictionary<string, string> defaultContent)
    {
        try
        {
            var existing = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(path));

            // 检查缺失的键
            foreach (var key in defaultContent.Keys)
            {
                if (!existing.ContainsKey(key))
                {
                    existing[key] = defaultContent[key];
                    Debug.WriteLine($"将缺少的键 '{key}' 添加到 {Path.GetFileName(path)}");
                }
            }

            // 更新文件（仅当需要时）
            if (existing.Count > defaultContent.Count)
            {
                var updatedJson = JsonConvert.SerializeObject(existing, Formatting.Indented);
                File.WriteAllText(path, updatedJson, Encoding.UTF8);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"验证语言文件失败 {path}: {ex.Message}");
        }
    }
}