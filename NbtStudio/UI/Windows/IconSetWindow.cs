﻿using fNbt;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using TryashtarUtils.Utility;

namespace NbtStudio.UI
{
    public partial class IconSetWindow : Form
    {
        private int SelectedRow = 0;
        private readonly IconSource CurrentSource;
        public IconSource SelectedSource { get; private set; }
        public IconSetWindow(IconSource current)
        {
            InitializeComponent();
            CurrentSource = current;
            this.Icon = current.GetImage(IconType.Refresh).Icon;
            RefreshIcons();
        }

        public void RefreshIcons()
        {
            SuspendLayout();
            Action select = () => { };
            int row = 0;
            IconTable.Controls.Clear();
            IconTable.RowStyles.Clear();
            foreach (var item in IconSourceRegistry.RegisteredSources)
            {
                var source = item.Value;
                var buttons = new IconSourceButtons(source);
                buttons.Dock = DockStyle.Fill;
                IconTable.RowStyles.Add(new RowStyle(SizeType.Absolute, buttons.Height));
                IconTable.Controls.Add(buttons, 0, row);
                IconTable.ColumnStyles[0].Width = Math.Max(IconTable.ColumnStyles[0].Width, buttons.PreferredSize.Width + 5);
                buttons.ConfirmClicked += (s, e) =>
                {
                    SelectedSource = source;
                    this.Close();
                };
                buttons.DeleteClicked += (s, e) =>
                {
                    IconSourceRegistry.Unregister(item.Key);
                    Properties.Settings.Default.CustomIconSets.Remove(item.Key);
                    RefreshIcons();
                };
                var preview = new IconSourcePreview(source,
                    IconType.OpenFile,
                    IconType.Save,
                    IconType.Edit,
                    IconType.Cut,
                    IconType.Undo,
                    IconType.ByteTag,
                    IconType.StringTag,
                    IconType.IntArrayTag,
                    IconType.ListTag,
                    IconType.Region,
                    IconType.Chunk
                );
                preview.Dock = DockStyle.Fill;
                IconTable.Controls.Add(preview, 1, row);
                IconTable.RowStyles[row].Height = Math.Max(IconTable.RowStyles[row].Height, preview.PreferredSize.Height + 5);
                if (CurrentSource == source)
                {
                    SelectedRow = row;
                    buttons.BackColor = Color.FromArgb(201, 255, 221);
                    preview.BackColor = Color.FromArgb(201, 255, 221);
                    select = () => buttons.Select();
                }
                row++;
            }
            select();
            ResumeLayout();
        }

        private void IconTable_CellPaint(object sender, TableLayoutCellPaintEventArgs e)
        {
            if (e.Row == SelectedRow)
                e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(201, 255, 221)), e.CellBounds);
        }

        private void IconSetWindow_Load(object sender, EventArgs e)
        {
            this.CenterToParent();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                this.Close();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void ImportButton_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog
            {
                Title = languageManager.GetText("Icon_ZIP"),
                RestoreDirectory = false,
                Multiselect = true,
                Filter = "ZIP Files|*.zip"
            })
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    foreach (var file in dialog.FileNames)
                    {
                        var attempt = TryImportSource(file);
                        if (attempt.Failed)
                            ShowImportFailed(file, attempt, this);
                    }
                    RefreshIcons();
                }
            }
        }

        public static void ShowImportFailed(string path, IFailable import, IWin32Window owner)
        {
            var window = new ExceptionWindow(languageManager.GetText("Failed_load_icons"),languageManager.GetText("Failed_load_icons_Detail", args: new Object[] { path }), import);
            window.ShowDialog(owner);
        }

        public static IFailable TryImportSource(string path)
        {
            var failable = new Failable(() =>
            {
                IconSourceRegistry.RegisterCustomSource(path);
                if (!Properties.Settings.Default.CustomIconSets.Contains(path))
                    Properties.Settings.Default.CustomIconSets.Add(path);
            }, languageManager.GetText("Loading_icons"));
            if (failable.Failed)
                Properties.Settings.Default.CustomIconSets.Remove(path);
            return failable;
        }
    }
}