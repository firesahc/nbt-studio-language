﻿using Be.Windows.Forms;
using fNbt;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TryashtarUtils.Utility;

namespace NbtStudio.UI
{
    public partial class EditHexWindow : Form
    {
        private readonly NbtTag WorkingTag;
        private readonly NbtContainerTag TagParent;
        private readonly bool SettingName;
        private readonly IByteTransformer Provider;

        private EditHexWindow(IconSource source, NbtTag tag, NbtContainerTag parent, bool set_name, EditPurpose purpose)
        {
            InitializeComponent();
            TabView.Size = new Size(0, 0);

            WorkingTag = tag;
            TagParent = parent;
            NameBox.SetTags(tag, parent);

            SettingName = set_name;
            NameLabel.Visible = SettingName;
            NameBox.Visible = SettingName;

            Provider = ByteProviders.GetByteProvider(tag);
            HexBox.ByteProvider = Provider;
            HexBox.GroupSize = Provider.BytesPerValue;
            HexBox.GroupSeparatorVisible = Provider.BytesPerValue > 1;
            HexBox.SelectionBackColor = Constants.SelectionColor;
            HexBox.SelectionForeColor = HexBox.ForeColor;

            string tagname;
            if (tag is NbtList list)
            {
                tagname = NbtUtil.TagTypeName(list.ListType) + " List";
                this.Icon = NbtUtil.TagTypeImage(source, list.ListType).Icon;
            }
            else
            {
                tagname = NbtUtil.TagTypeName(tag.TagType);
                this.Icon = NbtUtil.TagTypeImage(source, tag.TagType).Icon;
            }
            if (purpose == EditPurpose.Create)
                this.Text = languageManager.GetText("Create_Tag_Detail", args: new Object[] { tagname });
            else if (purpose == EditPurpose.EditValue || purpose == EditPurpose.Rename)
                this.Text = languageManager.GetText("Edit_Tag_Detail", args: new Object[] { tagname });

            if (SettingName && purpose != EditPurpose.EditValue)
            {
                NameBox.Select();
                NameBox.SelectAll();
            }
            else
                HexBox.Select();
        }

        public static NbtTag CreateTag(IconSource source, NbtTagType type, NbtContainerTag parent, bool bypass_window = false)
        {
            bool has_name = parent is NbtCompound;
            var tag = NbtUtil.CreateTag(type);

            if (bypass_window)
            {
                tag.Name = NbtUtil.GetAutomaticName(tag, parent);
                return tag;
            }
            var window = new EditHexWindow(source, tag, parent, has_name, EditPurpose.Create);
            return window.ShowDialog() == DialogResult.OK ? tag : null;
        }

        public static bool ModifyTag(IconSource source, NbtTag existing, EditPurpose purpose)
        {
            if (purpose == EditPurpose.Create)
                throw new ArgumentException(languageManager.GetText("Use_CreateTag"));
            var parent = existing.Parent;
            bool has_name = parent is NbtCompound;

            var window = new EditHexWindow(source, existing, parent, has_name, purpose);
            return window.ShowDialog() == DialogResult.OK; // window modifies the tag by itself
        }

        private void Confirm()
        {
            if (TryModify())
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private bool TryModify()
        {
            // check conditions first, tag must not be modified at ALL until we can be sure it's safe
            if (SettingName && !NameBox.CheckName())
                return false;

            if (SettingName)
                NameBox.ApplyName();

            HexBox.ByteProvider.ApplyChanges();
            return true;
        }

        private void ButtonOk_Click(object sender, EventArgs e)
        {
            if (TabView.SelectedTab == TextPage)
            {
                Provider.SetBytes(ConvertFromText(TextBox.Text, Provider.BytesPerValue));
            }
            Confirm();
        }

        private void UpdateCursorLabel()
        {
            long selected_byte = HexBox.SelectionStart;
            long selected_byte2 = HexBox.SelectionStart + HexBox.SelectionLength;
            if (HexBox.SelectionLength > 1)
                CursorLabel.Text = languageManager.GetText("Elements", args: new Object[] { selected_byte / Provider.BytesPerValue, selected_byte2 / Provider.BytesPerValue });
            else
                CursorLabel.Text = languageManager.GetText("Element", args: new Object[] { selected_byte / Provider.BytesPerValue });
        }

        private void HexBox_CurrentLineChanged(object sender, EventArgs e)
        {
            UpdateCursorLabel();
        }

        private void HexBox_CurrentPositionInLineChanged(object sender, EventArgs e)
        {
            UpdateCursorLabel();
        }

        private void HexBox_SelectionLengthChanged(object sender, EventArgs e)
        {
            UpdateCursorLabel();
        }

        private void HexBox_SelectionStartChanged(object sender, EventArgs e)
        {
            UpdateCursorLabel();
        }

        private void HexBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == (Keys.Control | Keys.A))
            {
                HexBox.SelectAll();
                e.Handled = true;
            }
        }

        private void TabView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (TabView.SelectedTab == HexPage)
            {
                Provider.SetBytes(ConvertFromText(TextBox.Text, Provider.BytesPerValue));
            }
            else if (TabView.SelectedTab == TextPage)
            {
                TextBox.Text = ConvertToText(Provider);
            }
        }

        private string ConvertToText(IByteTransformer provider)
        {
            var bytes = provider.CurrentBytes.ToArray();
            int size = provider.BytesPerValue;
            if (size == sizeof(byte))
                return String.Join(" ", bytes.Select(x => (sbyte)x));
            if (size == sizeof(short))
                return String.Join(" ", DataUtils.ToShortArray(bytes));
            if (size == sizeof(int))
                return String.Join(" ", DataUtils.ToIntArray(bytes));
            if (size == sizeof(long))
                return String.Join(" ", DataUtils.ToLongArray(bytes));
            throw new ArgumentException(languageManager.GetText("EditHex_Convert", args: new Object[]{size} ));
        }

        private byte[] ConvertFromText(string text, int size)
        {
            string[] vals = text.Replace(',', ' ').Split((char[])null, StringSplitOptions.RemoveEmptyEntries); // whitespace as delimiter
            if (size == sizeof(byte))
                return vals.Select(ParseByte).Select(x => (byte)x).ToArray();
            if (size == sizeof(short))
                return DataUtils.ToByteArray(vals.Select(ParseShort).ToArray());
            if (size == sizeof(int))
                return DataUtils.ToByteArray(vals.Select(ParseInt).ToArray());
            if (size == sizeof(long))
                return DataUtils.ToByteArray(vals.Select(ParseLong).ToArray());
            throw new ArgumentException(languageManager.GetText("EditHex_Convert", args: new Object[] { size }));
        }

        private sbyte ParseByte(string text)
        {
            if (sbyte.TryParse(text, out sbyte val))
                return val;
            return 0;
        }

        private short ParseShort(string text)
        {
            if (short.TryParse(text, out short val))
                return val;
            return 0;
        }

        private int ParseInt(string text)
        {
            if (int.TryParse(text, out int val))
                return val;
            return 0;
        }

        private long ParseLong(string text)
        {
            if (long.TryParse(text, out long val))
                return val;
            return 0;
        }

        private void EditHexWindow_Load(object sender, EventArgs e)
        {
            TabView_SelectedIndexChanged(sender, e);
        }
    }
}