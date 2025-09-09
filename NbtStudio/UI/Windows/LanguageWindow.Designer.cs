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
            this.listLanguages = new ListBox();
            this.btnConfirm = new Button();
            this.btnCancel = new Button();
            this.SuspendLayout();
            // 
            // listLanguages
            // 
            this.listLanguages.FormattingEnabled = true;
            this.listLanguages.ItemHeight = 20;
            this.listLanguages.Location = new Point(15, 16);
            this.listLanguages.Margin = new Padding(4, 4, 4, 4);
            this.listLanguages.Name = "listLanguages";
            this.listLanguages.Size = new Size(214, 204);
            this.listLanguages.TabIndex = 0;
            // 
            // btnConfirm
            // 
            this.btnConfirm.Location = new Point(15, 237);
            this.btnConfirm.Margin = new Padding(4, 4, 4, 4);
            this.btnConfirm.Name = "btnConfirm";
            this.btnConfirm.Size = new Size(96, 31);
            this.btnConfirm.TabIndex = 1;
            this.btnConfirm.Text = languageManager.GetText("OK");
            this.btnConfirm.UseVisualStyleBackColor = true;
            this.btnConfirm.Click += BtnConfirm_Click;
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new Point(133, 237);
            this.btnCancel.Margin = new Padding(4, 4, 4, 4);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new Size(96, 31);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = languageManager.GetText("Cancel");
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += BtnCancel_Click;
            // 
            // LanguageWindow
            // 
            this.AutoScaleDimensions = new SizeF(9F, 20F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(244, 281);
            this.Controls.Add(btnCancel);
            this.Controls.Add(btnConfirm);
            this.Controls.Add(listLanguages);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.Margin = new Padding(4, 4, 4, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LanguageWindow";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = languageManager.GetText("Select_Language");
            this.Load += new System.EventHandler(this.LanguageWindow_Load);
            this.ResumeLayout(false);
        }

        #endregion
    }
}