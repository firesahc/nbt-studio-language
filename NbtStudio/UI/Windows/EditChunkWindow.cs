﻿using fNbt;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace NbtStudio.UI
{
    public partial class EditChunkWindow : Form
    {
        private readonly Chunk WorkingChunk;
        private readonly RegionFile ChunkRegion;
        private readonly ChunkCoordsEditControls Manager;

        private EditChunkWindow(IconSource source, Chunk chunk, RegionFile region, ChunkEditPurpose purpose)
        {
            InitializeComponent();

            WorkingChunk = chunk;
            ChunkRegion = region;
            Manager = new ChunkCoordsEditControls(chunk, region, XBox, ZBox);

            this.Icon = source.GetImage(IconType.Chunk).Icon;
            if (purpose == ChunkEditPurpose.Create)
                this.Text = languageManager.GetText("Create_Chunk");
            else if (purpose == ChunkEditPurpose.Move)
                this.Text = languageManager.GetText("Move_Chunk");

            XBox.Select();
        }

        public static Chunk CreateChunk(IconSource source, RegionFile parent, bool bypass_window = false, NbtCompound data = null)
        {
            var chunk = Chunk.EmptyChunk(data);

            if (bypass_window)
            {
                // find first available slot
                var available = parent.GetAvailableCoords();
                if (!available.Any())
                    return null;
                var (x, y) = available.First();
                chunk.Move(x, y);
                return chunk;
            }
            else
            {
                var window = new EditChunkWindow(source, chunk, parent, ChunkEditPurpose.Create);
                return window.ShowDialog() == DialogResult.OK ? chunk : null;
            }
        }

        public static bool MoveChunk(IconSource source, Chunk existing)
        {
            var region = existing.Region;
            var window = new EditChunkWindow(source, existing, region, ChunkEditPurpose.Move);
            return window.ShowDialog() == DialogResult.OK; // window moves the chunk by itself
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
            if (!Manager.CheckCoords())
                return false;
            Manager.ApplyCoords();
            return true;
        }

        private void ButtonOk_Click(object sender, EventArgs e)
        {
            Confirm();
        }
    }

    public enum ChunkEditPurpose
    {
        Create,
        Move
    }
}