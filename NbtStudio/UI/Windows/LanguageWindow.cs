using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;
using NbtStudio.Properties;
using System.Linq;
using NbtStudio;
using System;

namespace NBTStudio
{
    public partial class LanguageWindow : Form
    {
        private static readonly Dictionary<string, string> LanguageDisplayNames = new()
        {
            ["en-US"] = "English",
            ["zh-CN"] = "简体中文"
            // 添加更多语言映射...
        };

        public LanguageWindow(IconSource source)
        {
            InitializeComponent();
            this.Icon = source.GetImage(IconType.NbtStudio).Icon;
            this.Text = LocalizationManager.GetText("LanguageWindow_Title");
            LoadAvailableLanguages();
        }

        private void LanguageWindow_Load(object sender, EventArgs e)
        {
            this.CenterToParent();
        }

        private void LoadAvailableLanguages()
        {
            listLanguages.Items.Clear();

            string langDir = Path.Combine(Application.StartupPath, "Localization");
            if (!Directory.Exists(langDir))
                return;

            var validFiles = Directory.EnumerateFiles(langDir, "*.json")
                .Select(Path.GetFileNameWithoutExtension)
                .Where(LanguageDisplayNames.ContainsKey);

            foreach (string langCode in validFiles)
            {
                listLanguages.Items.Add(new LanguageItem(
                    displayName: LanguageDisplayNames[langCode],
                    code: langCode
                ));
            }

            // 设置当前选中项
            string currentLang = Settings.Default.Language ?? "en-US";
            for (int i = 0; i < listLanguages.Items.Count; i++)
            {
                if (((LanguageItem)listLanguages.Items[i]).Code == currentLang)
                {
                    listLanguages.SelectedIndex = i;
                    break;
                }
            }
        }

        private void BtnConfirm_Click(object sender, System.EventArgs e)
        {
            if (listLanguages.SelectedItem is not LanguageItem selected)
                return;

            try
            {
                if (LocalizationManager.TryLoadLanguage(selected.Code))
                {
                    Settings.Default.Language = selected.Code;
                    Settings.Default.Save();

                    var result = MessageBox.Show(
                        text: LocalizationManager.GetText("Restart_Required_Detail"),
                        caption: LocalizationManager.GetText("Restart_Required"),
                        buttons: MessageBoxButtons.YesNo,
                        icon: MessageBoxIcon.Information
                    );

                    if (result == DialogResult.Yes)
                    {
                        Application.Restart();
                    }
                }
                this.DialogResult = DialogResult.OK;
            }
            finally
            {
                this.Close();
            }
        }

        private void BtnCancel_Click(object sender, System.EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        // 辅助类用于存储语言显示名称和代码
        private sealed class LanguageItem
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