using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;
using System;
using NbtStudio.Properties;
using NbtStudio;

namespace NBTStudio
{
    public partial class LanguageWindow : Form
    {
        public LanguageWindow(IconSource source)
        {
            InitializeComponent();
            this.Icon = source.GetImage(IconType.NbtStudio).Icon;
        }

        private void LanguageWindow_Load(object sender, EventArgs e)
        {
            LoadAvailableLanguages();
            this.Text = LocalizationManager.GetText("LanguageWindow_Title");
        }

        private void LoadAvailableLanguages()
        {
            listLanguages.Items.Clear();

            var langDir = Path.Combine(Application.StartupPath, "Localization");
            if (!Directory.Exists(langDir)) return;

            var languages = new Dictionary<string, string>
            {
                { "en-US", "English" },
                { "zh-CN", "简体中文" }
                // 添加更多语言映射...
            };

            foreach (var file in Directory.GetFiles(langDir, "*.json"))
            {
                var langCode = Path.GetFileNameWithoutExtension(file);
                if (languages.TryGetValue(langCode, out var displayName))
                {
                    listLanguages.Items.Add(new LanguageItem(displayName, langCode));
                }
            }

            // 设置当前选中项
            var currentIndex = listLanguages.FindString(LocalizationManager.CurrentLanguage);
            if (currentIndex >= 0) listLanguages.SelectedIndex = currentIndex;
        }

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            if (listLanguages.SelectedItem is LanguageItem item)
            {
                if (LocalizationManager.LoadLanguage(item.Code))
                {
                    var result = MessageBox.Show(
                        LocalizationManager.GetText("Restart_Required_Detail"),
                        LocalizationManager.GetText("Restart_Required"),
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Information);

                    if (result == DialogResult.Yes)
                    {
                        Settings.Default.Language = item.Code;
                        Settings.Default.Save();
                        Application.Restart();
                    }
                }
                this.DialogResult = DialogResult.OK;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        // 辅助类用于存储语言显示名称和代码
        private class LanguageItem
        {
            public string DisplayName { get; }
            public string Code { get; }

            public LanguageItem(string displayName, string code)
            {
                DisplayName = displayName;
                Code = code;
            }

            public override string ToString() => DisplayName;
        }
    }
}