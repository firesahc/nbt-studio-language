﻿namespace NbtStudio.UI
{
    partial class FindWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.ButtonFindNext = new System.Windows.Forms.Button();
            this.ButtonFindPrev = new System.Windows.Forms.Button();
            this.RegexCheck = new System.Windows.Forms.CheckBox();
            this.NameLabel = new System.Windows.Forms.Label();
            this.ValueLabel = new System.Windows.Forms.Label();
            this.ButtonFindAll = new System.Windows.Forms.Button();
            this.ProgressBar = new System.Windows.Forms.ProgressBar();
            this.FoundResultsLabel = new System.Windows.Forms.Label();
            this.ValueBox = new NbtStudio.UI.RegexTextBox();
            this.NameBox = new NbtStudio.UI.RegexTextBox();
            this.SuspendLayout();
            // 
            // ButtonFindNext
            // 
            this.ButtonFindNext.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ButtonFindNext.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.ButtonFindNext.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.ButtonFindNext.Location = new System.Drawing.Point(191, 91);
            this.ButtonFindNext.Name = "ButtonFindNext";
            this.ButtonFindNext.Size = new System.Drawing.Size(75, 23);
            this.ButtonFindNext.TabIndex = 8;
            this.ButtonFindNext.Text = languageManager.GetText("Find_Next");
            this.ButtonFindNext.UseVisualStyleBackColor = true;
            this.ButtonFindNext.Click += new System.EventHandler(this.ButtonFindNext_Click);
            // 
            // ButtonFindPrev
            // 
            this.ButtonFindPrev.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ButtonFindPrev.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.ButtonFindPrev.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.ButtonFindPrev.Location = new System.Drawing.Point(107, 91);
            this.ButtonFindPrev.Name = "ButtonFindPrev";
            this.ButtonFindPrev.Size = new System.Drawing.Size(75, 23);
            this.ButtonFindPrev.TabIndex = 7;
            this.ButtonFindPrev.Text = languageManager.GetText("Find_Previous");
            this.ButtonFindPrev.UseVisualStyleBackColor = true;
            this.ButtonFindPrev.Click += new System.EventHandler(this.ButtonFindPrev_Click);
            // 
            // RegexCheck
            // 
            this.RegexCheck.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.RegexCheck.AutoSize = true;
            this.RegexCheck.Location = new System.Drawing.Point(12, 93);
            this.RegexCheck.Name = "RegexCheck";
            this.RegexCheck.Size = new System.Drawing.Size(57, 17);
            this.RegexCheck.TabIndex = 6;
            this.RegexCheck.Text = languageManager.GetText("Regex");
            this.RegexCheck.UseVisualStyleBackColor = true;
            this.RegexCheck.CheckedChanged += new System.EventHandler(this.RegexCheck_CheckedChanged);
            // 
            // NameLabel
            // 
            this.NameLabel.AutoSize = true;
            this.NameLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.NameLabel.Location = new System.Drawing.Point(14, 14);
            this.NameLabel.Margin = new System.Windows.Forms.Padding(10, 10, 5, 0);
            this.NameLabel.Name = "NameLabel";
            this.NameLabel.Size = new System.Drawing.Size(44, 15);
            this.NameLabel.TabIndex = 0;
            this.NameLabel.Text = languageManager.GetText("Name") + ":";
            // 
            // ValueLabel
            // 
            this.ValueLabel.AutoSize = true;
            this.ValueLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.ValueLabel.Location = new System.Drawing.Point(14, 41);
            this.ValueLabel.Margin = new System.Windows.Forms.Padding(10, 10, 5, 0);
            this.ValueLabel.Name = "ValueLabel";
            this.ValueLabel.Size = new System.Drawing.Size(41, 15);
            this.ValueLabel.TabIndex = 2;
            this.ValueLabel.Text = languageManager.GetText("Value") + ":";
            // 
            // ButtonFindAll
            // 
            this.ButtonFindAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ButtonFindAll.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.ButtonFindAll.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.ButtonFindAll.Location = new System.Drawing.Point(272, 91);
            this.ButtonFindAll.Name = "ButtonFindAll";
            this.ButtonFindAll.Size = new System.Drawing.Size(75, 23);
            this.ButtonFindAll.TabIndex = 9;
            this.ButtonFindAll.Text = languageManager.GetText("Find_All");
            this.ButtonFindAll.UseVisualStyleBackColor = true;
            this.ButtonFindAll.Click += new System.EventHandler(this.ButtonFindAll_Click);
            // 
            // ProgressBar
            // 
            this.ProgressBar.Location = new System.Drawing.Point(11, 71);
            this.ProgressBar.Name = "ProgressBar";
            this.ProgressBar.Size = new System.Drawing.Size(336, 14);
            this.ProgressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.ProgressBar.TabIndex = 4;
            this.ProgressBar.Visible = false;
            // 
            // FoundResultsLabel
            // 
            this.FoundResultsLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.FoundResultsLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.FoundResultsLabel.ForeColor = System.Drawing.Color.Gray;
            this.FoundResultsLabel.Location = new System.Drawing.Point(191, 68);
            this.FoundResultsLabel.Margin = new System.Windows.Forms.Padding(10, 10, 5, 0);
            this.FoundResultsLabel.Name = "FoundResultsLabel";
            this.FoundResultsLabel.Size = new System.Drawing.Size(156, 17);
            this.FoundResultsLabel.TabIndex = 5;
            this.FoundResultsLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.FoundResultsLabel.Visible = false;
            // 
            // ValueBox
            // 
            this.ValueBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ValueBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Append;
            this.ValueBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.ValueBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.ValueBox.Location = new System.Drawing.Point(73, 41);
            this.ValueBox.Margin = new System.Windows.Forms.Padding(5, 10, 10, 0);
            this.ValueBox.Name = "ValueBox";
            this.ValueBox.RegexMode = false;
            this.ValueBox.Size = new System.Drawing.Size(274, 21);
            this.ValueBox.TabIndex = 3;
            this.ValueBox.TextChanged += new System.EventHandler(this.ValueBox_TextChanged);
            // 
            // NameBox
            // 
            this.NameBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.NameBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Append;
            this.NameBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.NameBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.NameBox.Location = new System.Drawing.Point(73, 14);
            this.NameBox.Margin = new System.Windows.Forms.Padding(5, 10, 10, 0);
            this.NameBox.Name = "NameBox";
            this.NameBox.RegexMode = false;
            this.NameBox.Size = new System.Drawing.Size(274, 21);
            this.NameBox.TabIndex = 1;
            this.NameBox.TextChanged += new System.EventHandler(this.NameBox_TextChanged);
            // 
            // FindWindow
            // 
            this.AcceptButton = this.ButtonFindNext;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(360, 122);
            this.Controls.Add(this.FoundResultsLabel);
            this.Controls.Add(this.ProgressBar);
            this.Controls.Add(this.ButtonFindAll);
            this.Controls.Add(this.ValueBox);
            this.Controls.Add(this.ValueLabel);
            this.Controls.Add(this.RegexCheck);
            this.Controls.Add(this.NameBox);
            this.Controls.Add(this.ButtonFindPrev);
            this.Controls.Add(this.ButtonFindNext);
            this.Controls.Add(this.NameLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FindWindow";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = languageManager.GetText("Find");
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FindWindow_FormClosed);
            this.Load += new System.EventHandler(this.FindWindow_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button ButtonFindNext;
        private System.Windows.Forms.Button ButtonFindPrev;
        private System.Windows.Forms.CheckBox RegexCheck;
        private System.Windows.Forms.Label NameLabel;
        private RegexTextBox NameBox;
        private System.Windows.Forms.Label ValueLabel;
        private RegexTextBox ValueBox;
        private System.Windows.Forms.Button ButtonFindAll;
        private System.Windows.Forms.ProgressBar ProgressBar;
        private System.Windows.Forms.Label FoundResultsLabel;
    }
}