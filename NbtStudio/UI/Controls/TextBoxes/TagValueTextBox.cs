﻿using fNbt;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TryashtarUtils.Nbt;

namespace NbtStudio.UI
{
    public class TagValueTextBox : ConvenienceTextBox
    {
        private NbtTag NbtTag;
        private NbtContainerTag NbtParent;
        public TagValueTextBox()
        {
            this.TextChanged += TagValueTextBox_TextChanged;
        }

        private void TagValueTextBox_TextChanged(object sender, EventArgs e)
        {
            SetColor(CheckValueInternal(out _));
        }

        private void SetColor(ValueCheckResult result)
        {
            switch (result)
            {
                case ValueCheckResult.InvalidFormat:
                case ValueCheckResult.InvalidOutOfRange:
                case ValueCheckResult.InvalidUnknown:
                    SetBackColor(Color.FromArgb(255, 230, 230));
                    break;
                case ValueCheckResult.Valid:
                    RestoreBackColor();
                    break;
            }
        }

        private void ShowTooltip(ValueCheckResult result)
        {
            if (result == ValueCheckResult.InvalidFormat)
                ShowTooltip(languageManager.GetText("Invalid_Format"),languageManager.GetText("Invalid_Format_Detail", args: new Object[] { NbtUtil.TagTypeName(NbtTag.TagType).ToLower()}), TimeSpan.FromSeconds(2));
            else if (result == ValueCheckResult.InvalidOutOfRange)
            {
                var (min, max) = NbtUtil.MinMaxFor(NbtTag.TagType);
                ShowTooltip(languageManager.GetText("Out_of_Range"),languageManager.GetText("Out_of_Range_Detail", args: new Object[] { NbtUtil.TagTypeName(NbtTag.TagType).ToLower(),min,max}), TimeSpan.FromSeconds(4));
            }
            else if (result == ValueCheckResult.InvalidUnknown)
                ShowTooltip(languageManager.GetText("Unknown_Error"),languageManager.GetText("Unknown_Error_Detail"), TimeSpan.FromSeconds(2));
        }

        public void SetTags(NbtTag tag, NbtContainerTag parent, bool fill_current_value)
        {
            NbtTag = tag;
            NbtParent = parent;
            if (fill_current_value)
                this.Text = tag.ToSnbt(SnbtOptions.MultilinePreview);
        }

        public string GetValueText()
        {
            return this.Text.Trim().Replace("\r", "");
        }

        private object GetValue()
        {
            var text = GetValueText();
            if (text == "")
                return null;
            return NbtUtil.ParseValue(text, NbtTag.TagType);
        }

        private ValueCheckResult CheckValueInternal(out object value)
        {
            value = null;
            try
            { value = GetValue(); }
            catch (FormatException)
            { return ValueCheckResult.InvalidFormat; }
            catch (OverflowException)
            { return ValueCheckResult.InvalidOutOfRange; }
            catch
            { return ValueCheckResult.InvalidUnknown; }
            return ValueCheckResult.Valid;
        }

        public bool CheckValue(out object value)
        {
            var result = CheckValueInternal(out value);
            bool valid = result == ValueCheckResult.Valid;
            SetColor(result);
            if (!valid)
            {
                ShowTooltip(result);
                this.Select();
            }
            return valid;
        }

        public void ApplyValue()
        {
            CheckValueInternal(out var value);
            ApplyValue(value);
        }

        public void ApplyValue(object value)
        {
            if (value is null)
                NbtUtil.ResetValue(NbtTag);
            else
            {
                var current = NbtUtil.GetValue(NbtTag);
                if (!current.Equals(value))
                    NbtUtil.SetValue(NbtTag, value);
            }
        }

        public enum ValueCheckResult
        {
            Valid,
            InvalidFormat,
            InvalidOutOfRange,
            InvalidUnknown
        }
    }
}
