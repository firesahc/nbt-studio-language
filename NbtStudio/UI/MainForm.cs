using fNbt;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Aga.Controls.Tree;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Collections.Specialized;
using System.Diagnostics;
using TryashtarUtils.Utility;
using TryashtarUtils.Nbt;
using TryashtarUtils.Forms;
using NBTStudio;

namespace NbtStudio.UI
{
    public partial class MainForm : Form
    {
        private NbtTreeModel _ViewModel;
        private NbtTreeModel ViewModel
        {
            get => _ViewModel;
            set
            {
                if (_ViewModel is not null)
                {
                    _ViewModel.Changed -= ViewModel_Changed;
                }

                _ViewModel = value;
                NbtTree.Model = _ViewModel;

                _ViewModel.Changed += ViewModel_Changed;

                ViewModel_Changed(this, EventArgs.Empty);
            }
        }

        private UndoHistory UndoHistory => ViewModel.UndoHistory;
        private IconSource IconSource;

        private readonly Dictionary<NbtTagType, DualMenuItem> CreateTagButtons;
        private readonly string[] ClickedFiles;

        private readonly DualItemCollection ItemCollection;
        private readonly DualMenuItem ActionNew = new DualMenuItem(LocalizationManager.GetText("New"),LocalizationManager.GetText("New_File"), IconType.NewFile, Keys.Control | Keys.N);
        private readonly DualMenuItem ActionNewClipboard = DualMenuItem.SingleMenuItem(LocalizationManager.GetText("New_from_Clipboard"), IconType.Paste, Keys.Control | Keys.Alt | Keys.V);
        private readonly DualMenuItem ActionNewRegion = DualMenuItem.SingleMenuItem(LocalizationManager.GetText("New_Region_File"), IconType.Region, Keys.Control | Keys.Alt | Keys.R);
        private readonly DualMenuItem ActionOpenFile = new DualMenuItem(LocalizationManager.GetText("Open_File"),LocalizationManager.GetText("Open_File"), IconType.OpenFile, Keys.Control | Keys.O);
        private readonly DualMenuItem ActionOpenFolder = new DualMenuItem(LocalizationManager.GetText("Open_Folder"),LocalizationManager.GetText("Open_Folder"), IconType.OpenFolder, Keys.Control | Keys.Shift | Keys.O);
        private readonly DualMenuItem ActionSave = new DualMenuItem(LocalizationManager.GetText("Save"),LocalizationManager.GetText("Save"),IconType.Save, Keys.Control | Keys.S);
        private readonly DualMenuItem ActionSaveAs = DualMenuItem.SingleMenuItem(LocalizationManager.GetText("Save_As"), IconType.Save, Keys.Control | Keys.Shift | Keys.S);
        private readonly DualMenuItem DropDownRecent = DualMenuItem.SingleMenuItem(LocalizationManager.GetText("Recent"), null, Keys.None);
        private readonly DualMenuItem DropDownImport = DualMenuItem.SingleMenuItem(LocalizationManager.GetText("Import"), null, Keys.None);
        private readonly DualMenuItem ActionImportFile = DualMenuItem.SingleMenuItem(LocalizationManager.GetText("Import_File"), IconType.OpenFile, Keys.Control | Keys.I);
        private readonly DualMenuItem ActionImportFolder = DualMenuItem.SingleMenuItem(LocalizationManager.GetText("Import_Folder"), IconType.OpenFolder, Keys.Control | Keys.Shift | Keys.I);
        private readonly DualMenuItem ActionImportNew = DualMenuItem.SingleMenuItem(LocalizationManager.GetText("Import_New_File"), IconType.NewFile, Keys.Control | Keys.Alt | Keys.N);
        private readonly DualMenuItem ActionImportNewRegion = DualMenuItem.SingleMenuItem(LocalizationManager.GetText("Import_New_Region_File"), IconType.Region, Keys.None);
        private readonly DualMenuItem ActionImportClipboard = DualMenuItem.SingleMenuItem(LocalizationManager.GetText("Import_From_Clipboard"), IconType.Paste, Keys.Control | Keys.Alt | Keys.I);
        private readonly DualMenuItem ActionSort = DualMenuItem.SingleButton(LocalizationManager.GetText("Sort"), IconType.Sort);
        private readonly DualMenuItem ActionRefresh = DualMenuItem.SingleButton(LocalizationManager.GetText("Refresh"), IconType.Refresh);
        private readonly DualMenuItem ActionUndo = DualMenuItem.SingleMenuItem(LocalizationManager.GetText("Undo"), IconType.Undo, Keys.Control | Keys.Z);
        private readonly DualMenuItem ActionRedo = DualMenuItem.SingleMenuItem(LocalizationManager.GetText("Redo"), IconType.Redo, Keys.Control | Keys.Shift | Keys.Z);
        private readonly DualMenuItem ActionCut = new DualMenuItem(LocalizationManager.GetText("Cut"), LocalizationManager.GetText("Cut"),  IconType.Cut, Keys.Control | Keys.X);
        private readonly DualMenuItem ActionCopy = new DualMenuItem(LocalizationManager.GetText("Copy"),LocalizationManager.GetText("Copy"), IconType.Copy, Keys.Control | Keys.C);
        private readonly DualMenuItem ActionPaste = new DualMenuItem(LocalizationManager.GetText("Paste"),LocalizationManager.GetText("Paste"), IconType.Paste, Keys.Control | Keys.V);
        private readonly DualMenuItem ActionRename = new DualMenuItem(LocalizationManager.GetText("Rename"),LocalizationManager.GetText("Rename"), IconType.Rename, Keys.F2);
        private readonly DualMenuItem ActionEdit = new DualMenuItem(LocalizationManager.GetText("Edit_Value"),LocalizationManager.GetText("Edit_Value"), IconType.Edit, Keys.Control | Keys.E);
        private readonly DualMenuItem ActionEditSnbt = new DualMenuItem(LocalizationManager.GetText("Edit_as_SNBT"), LocalizationManager.GetText("Edit_as_SNBT"), IconType.EditSnbt, Keys.Control | Keys.Shift | Keys.E);
        private readonly DualMenuItem ActionDelete = new DualMenuItem(LocalizationManager.GetText("Delete"),LocalizationManager.GetText("Delete"), IconType.Delete, Keys.Delete);
        private readonly DualMenuItem DropDownUndoHistory = DualMenuItem.SingleMenuItem(LocalizationManager.GetText("Undo_History"), IconType.Undo, Keys.None);
        private readonly DualMenuItem DropDownRedoHistory = DualMenuItem.SingleMenuItem(LocalizationManager.GetText("Redo_History"), IconType.Redo, Keys.None);
        private readonly DualMenuItem ActionClearUndoHistory = DualMenuItem.SingleMenuItem(LocalizationManager.GetText("Clear_Undo_History"), null, Keys.None);
        private readonly DualMenuItem ActionFind = new DualMenuItem(LocalizationManager.GetText("Find"), LocalizationManager.GetText("Find"), IconType.Search, Keys.Control | Keys.F);
        private readonly DualMenuItem ActionAbout = DualMenuItem.SingleMenuItem(LocalizationManager.GetText("About"), IconType.NbtStudio, Keys.Shift | Keys.F1);
        private readonly DualMenuItem ActionLanguage = DualMenuItem.SingleMenuItem(LocalizationManager.GetText("Language"), null, Keys.None);
        private readonly DualMenuItem ActionChangeIcons = DualMenuItem.SingleMenuItem(LocalizationManager.GetText("Change_Icons"), IconType.Refresh, Keys.Control | Keys.I);
        private readonly DualMenuItem ActionAddSnbt = DualMenuItem.SingleButton(LocalizationManager.GetText("Add_as_SNBT"), IconType.AddSnbt);
        private readonly DualMenuItem ActionAddChunk = DualMenuItem.SingleButton(LocalizationManager.GetText("Add_Chunk"), IconType.Chunk);
        private readonly DualMenuItem ActionUpdate = DualMenuItem.SingleMenuItem(LocalizationManager.GetText("Update"), null, Keys.None);
        private readonly DualMenuItem ActionCheckUpdates = DualMenuItem.SingleMenuItem(LocalizationManager.GetText("Check_for_Updates"), null, Keys.Control | Keys.U);
        public MainForm(string[] args)
        {
            ClickedFiles = args;
            if (Properties.Settings.Default.RecentFiles is null)
                Properties.Settings.Default.RecentFiles = new StringCollection();
            if (Properties.Settings.Default.CustomIconSets is null)
                Properties.Settings.Default.CustomIconSets = new StringCollection();

            // stuff from the designer
            InitializeComponent();

            // stuff excluded from the designer for cleaner/less duplicated code
            ActionNew.Click += (s, e) => New();
            ActionNewClipboard.Click += (s, e) => NewPaste();
            ActionNewRegion.Click += (s, e) => NewRegion();
            ActionOpenFile.Click += (s, e) => OpenFile();
            ActionOpenFolder.Click += (s, e) => OpenFolder();
            ActionImportFile.Click += (s, e) => ImportFile();
            ActionImportFolder.Click += (s, e) => ImportFolder();
            ActionImportNew.Click += (s, e) => ImportNew();
            ActionImportNewRegion.Click += (s, e) => ImportNewRegion();
            ActionImportClipboard.Click += (s, e) => ImportClipboard();
            ActionSave.Click += (s, e) => Save();
            ActionSaveAs.Click += (s, e) => SaveAs();
            ActionSort.Click += (s, e) => Sort();
            ActionRefresh.Click += (s, e) => RefreshAll();
            ActionUndo.Click += (s, e) => Undo();
            ActionRedo.Click += (s, e) => Redo();
            ActionClearUndoHistory.Click += (s, e) => ClearUndoHistory();
            ActionCut.Click += (s, e) => Cut();
            ActionCopy.Click += (s, e) => Copy();
            ActionPaste.Click += (s, e) => Paste();
            ActionRename.Click += (s, e) => Rename();
            ActionEdit.Click += (s, e) => Edit();
            ActionEditSnbt.Click += (s, e) => EditSnbt();
            ActionDelete.Click += (s, e) => Delete();
            ActionFind.Click += (s, e) => Find();
            ActionAbout.Click += (s, e) => About();
            ActionLanguage.Click += (s, e) => Language();
            ActionChangeIcons.Click += (s, e) => ChangeIcons();
            ActionAddSnbt.Click += (s, e) => AddSnbt();
            ActionAddChunk.Click += (s, e) => AddChunk();
            ActionUpdate.Click += (s, e) => ShowUpdate();
            ActionCheckUpdates.Click += (s, e) => CheckForUpdates();

            ActionNew.AddTo(Tools, MenuFile);
            ActionNewRegion.AddToMenuItem(MenuFile);
            ActionNewClipboard.AddToMenuItem(MenuFile);
            MenuFile.DropDownItems.Add(new ToolStripSeparator());
            ActionOpenFile.AddTo(Tools, MenuFile);
            ActionOpenFolder.AddTo(Tools, MenuFile);
            DropDownImport.AddToMenuItem(MenuFile);
            ActionImportFile.AddToDual(DropDownImport);
            ActionImportFolder.AddToDual(DropDownImport);
            ActionImportNew.AddToDual(DropDownImport);
            ActionImportNewRegion.AddToDual(DropDownImport);
            ActionImportClipboard.AddToDual(DropDownImport);
            MenuFile.DropDownItems.Add(new ToolStripSeparator());
            ActionSave.AddTo(Tools, MenuFile);
            ActionSaveAs.AddToMenuItem(MenuFile);
            MenuFile.DropDownItems.Add(new ToolStripSeparator());
            DropDownRecent.AddToMenuItem(MenuFile);
            ActionRefresh.AddToToolStrip(Tools);
            Tools.Items.Add(new ToolStripSeparator());
            ActionUndo.AddToMenuItem(MenuEdit);
            ActionRedo.AddToMenuItem(MenuEdit);
            MenuEdit.DropDownItems.Add(new ToolStripSeparator());
            ActionCut.AddTo(Tools, MenuEdit);
            ActionCopy.AddTo(Tools, MenuEdit);
            ActionPaste.AddTo(Tools, MenuEdit);
            MenuEdit.DropDownItems.Add(new ToolStripSeparator());
            Tools.Items.Add(new ToolStripSeparator());
            ActionRename.AddTo(Tools, MenuEdit);
            ActionEdit.AddTo(Tools, MenuEdit);
            ActionEditSnbt.AddTo(Tools, MenuEdit);
            ActionDelete.AddTo(Tools, MenuEdit);
            ActionSort.AddToToolStrip(Tools);
            MenuEdit.DropDownItems.Add(new ToolStripSeparator());
            DropDownUndoHistory.AddToMenuItem(MenuEdit);
            DropDownRedoHistory.AddToMenuItem(MenuEdit);
            ActionClearUndoHistory.AddToMenuItem(MenuEdit);
            Tools.Items.Add(new ToolStripSeparator());
            ActionAddChunk.AddToToolStrip(Tools);
            ActionAbout.AddToMenuItem(MenuHelp);
            ActionLanguage.AddToMenuItem(MenuHelp);
            ActionChangeIcons.AddToMenuItem(MenuHelp);
            ActionUpdate.Visible = false;
            ActionUpdate.AddToMenuStrip(MenuStrip);
            MenuHelp.DropDownItems.Add(new ToolStripSeparator());
            ActionCheckUpdates.AddToMenuItem(MenuHelp);

            CreateTagButtons = MakeCreateTagButtons();
            foreach (var item in CreateTagButtons.Values)
            {
                item.AddToToolStrip(Tools);
            }
            ActionAddSnbt.AddToToolStrip(Tools);

            Tools.Items.Add(new ToolStripSeparator());
            ActionFind.AddTo(Tools, MenuSearch);

            ViewModel = new NbtTreeModel();
            NbtTree.Font = new Font(NbtTree.Font.FontFamily, Properties.Settings.Default.TreeZoom);

            ItemCollection = new DualItemCollection(
                ActionNew, ActionNewClipboard, ActionNewRegion,
                ActionOpenFile, ActionOpenFolder, DropDownImport,
                ActionImportFile, ActionImportFolder, ActionImportClipboard,
                ActionImportNew, ActionImportNewRegion, ActionSave,
                ActionSaveAs, DropDownRecent, ActionSort, ActionRefresh,
                ActionUndo, ActionRedo, ActionCut,
                ActionCopy, ActionPaste, ActionRename,
                ActionEdit, ActionEditSnbt, ActionDelete,
                DropDownUndoHistory, DropDownRedoHistory, ActionClearUndoHistory,
                ActionFind, ActionAbout, ActionAddSnbt, ActionAddChunk,
                ActionChangeIcons, ActionUpdate, ActionCheckUpdates
            );
            ItemCollection.AddRange(CreateTagButtons.Values);

            foreach (var item in Properties.Settings.Default.CustomIconSets.Cast<string>().ToList())
            {
                var attempt = IconSetWindow.TryImportSource(item);
                if (attempt.Failed)
                    IconSetWindow.ShowImportFailed(item, attempt, this);
            }
            SetIconSource(IconSourceRegistry.FromID(Properties.Settings.Default.IconSet));

            UpdateChecker = new Task<AvailableUpdate>(() => Updater.CheckForUpdates());
            UpdateChecker.Start();
            UpdateChecker.ContinueWith(x =>
            {
                if (x.Status == TaskStatus.RanToCompletion && x.Result is not null)
                {
                    ReadyUpdate = x.Result;
                    ActionUpdate.Visible = true;
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());

            NbtTree.NodeAdded += NbtTree_NodeAdded;
        }

        private void NbtTree_NodeAdded(object sender, TreeNodeAdv e)
        {
            if (NbtTree.INodeFromNode(e) is FolderNode folder)
                folder.Folder.FilesFailed += Folder_FilesFailed;
        }

        private Task<AvailableUpdate> UpdateChecker;
        private AvailableUpdate ReadyUpdate;
        private void CheckForUpdates()
        {
            if (UpdateChecker is not null && !UpdateChecker.IsCompleted)
                return;
            UpdateChecker = new Task<AvailableUpdate>(() => Updater.CheckForUpdates());
            UpdateChecker.Start();
            UpdateChecker.ContinueWith(x =>
            {
                if (x.Status == TaskStatus.Faulted)
                {
                    var window = new ExceptionWindow(LocalizationManager.GetText("Update_check_failed"),
                        LocalizationManager.GetText("Update_check_failed_Detail"),
                        FailableFactory.Failure(x.Exception, LocalizationManager.GetText("Check_for_Updates")),
                        LocalizationManager.GetText("Update_Page") +
                        "https://github.com/tryashtar/nbt-studio/releases",
                        ExceptionWindowButtons.OKCancel
                    );
                    window.ShowDialog(this);
                    if (window.DialogResult == DialogResult.OK)
                        IOUtils.OpenUrlInBrowser("https://github.com/tryashtar/nbt-studio/releases");
                }
                else if (x.Status == TaskStatus.RanToCompletion)
                {
                    if (x.Result is null)
                    {
                        if (MessageBox.Show(LocalizationManager.GetText("Latest_Update") +
                            LocalizationManager.GetText("Update_Page") +
                            "https://github.com/tryashtar/nbt-studio/releases",
                            LocalizationManager.GetText("No_update_found"), MessageBoxButtons.OKCancel) == DialogResult.OK)
                            IOUtils.OpenUrlInBrowser("https://github.com/tryashtar/nbt-studio/releases");
                    }
                    else
                    {
                        ReadyUpdate = x.Result;
                        ActionUpdate.Visible = true;
                        ShowUpdate();
                    }
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void SetIconSource(IconSource source)
        {
            IconSource = source;
            ItemCollection.SetIconSource(source);
            NbtTree.SetIconSource(source);
            NbtTree.Refresh();
            this.Icon = source.GetImage(IconType.NbtStudio).Icon;
            Properties.Settings.Default.IconSet = IconSourceRegistry.GetID(source);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            NbtTree_SelectionChanged(this, EventArgs.Empty);
            ViewModel_Changed(this, EventArgs.Empty);
            if (ClickedFiles is not null && ClickedFiles.Any())
                OpenFiles(ClickedFiles);
        }

        private void New()
        {
            if (!ConfirmIfUnsaved(LocalizationManager.GetText("Create_New_File_Anyway")))
                return;
            OpenFile(new NbtFile(), skip_confirm: true);
        }

        private void ImportNew()
        {
            ViewModel.Import(new NbtFile());
        }

        private void NewRegion()
        {
            if (!ConfirmIfUnsaved(LocalizationManager.GetText("Create_New_File_Anyway")))
                return;
            OpenFile(RegionFile.EmptyRegion(), skip_confirm: true);
        }

        private void ImportNewRegion()
        {
            ViewModel.Import(RegionFile.EmptyRegion());
        }

        private void PasteLike(Action<IEnumerable<string>> when_paths, Action<NbtFile> when_file)
        {
            if (Clipboard.ContainsFileDropList())
            {
                var files = Clipboard.GetFileDropList();
                when_paths(files.Cast<string>());
            }
            else if (Clipboard.ContainsText())
            {
                var text = Clipboard.GetText();
                var attempt1 = SnbtParser.TryParse(text, named: false);
                if (!attempt1.Failed)
                    PasteTagLike(attempt1.Result, when_file);
                else
                {
                    var attempt2 = SnbtParser.TryParse(text, named: true);
                    if (!attempt2.Failed)
                        PasteTagLike(attempt2.Result, when_file);
                    else
                    {
                        var error = FailableFactory.Aggregate(attempt1, attempt2);
                        var window = new ExceptionWindow(LocalizationManager.GetText("Clipboard_error"),LocalizationManager.GetText("Clipboard_error_Detail"), error);
                        window.ShowDialog(this);
                    }
                }
            }
        }

        private void PasteTagLike(NbtTag tag, Action<NbtFile> when_file)
        {
            if (tag is NbtCompound compound)
                when_file(new NbtFile(compound));
            else
            {
                var root = new NbtCompound();
                tag.Name = NbtUtil.GetAutomaticName(tag, root);
                root.Add(tag);
                when_file(new NbtFile(root));
            }
        }

        private void NewPaste()
        {
            PasteLike(x => OpenFiles(x), x => OpenFile(x));
        }

        private void ImportClipboard()
        {
            PasteLike(x => ImportFiles(x), x => ImportFile(x));
        }

        private void BrowseFileLike(Action<string[]> then)
        {
            using (var dialog = new OpenFileDialog
            {
                Title = LocalizationManager.GetText("Select_NBT_files"),
                RestoreDirectory = true,
                Multiselect = true,
                Filter = NbtUtil.OpenFilter()
            })
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                    then(dialog.FileNames);
            }
        }

        private void BrowseFolderLike(Action<string> then)
        {
            using (var dialog = new CommonOpenFileDialog
            {
                Title = LocalizationManager.GetText("Select_NBT_folder"),
                RestoreDirectory = true,
                Multiselect = false,
                IsFolderPicker = true
            })
            {
                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                    then(dialog.FileName);
            }
        }

        private void OpenFile()
        {
            if (!ConfirmIfUnsaved(LocalizationManager.GetText("Open_New_File_Anyway")))
                return;
            BrowseFileLike(x => OpenFiles(x, skip_confirm: true));
        }

        private void OpenFile(ISaveable file, bool skip_confirm = false)
        {
            if (!skip_confirm && !ConfirmIfUnsaved(LocalizationManager.GetText("Open_New_File_Anyway")))
                return;
            ViewModel = new NbtTreeModel(file);
        }

        private void ImportFile()
        {
            BrowseFileLike(x => ImportFiles(x));
        }

        private void ImportFile(ISaveable file)
        {
            ViewModel.Import(file);
        }

        private void OpenFolder()
        {
            if (!ConfirmIfUnsaved(LocalizationManager.GetText("Open_New_Folder_Anyway")))
                return;
            BrowseFolderLike(x => OpenFolder(x, skip_confirm: true));
        }

        private void ImportFolder()
        {
            BrowseFolderLike(x => ImportFolder(x));
        }

        private void Discard(IEnumerable<INode> nodes)
        {
            var unsaved = nodes.Filter(x => x.Get<ISaveable>()).Where(x => x.HasUnsavedChanges);
            if (!unsaved.Any() || MessageBox.Show(LocalizationManager.GetText("Unsaved_Changes_Detail.1"), LocalizationManager.GetText("Unsaved_Changes"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                ViewModel.RemoveMany(nodes);
        }

        private void RefreshItems(IEnumerable<IRefreshable> items)
        {
            items = items.Where(x => x.CanRefresh);
            var unsaved = items.OfType<ISaveable>().Where(x => x.HasUnsavedChanges);
            if (!unsaved.Any() || MessageBox.Show(LocalizationManager.GetText("Unsaved_Changes_Detail.1"),  LocalizationManager.GetText("Unsaved_Changes"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                UndoHistory.StartBatchOperation();
                var errors = new List<(IHavePath item, Exception exception)>();
                foreach (var item in items)
                {
                    try
                    {
                        item.Refresh();
                    }
                    catch (Exception ex)
                    {
                        errors.Add((item as IHavePath, ex));
                    }
                }
                UndoHistory.FinishBatchOperation(new DescriptionHolder("Refresh {0}", items.ToArray()), true);
                if (errors.Any())
                {
                    var error = FailableFactory.AggregateFailure(errors.Select(x => x.exception).ToArray());
                    string message = LocalizationManager.GetText("Refresh_Failed", args: new Object[]{StringUtils.Pluralize(errors.Count(), "file")} );
                    message += String.Join("\n", errors.Select(x => x.item).Where(x => x is not null).Select(x => Path.GetFileName(x.Path)));
                    var window = new ExceptionWindow("Refresh error", message, error);
                    window.ShowDialog(this);
                }
            }
        }

        private void Save()
        {
            foreach (var file in ViewModel.OpenedFiles)
            {
                if (!Save(file))
                    break;
            }
        }

        private void SaveAs()
        {
            foreach (var file in ViewModel.OpenedFiles)
            {
                if (!SaveAs(file))
                    break;
            }
        }

        private bool Save(ISaveable file)
        {
            if (file.CanSave)
            {
                file.Save();
                NbtTree.Refresh();
                return true;
            }
            else if (file is IExportable exp)
                return SaveAs(exp);
            return false;
        }

        private bool SaveAs(IExportable file)
        {
            string path = null;
            if (file is IHavePath has_path)
                path = has_path.Path;
            using var dialog = new SaveFileDialog
            {
                Title = path is null ? "Save NBT file" : $"Save {Path.GetFileName(path)} as...",
                RestoreDirectory = true,
                FileName = path,
                Filter = NbtUtil.SaveFilter(path, NbtUtil.GetFileType(file))
            };
            if (path is not null)
            {
                dialog.InitialDirectory = Path.GetDirectoryName(path);
                dialog.FileName = Path.GetFileName(path);
            }
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                if (file is NbtFile nbtfile)
                {
                    var export = new ExportWindow(IconSource, nbtfile.ExportSettings, dialog.FileName);
                    if (export.ShowDialog() == DialogResult.OK)
                    {
                        nbtfile.SaveAs(dialog.FileName, export.GetSettings());
                        Properties.Settings.Default.RecentFiles.Add(dialog.FileName);
                        return true;
                    }
                }
                else
                {
                    file.SaveAs(dialog.FileName);
                    Properties.Settings.Default.RecentFiles.Add(dialog.FileName);
                        return true;
                }
            }
            return false;
        }

        private void OpenInExplorer(IHavePath file)
        {
            var info = new ProcessStartInfo { FileName = "explorer", Arguments = $"/select, \"{file.Path}\"" };
            Process.Start(info);
        }

        private void Sort()
        {
            var obj = NbtTree.SelectedINode;
            if (obj is null || !obj.CanSort) return;
            UndoHistory.StartBatchOperation();
            obj.Sort();
            UndoHistory.FinishBatchOperation(new DescriptionHolder("Sort {0}", obj), true);
        }

        private void RefreshAll()
        {
            RefreshItems(ViewModel.OpenedFiles);
        }

        private void Undo()
        {
            UndoHistory.Undo();
        }

        private void Redo()
        {
            UndoHistory.Redo();
        }

        private void ClearUndoHistory()
        {
            UndoHistory.Clear();
        }

        private void CopyLike(Func<INode, bool> check, Func<INode, DataObject> perform)
        {
            var objs = NbtTree.SelectedINodes.Where(check).ToList();
            if (objs.Any())
            {
                var data = objs.Select(perform).Aggregate((x, y) => Utils.Merge(x, y));
                Clipboard.SetDataObject(data);
            }
        }

        private void Cut()
        {
            CopyLike(x => x.CanCut, x => x.Cut());
        }

        private void Copy()
        {
            CopyLike(x => x.CanCopy, x => x.Copy());
        }

        private void Paste()
        {
            var parent = NbtTree.SelectedINode;
            if (parent is null) return;
            Paste(parent);
        }

        private void Paste(INode node)
        {
            if (!node.CanPaste)
                return;
            IEnumerable<INode> results = Enumerable.Empty<INode>();
            UndoHistory.StartBatchOperation();
            try
            { results = node.Paste(Clipboard.GetDataObject()); }
            catch (Exception ex)
            {
                if (!(ex is OperationCanceledException))
                {
                    var error = FailableFactory.Failure(ex, "Pasting");
                    var window = new ExceptionWindow("Error while pasting", "An error occurred while pasting:", error);
                    window.ShowDialog(this);
                }
            }
            UndoHistory.FinishBatchOperation(new DescriptionHolder("Paste {0} into {1}", results, node), true);
        }

        private void Rename()
        {
            var items = NbtTree.SelectedINodes;
            if (ListUtils.ExactlyOne(items))
                Rename(items.Single());
            else
                BulkRename(items.Filter(x => x.GetNbtTag()));
        }

        private void Edit()
        {
            var items = NbtTree.SelectedINodes;
            if (ListUtils.ExactlyOne(items))
                Edit(items.Single());
            else
                BulkEdit(items.Filter(x => x.GetNbtTag()));
        }

        private void BulkRename(IEnumerable<NbtTag> tags)
        {
            UndoHistory.StartBatchOperation();
            var changed = BulkEditWindow.BulkRename(IconSource, tags);
            UndoHistory.FinishBatchOperation(new DescriptionHolder("Bulk rename {0}", changed), false);
        }

        private void BulkEdit(IEnumerable<NbtTag> tags)
        {
            UndoHistory.StartBatchOperation();
            var changed = BulkEditWindow.BulkEdit(IconSource, tags);
            UndoHistory.FinishBatchOperation(new DescriptionHolder("Bulk edit {0}", changed), false);
        }

        private void EditLike(INode node, Predicate<INode> check, Action<NbtTag> when_tag)
        {
            if (!check(node)) return;
            var chunk = node.Get<Chunk>();
            var path = node.Get<IHavePath>();
            var tag = node.GetNbtTag();
            // batch operation to combine the rename and value change into one undo
            UndoHistory.StartBatchOperation();
            if (path is not null)
                RenameFile(path);
            if (chunk is not null)
                EditChunk(chunk);
            else if (tag is not null)
                when_tag(tag);
            UndoHistory.FinishBatchOperation(new DescriptionHolder("Edit {0}", node), false);
        }

        private void Rename(INode node)
        {
            EditLike(node, x => x.CanRename, RenameTag);
        }

        private void Edit(INode node)
        {
            EditLike(node, x => x.CanEdit, EditTag);
        }

        private void RenameFile(IHavePath item)
        {
            if (item.Path is not null)
                RenameFileWindow.RenameFile(IconSource, item);
        }

        private void EditTag(NbtTag tag)
        {
            if (ByteProviders.HasProvider(tag))
                EditHexWindow.ModifyTag(IconSource, tag, EditPurpose.EditValue);
            else
                EditTagWindow.ModifyTag(IconSource, tag, EditPurpose.EditValue);
        }

        private void EditChunk(Chunk chunk)
        {
            EditChunkWindow.MoveChunk(IconSource, chunk);
        }

        private void RenameTag(NbtTag tag)
        {
            // likewise
            UndoHistory.StartBatchOperation();
            EditTagWindow.ModifyTag(IconSource, tag, EditPurpose.Rename);
            UndoHistory.FinishBatchOperation(new DescriptionHolder("Rename {0}", tag), false);
        }

        private void EditSnbt()
        {
            var tag = NbtTree.SelectedINode?.GetNbtTag();
            if (tag is null) return;
            UndoHistory.StartBatchOperation();
            EditSnbtWindow.ModifyTag(IconSource, tag, EditPurpose.EditValue);
            UndoHistory.FinishBatchOperation(new DescriptionHolder("Edit {0} as SNBT", tag), false);
        }

        private void Delete()
        {
            var selected_nodes = NbtTree.SelectedNodes;
            var nexts = selected_nodes.Select(x => x.NextNode).Where(x => x is not null).ToList();
            var prevs = selected_nodes.Select(x => x.PreviousNode).Where(x => x is not null).ToList();
            var parents = selected_nodes.Select(x => x.Parent).Where(x => x is not null).ToList();

            var selected_objects = NbtTree.SelectedINodes.ToList();
            Delete(selected_objects);

            // Index == -1 checks whether this node has been removed from the tree
            if (selected_nodes.All(x => x.Index == -1))
            {
                var select_next = nexts.FirstOrDefault(x => x.Index != -1) ?? prevs.FirstOrDefault(x => x.Index != -1) ?? parents.FirstOrDefault(x => x.Index != -1);
                if (select_next is not null)
                    select_next.IsSelected = true;
            }
        }

        private void Delete(IEnumerable<INode> nodes)
        {
            nodes = nodes.Where(x => x.CanDelete);
            var file_nodes = nodes.Where(x => x.Get<IHavePath>() is not null);
            var files = nodes.Filter(x => x.Get<IHavePath>());
            if (files.Any())
            {
                DialogResult result;
                if (ListUtils.ExactlyOne(files))
                {
                    var file = files.Single();
                    if (file.Path is null)
                        result = MessageBox.Show(
                            LocalizationManager.GetText("Delete_Item_Confirmation"),
                            LocalizationManager.GetText("Delete_Item_Confirmation_Detail.1"),
                            MessageBoxButtons.YesNo);
                    else
                        result = MessageBox.Show(
                            LocalizationManager.GetText("Delete_Item_Confirmation") + "\n\n" + 
                            LocalizationManager.GetText("Delete_Item_Confirmation_Detail.2"),
                            LocalizationManager.GetText("Delete_Item_Confirmation_Detail.3", args: new Object[] { file_nodes.Single().Description }),
                            MessageBoxButtons.YesNo);
                }
                else
                {
                    var unsaved = files.Where(x => x.Path is null);
                    var saved = files.Where(x => x.Path is not null);
                    if (!saved.Any())
                        result = MessageBox.Show(
                            LocalizationManager.GetText("Delete_Items_Confirmation", args: new Object[] { ExtractNodeOperations.Description(file_nodes) }) ,
                            LocalizationManager.GetText("Delete_Items_Confirmation_Detail.1"),
                            MessageBoxButtons.YesNo);
                    else
                        result = MessageBox.Show(
                            LocalizationManager.GetText("Delete_Items_Confirmation", args: new Object[] { ExtractNodeOperations.Description(file_nodes) }) + "\n\n" +
                            LocalizationManager.GetText("Delete_Items_Confirmation_Detail.2", args: new Object[] { StringUtils.Pluralize(saved.Count(), "item") }),
                            LocalizationManager.GetText("Delete_Items_Confirmation_Detail.1"),
                            MessageBoxButtons.YesNo);
                }
                if (result != DialogResult.Yes)
                    return;
            }
            UndoHistory.StartBatchOperation();
            var errors = new List<Exception>();
            foreach (var node in nodes)
            {
                try
                { node.Delete(); }
                catch (Exception ex)
                { errors.Add(ex); }
            }
            var relevant = errors.Where(x => !(x is OperationCanceledException)).ToArray();
            if (relevant.Any())
            {
                var error = FailableFactory.AggregateFailure(relevant);
                var window = new ExceptionWindow(LocalizationManager.GetText("Error_while_deleting"), LocalizationManager.GetText("Error_while_deleting_Detail"), error);
                window.ShowDialog(this);
            }
            UndoHistory.FinishBatchOperation(new DescriptionHolder("Delete {0}", nodes), false);
        }

        private FindWindow FindWindow;
        private void Find()
        {
            if (FindWindow is null || FindWindow.IsDisposed)
                FindWindow = new FindWindow(IconSource, ViewModel, NbtTree);
            if (!FindWindow.Visible)
                FindWindow.Show(this);
            FindWindow.Focus();
        }

        private AboutWindow AboutWindow;
        private void About()
        {
            if (AboutWindow is null || AboutWindow.IsDisposed)
                AboutWindow = new AboutWindow(IconSource);
            if (!AboutWindow.Visible)
                AboutWindow.Show(this);
            AboutWindow.Focus();
        }

        private LanguageWindow LanguageWindow;

        private void Language()
        {
            if (LanguageWindow is null || LanguageWindow.IsDisposed)
                LanguageWindow = new LanguageWindow(IconSource);
            if (!LanguageWindow.Visible)
                LanguageWindow.Show(this);
            LanguageWindow.Focus();
        }

        private IconSetWindow IconSetWindow;
        private void ChangeIcons()
        {
            if (IconSetWindow is null || IconSetWindow.IsDisposed)
            {
                IconSetWindow = new IconSetWindow(IconSource);
                IconSetWindow.FormClosed += IconSetWindow_FormClosed;
            }
            if (!IconSetWindow.Visible)
                IconSetWindow.Show(this);
            IconSetWindow.Focus();
        }

        private UpdateWindow UpdateWindow;
        private void ShowUpdate()
        {
            if (ReadyUpdate is null)
                return;
            if (UpdateWindow is null || UpdateWindow.IsDisposed)
                UpdateWindow = new UpdateWindow(IconSource, ReadyUpdate);
            if (!UpdateWindow.Visible)
                UpdateWindow.Show(this);
            UpdateWindow.Focus();
        }

        private void IconSetWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (IconSetWindow.SelectedSource is not null)
                SetIconSource(IconSetWindow.SelectedSource);
        }

        private void AddSnbt()
        {
            var parent = NbtTree.SelectedINode?.GetNbtTag() as NbtContainerTag;
            if (parent is null) return;
            var tag = EditSnbtWindow.CreateTag(IconSource, parent);
            if (tag is not null)
                tag.AddTo(parent);
        }

        private void AddChunk()
        {
            var parent = NbtTree.SelectedINode?.Get<RegionFile>();
            if (parent is null) return;
            var chunk = EditChunkWindow.CreateChunk(IconSource, parent, bypass_window: Control.ModifierKeys == Keys.Shift);
            if (chunk is not null)
                chunk.AddTo(parent);
        }

        private void AddTag(NbtTagType type)
        {
            var parent = NbtTree.SelectedINode?.GetNbtTag() as NbtContainerTag;
            if (parent is null) return;
            AddTag(parent, type);
        }

        private void AddTag(NbtContainerTag container, NbtTagType type)
        {
            NbtTag tag;
            if (NbtUtil.IsArrayType(type))
                tag = EditHexWindow.CreateTag(IconSource, type, container, bypass_window: Control.ModifierKeys == Keys.Shift);
            else
                tag = EditTagWindow.CreateTag(IconSource, type, container, bypass_window: Control.ModifierKeys == Keys.Shift);
            if (tag is not null)
                container.Add(tag);
        }

        private Dictionary<NbtTagType, DualMenuItem> MakeCreateTagButtons()
        {
            var buttons = new Dictionary<NbtTagType, DualMenuItem>();
            foreach (var type in NbtUtil.NormalTagTypes())
            {
                var button = DualMenuItem.SingleButton(
                    hover: LocalizationManager.GetText("Add_Tag.1", args: new Object[] { NbtUtil.TagTypeName(type) }),
                    icon: NbtUtil.TagIconType(type));
                button.Click += (s, e) => AddTag(type);
                buttons.Add(type, button);
            }
            return buttons;
        }

        private void OpenPathsLike(IEnumerable<string> paths, Action<IEnumerable<IHavePath>> then)
        {
            var files = paths.Distinct().Select(path => (path, item: NbtFolder.OpenFileOrFolder(Path.GetFullPath(path)))).ToList();
            var bad = files.Where(x => x.item.Failed);
            var good = files.Where(x => !x.item.Failed);
            if (bad.Any())
            {
                string message = LocalizationManager.GetText("Load_Failed", args: new Object[] { StringUtils.Pluralize(bad.Count(), "file") }) + ":\n\n";
                message += String.Join("\n", bad.Select(x => Path.GetFileName(x.path)));
                var fail = FailableFactory.Aggregate(bad.Select(x => x.item).ToArray());
                var window = new ExceptionWindow("Load failure", message, fail);
                window.ShowDialog(this);
            }
            if (good.Any())
            {
                Properties.Settings.Default.RecentFiles.AddRange(good.Select(x => x.path).ToArray());
                var results = good.Select(x => x.item.Result);
                then(results);
            }
        }

        private void Folder_FilesFailed(object sender, IEnumerable<(string path, IFailable<IFile> file)> bad)
        {
            string message =LocalizationManager.GetText("Load_Failed", args: new Object[] { StringUtils.Pluralize(bad.Count(), "file") }) + ":\n\n";
            message += String.Join("\n", bad.Select(x => Path.GetFileName(x.path)));
            var fail = FailableFactory.Aggregate(bad.Select(x => x.file).ToArray());
            var window = new ExceptionWindow("Load failure", message, fail);
            window.ShowDialog(this);
        }

        private void OpenFolder(string path, bool skip_confirm = false)
        {
            if (!skip_confirm && !ConfirmIfUnsaved(LocalizationManager.GetText("Open_New_Folder_Anyway")))
                return;
            OpenPathsLike(new[] { path }, x => ViewModel = new NbtTreeModel(x));
        }

        private void ImportFolder(string path)
        {
            OpenPathsLike(new[] { path }, x => ViewModel.ImportMany(x));
        }

        private void OpenFiles(IEnumerable<string> paths, bool skip_confirm = false)
        {
            if (!skip_confirm && !ConfirmIfUnsaved(LocalizationManager.GetText("Open_New_File_Anyway")))
                return;
            OpenPathsLike(paths, x => ViewModel = new NbtTreeModel(x));
        }

        private void ImportFiles(IEnumerable<string> paths)
        {
            OpenPathsLike(paths, x => ViewModel.ImportMany(x));
        }

        private void OpenRecentFile()
        {
            UpdateRecentFiles();
            var files = Properties.Settings.Default.RecentFiles;
            if (files.Count >= 1)
                OpenFiles(Properties.Settings.Default.RecentFiles.Cast<string>().Reverse().Take(1));
        }

        private bool ConfirmIfUnsaved(string message)
        {
            if (!ViewModel.HasAnyUnsavedChanges)
                return true;
            return MessageBox.Show(LocalizationManager.GetText("Unsaved_Changes_Detail.2", args: new Object[] { message }), LocalizationManager.GetText("Unsaved_Changes"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes;
        }

        private void NbtTree_SelectionChanged(object sender, EventArgs e)
        {
            if (InvokeRequired) // only run on UI thread
                return;
            var obj = NbtTree.SelectedINode;
            var objs = NbtTree.SelectedINodes;
            var nbt = obj.GetNbtTag();
            var container = nbt as NbtContainerTag;
            var region = obj.Get<RegionFile>();
            foreach (var item in CreateTagButtons)
            {
                item.Value.Enabled = container is not null && container.CanAdd(item.Key);
                item.Value.Visible = region is null;
            }
            ActionSort.Enabled = obj is not null && obj.CanSort;
            ActionCut.Enabled = obj is not null && objs.Any(x => x.CanCut);
            ActionCopy.Enabled = obj is not null && objs.Any(x => x.CanCopy);
            ActionPaste.Enabled = obj is not null && obj.CanPaste; // don't check for Clipboard.ContainsText() because listening for clipboard events (to re-enable) is ugly
            ActionDelete.Enabled = obj is not null && objs.Any(x => x.CanDelete);
            ActionRename.Enabled = obj is not null && (objs.Any(x => x.CanRename) || objs.Any(x => x.CanEdit));
            ActionEdit.Enabled = obj is not null && (objs.Any(x => x.CanRename) || objs.Any(x => x.CanEdit));
            ActionEditSnbt.Enabled = nbt is not null;
            ActionAddSnbt.Enabled = container is not null;

            ActionAddSnbt.Visible = region is null;
            ActionAddChunk.Visible = region is not null;
        }

        private void ViewModel_Changed(object sender, EventArgs e)
        {
            if (InvokeRequired) // only run on UI thread
                return;
            ActionSave.Enabled = ViewModel.HasAnyUnsavedChanges;
            ActionSaveAs.Enabled = ViewModel.OpenedFiles.Any();
            ActionRefresh.Enabled = ViewModel.OpenedFiles.Any();
            bool multiple_files = ViewModel.OpenedFiles.Skip(1).Any();
            var save_image = multiple_files ? IconType.SaveAll : IconType.Save;
            ActionSave.IconType = save_image;
            ActionSaveAs.IconType = save_image;
            ActionUndo.Enabled = UndoHistory.CanUndo;
            ActionRedo.Enabled = UndoHistory.CanRedo;
            NbtTree_SelectionChanged(sender, e);
        }

        private void NbtTree_NodeMouseDoubleClick(object sender, TreeNodeAdvMouseEventArgs e)
        {
            var tag = NbtTree.INodeFromClick(e);
            if (!e.Node.CanExpand && tag.CanEdit)
                Edit(tag);
        }

        private void NbtTree_ItemDrag(object sender, ItemDragEventArgs e)
        {
            DoDragDrop(NbtTree.SelectedNodes.ToArray(), DragDropEffects.Move);
        }

        private void NbtTree_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = Control.ModifierKeys == Keys.Shift ? DragDropEffects.Copy : DragDropEffects.Move;
            else
            {
                var tags = NbtTree.INodesFromDrag(e);
                var drop = NbtTree.DropINode;
                if (tags.Any()
                    && NbtTree.DropINode is not null
                    && CanMoveObjects(tags, drop, NbtTree.DropPosition.Position))
                    e.Effect = e.AllowedEffect;
                else
                    e.Effect = DragDropEffects.None;
            }
        }

        private void NbtTree_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (e.Effect == DragDropEffects.Move)
                    OpenFiles(files);
                else if (e.Effect == DragDropEffects.Copy)
                    ImportFiles(files);
            }
            else
            {
                var tags = NbtTree.INodesFromDrag(e);
                var drop = NbtTree.DropINode;
                if (tags.Any())
                    MoveObjects(tags, drop, NbtTree.DropPosition.Position);
            }
        }

        private bool CanMoveObjects(IEnumerable<INode> nodes, INode target, NodePosition position)
        {
            var (destination, index) = ViewModel.GetInsertionLocation(target, position);
            if (destination is null) return false;
            return destination.CanReceiveDrop(nodes);
        }

        private void MoveObjects(IEnumerable<INode> nodes, INode target, NodePosition position)
        {
            var (destination, index) = ViewModel.GetInsertionLocation(target, position);
            if (destination is null) return;
            UndoHistory.StartBatchOperation();
            destination.ReceiveDrop(nodes, index);
            UndoHistory.FinishBatchOperation(new DescriptionHolder("Move {0} into {1} at position {2}", nodes, destination, index), true);
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
#if !DEBUG
            if (!ConfirmIfUnsaved(LocalizationManager.GetText("Exit_Anyway")))
                e.Cancel = true;
#endif
        }

        private void SetAllSelected(IEnumerable<TreeNodeAdv> nodes, bool selected)
        {
            foreach (var node in nodes)
            {
                node.IsSelected = selected;
            }
        }

        private ContextMenuStrip CreateContextMenu(TreeNodeAdvMouseEventArgs e)
        {
            var menu = new ContextMenuStrip();
            var obj = NbtTree.INodeFromClick(e);
            var root_items = new List<ToolStripItem>();
            var node_items = new List<ToolStripItem>();
            var file_items = new List<ToolStripItem>();
            var nbt_items = new List<ToolStripItem>();
            if (obj.Parent is ModelRootNode)
                root_items.Add(new ToolStripMenuItem(LocalizationManager.GetText("Delete"), IconSource.GetImage(IconType.Delete).Image, Discard_Click));
            if (e.Node.CanExpand)
            {
                if (e.Node.IsExpanded)
                    node_items.Add(new ToolStripMenuItem(LocalizationManager.GetText("Collapse"), null, Collapse_Click));
                else
                    node_items.Add(new ToolStripMenuItem(LocalizationManager.GetText("Expand_All"), null, ExpandAll_Click));
                var children = NbtTree.AllChildren(e.Node);
                if (children.All(x => x.IsSelected))
                    node_items.Add(new ToolStripMenuItem(LocalizationManager.GetText("Deselect_all_Children"), null, DeselectChildren_Click));
                else
                    node_items.Add(new ToolStripMenuItem(LocalizationManager.GetText("Select_all_Children"), null, SelectChildren_Click));
            }
            var saveable = obj.Get<ISaveable>();
            if (saveable is not null && saveable.CanSave)
                file_items.Add(new ToolStripMenuItem(LocalizationManager.GetText("Save_File"), IconSource.GetImage(IconType.Save).Image, Save_Click));
            if (obj.Get<IExportable>() is not null)
                file_items.Add(new ToolStripMenuItem(LocalizationManager.GetText("Save_File_As"), IconSource.GetImage(IconType.Save).Image, SaveAs_Click));
            var refresh = obj.Get<IRefreshable>();
            if (refresh is not null && refresh.CanRefresh)
                file_items.Add(new ToolStripMenuItem(LocalizationManager.GetText("Refresh"), IconSource.GetImage(IconType.Refresh).Image, Refresh_Click));
            var path = obj.Get<IHavePath>();
            if (path is not null && path.Path is not null)
                file_items.Add(new ToolStripMenuItem(LocalizationManager.GetText("Open_in_Explorer"), IconSource.GetImage(IconType.OpenFile).Image, OpenInExplorer_Click));
            var container = obj.GetNbtTag() as NbtContainerTag;
            if (container is not null)
            {
                var addable = NbtUtil.NormalTagTypes().Where(x => container.CanAdd(x));
                bool single = ListUtils.ExactlyOne(addable);
                var display = single ? (Func<NbtTagType, string>)(x => LocalizationManager.GetText("Add_Tag.1", args: new Object[] { NbtUtil.TagTypeName(x) })) : (x => LocalizationManager.GetText("Add_Tag.2", args: new Object[] { NbtUtil.TagTypeName(x) }));
                var items = addable.Select(x => new ToolStripMenuItem(display(x), NbtUtil.TagTypeImage(IconSource, x).Image, (s, ea) => AddTag_Click(x))).ToArray();
                if (single)
                    nbt_items.AddRange(items);
                else
                {
                    var add = new ToolStripMenuItem(LocalizationManager.GetText("Add"));
                    add.DropDownItems.AddRange(items);
                    nbt_items.Add(add);
                }
            }
            AddMenuSections(menu.Items, root_items, node_items, file_items, nbt_items);
            return menu;
        }

        private void AddMenuSections(ToolStripItemCollection collection, params IEnumerable<ToolStripItem>[] sources)
        {
            for (int i = 0; i < sources.Length - 1; i++)
            {
                collection.AddRange(sources[i].ToArray());
                if (sources[i].Any())
                    collection.Add(new ToolStripSeparator());
            }
            collection.AddRange(sources[sources.Length - 1].ToArray());
        }

        private void Discard_Click(object sender, EventArgs e)
        {
            var selected_roots = NbtTree.SelectedINodes.Where(x => x.Parent is ModelRootNode);
            Discard(selected_roots);
        }

        private void Collapse_Click(object sender, EventArgs e)
        {
            var selected = NbtTree.SelectedNodes;
            foreach (var node in selected)
            {
                node.CollapseAll();
            }
        }

        private void ExpandAll_Click(object sender, EventArgs e)
        {
            var selected = NbtTree.SelectedNodes;
            foreach (var node in selected)
            {
                node.ExpandAll();
            }
        }

        private void SelectChildren_Click(object sender, EventArgs e)
        {
            var selected = NbtTree.SelectedNodes.ToList();
            foreach (var node in selected)
            {
                SetAllSelected(NbtTree.AllChildren(node), true);
            }
        }

        private void DeselectChildren_Click(object sender, EventArgs e)
        {
            var selected = NbtTree.SelectedNodes.ToList();
            foreach (var node in selected)
            {
                SetAllSelected(NbtTree.AllChildren(node), false);
            }
        }

        private void Save_Click(object sender, EventArgs e)
        {
            var selected = NbtTree.SelectedINodes.Filter(x => x.Get<ISaveable>());
            foreach (var item in selected)
            {
                Save(item);
            }
        }

        private void SaveAs_Click(object sender, EventArgs e)
        {
            var selected = NbtTree.SelectedINodes.Filter(x => x.Get<IExportable>());
            foreach (var item in selected)
            {
                SaveAs(item);
            }
        }

        private void OpenInExplorer_Click(object sender, EventArgs e)
        {
            var selected = NbtTree.SelectedINodes.Filter(x => x.Get<IHavePath>());
            foreach (var item in selected)
            {
                OpenInExplorer(item);
            }
        }

        private void Refresh_Click(object sender, EventArgs e)
        {
            var selected = NbtTree.SelectedINodes.Filter(x => x.Get<IRefreshable>());
            RefreshItems(selected);
        }

        private void AddTag_Click(NbtTagType type)
        {
            var selected = NbtTree.SelectedINodes.Filter(x => x.GetNbtTag()).OfType<NbtContainerTag>();
            foreach (var item in selected)
            {
                AddTag(item, type);
            }
        }

        private void NbtTree_NodeMouseClick(object sender, TreeNodeAdvMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var menu = CreateContextMenu(e);
                menu.Show(NbtTree, e.Location);
                e.Handled = true;
            }
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Properties.Settings.Default.TreeZoom = (int)NbtTree.Font.Size;
            var icon_sets = Properties.Settings.Default.CustomIconSets.Cast<string>().Distinct().ToArray();
            Properties.Settings.Default.CustomIconSets.Clear();
            Properties.Settings.Default.CustomIconSets.AddRange(icon_sets);
            Properties.Settings.Default.Save();
        }

        private void MenuEdit_DropDownOpening(object sender, EventArgs e)
        {
            var undo_history = UndoHistory.GetUndoHistory();
            var redo_history = UndoHistory.GetRedoHistory();

            var undo_dropdown = new ToolStripDropDown();
            DropDownUndoHistory.DropDown = undo_dropdown;
            var undo_actions = new ActionHistory(undo_history,
                x => { UndoHistory.Undo(x + 1); MenuEdit.HideDropDown(); },
                x => $"Undo {StringUtils.Pluralize(x + 1, "action")}",
                DropDownUndoHistory.Font);
            undo_dropdown.Items.Add(new ToolStripControlHost(undo_actions));

            var redo_dropdown = new ToolStripDropDown();
            DropDownRedoHistory.DropDown = redo_dropdown;
            var redo_actions = new ActionHistory(redo_history,
                x => { UndoHistory.Redo(x + 1); MenuEdit.HideDropDown(); },
                x => $"Redo {StringUtils.Pluralize(x + 1, "action")}",
                DropDownRedoHistory.Font);
            redo_dropdown.Items.Add(new ToolStripControlHost(redo_actions));

            DropDownUndoHistory.Enabled = undo_history.Any();
            DropDownRedoHistory.Enabled = redo_history.Any();
            ActionClearUndoHistory.Enabled = undo_history.Any() || redo_history.Any();
        }

        private void UpdateRecentFiles()
        {
            // remove duplicates of recent files and limit to 20 most recent
            var distinct = Properties.Settings.Default.RecentFiles.Cast<string>().Reverse().Distinct();
            var recents = distinct.Take(20).ToList();

            DropDownRecent.Enabled = recents.Count > 0;
            DropDownRecent.DropDownItems.Clear();
            var items = new List<ToolStripMenuItem>();
            foreach (string path in recents.ToList())
            {
                var item = RecentEntry(path);
                if (item is null)
                    recents.Remove(path);
                else
                    items.Add(item);
            }
            DropDownRecent.DropDownItems.AddRange(items.ToArray());

            Properties.Settings.Default.RecentFiles.Clear();
            Properties.Settings.Default.RecentFiles.AddRange(recents.AsEnumerable().Reverse().ToArray());
        }

        private void MenuFile_DropDownOpening(object sender, EventArgs e)
        {
            ActionNewClipboard.Enabled = Clipboard.ContainsFileDropList() || Clipboard.ContainsText();
            UpdateRecentFiles();
        }

        private ToolStripMenuItem RecentEntry(string path)
        {
            bool directory = Directory.Exists(path);
            Image image;
            EventHandler click;
            if (directory)
            {
                image = IconSource.GetImage(IconType.Folder).Image;
                click = (s, e) => OpenFolder(path);
            }
            else
            {
                if (!File.Exists(path))
                    return null;
                image = IconSource.GetImage(IconType.File).Image;
                click = (s, e) => OpenFiles(new[] { path });
            }
            return new ToolStripMenuItem(path, image, click);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Enter)
            {
                Edit();
                return true;
            }
            if (keyData == (Keys.Control | Keys.Shift | Keys.T))
            {
                OpenRecentFile();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
