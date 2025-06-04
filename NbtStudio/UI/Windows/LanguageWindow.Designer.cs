using System.Drawing;
using System.Windows.Forms;
using System;
using NbtStudio;

namespace NBTStudio
{
    partial class LanguageWindow
    {
        private System.ComponentModel.IContainer components = null;

        private ListBox listLanguages;
        private Button btnConfirm;
        private Button btnCancel;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            listLanguages = new ListBox();
            btnConfirm = new Button();
            btnCancel = new Button();
            SuspendLayout();
            // 
            // listLanguages
            // 
            listLanguages.FormattingEnabled = true;
            listLanguages.ItemHeight = 20;
            listLanguages.Location = new Point(15, 16);
            listLanguages.Margin = new Padding(4, 4, 4, 4);
            listLanguages.Name = "listLanguages";
            listLanguages.Size = new Size(214, 204);
            listLanguages.TabIndex = 0;
            // 
            // btnConfirm
            // 
            btnConfirm.Location = new Point(15, 237);
            btnConfirm.Margin = new Padding(4, 4, 4, 4);
            btnConfirm.Name = "btnConfirm";
            btnConfirm.Size = new Size(96, 31);
            btnConfirm.TabIndex = 1;
            btnConfirm.Text = LocalizationManager.GetText("OK");
            btnConfirm.UseVisualStyleBackColor = true;
            btnConfirm.Click += BtnConfirm_Click;
            // 
            // btnCancel
            // 
            btnCancel.Location = new Point(133, 237);
            btnCancel.Margin = new Padding(4, 4, 4, 4);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(96, 31);
            btnCancel.TabIndex = 2;
            btnCancel.Text = LocalizationManager.GetText("Cancel");
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += BtnCancel_Click;
            // 
            // LanguageWindow
            // 
            AutoScaleDimensions = new SizeF(9F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(244, 281);
            Controls.Add(btnCancel);
            Controls.Add(btnConfirm);
            Controls.Add(listLanguages);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Margin = new Padding(4, 4, 4, 4);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "LanguageWindow";
            StartPosition = FormStartPosition.CenterParent;
            Text = LocalizationManager.GetText("Select_Language");
            Load += LanguageWindow_Load;
            ResumeLayout(false);
        }

        #endregion
    }
}