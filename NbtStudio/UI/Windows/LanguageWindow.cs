using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;
using NbtStudio.Properties;
using System.Linq;
using NbtStudio;
using System;

namespace NbtStudio.UI
{
    public partial class LanguageWindow : Form
    {
        private static readonly Dictionary<string, string> LanguageDisplayNames = new()
        {
            ["en-US"] = "English",
            ["zh-CN"] = "中文 (Chinese)"
        };

        public LanguageWindow(IconSource source)
        {
            InitializeComponent();
            this.Icon = source.GetImage(IconType.NbtStudio).Icon;
            this.Text = languageManager.GetText("LanguageWindow_Title");
            LoadAvailableLanguages();
        }

        private void LanguageWindow_Load(object sender, EventArgs e)
        {
            this.CenterToParent();
        }

        private void LoadAvailableLanguages()
        {
            listLanguages.Items.Clear();

            var availableLanguages = languageManager.GetAvailableLanguages().ToList();

            foreach (string langCode in availableLanguages)
            {
                string displayName = LanguageDisplayNames.TryGetValue(langCode, out var name)
                    ? name
                    : langCode;

                listLanguages.Items.Add(new LanguageItem(
                    displayName: displayName,
                    code: langCode
                ));
            }

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

            string currentLang = Settings.Default.Language ?? "en-US";
            if (selected.Code == currentLang)
            {
                this.Close();
                return;
            }

            if (languageManager.TryLoadLanguage(selected.Code))
            {
                Settings.Default.Language = selected.Code;
                Settings.Default.Save();

                var result = MessageBox.Show(
                    text: languageManager.GetText("Restart_Required_Detail"),
                    caption: languageManager.GetText("Restart_Required"),
                    buttons: MessageBoxButtons.YesNo,
                    icon: MessageBoxIcon.Information
                );

                if (result == DialogResult.Yes)
                {
                    Application.Restart();
                }
                else
                {
                    this.Close();
                }
            }
            else
            {
                MessageBox.Show(
                    text: languageManager.GetText("Language_Load_Failed_Detail", null, selected.DisplayName),
                    caption: languageManager.GetText("Language_Load_Failed"),
                    buttons: MessageBoxButtons.OK,
                    icon: MessageBoxIcon.Warning
                );
            }
        }

        private void BtnCancel_Click(object sender, System.EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

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
