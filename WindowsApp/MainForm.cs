using PaperPlane2.Actions;
using PaperPlane2.Models;

namespace PaperPlane2
{
    public partial class MainForm : Form
    {
        private List<DocumentAction> UndoStacks;
        private List<DocumentAction> RedoStacks;

        public Workspace Workspace; 

        public MainForm(string? openFileName)
        {


            UndoStacks = new List<DocumentAction>();
            RedoStacks = new List<DocumentAction>();

            string tempPath = Path.Combine(Path.GetTempPath(), "PaperPlane", Guid.NewGuid().ToString("N"));
            Workspace = new Workspace(tempPath);

            InitializeComponent();

            if (openFileName != null) {
                ExecuteAction(new OpenDocumentAction(openFileName));
            }
        }

        public void ExecuteAction(DocumentAction action) {
            action.Execute(this);
            UndoStacks.Add(action);
            RedoStacks.Clear();

            undoToolStripMenuItem.Enabled = true;
            redoToolStripMenuItem.Enabled = false;
        }

        public bool Undo() {
            var lastIndex = UndoStacks.Count - 1;
            if (lastIndex < 0) {
                return false;
            }

            var lastAction = UndoStacks[lastIndex];
            UndoStacks.RemoveAt(lastIndex);
            if (lastIndex <= 0) {
                undoToolStripMenuItem.Enabled = false;
            }

            lastAction.Undo(this);
            RedoStacks.Add(lastAction);
            redoToolStripMenuItem.Enabled = true;

            return true;
        }

        public bool Redo() {
            var lastIndex = RedoStacks.Count - 1;
            if (lastIndex < 0) {
                return false;
            }

            var lastAction = RedoStacks[lastIndex];
            RedoStacks.RemoveAt(lastIndex);
            if (lastIndex <= 0) {
                redoToolStripMenuItem.Enabled = false;
            }

            lastAction.Redo(this);

            UndoStacks.Add(lastAction);
            undoToolStripMenuItem.Enabled = true;
            return true;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            splitContainer1.SplitterDistance = Properties.Settings.Default.SplitterDistance;

        }



        private int reloadThumbnailsVersion = 0;
        public void ReloadDocumentThumbnails() {

            reloadThumbnailsVersion += 1;
            if (backgroundWorker1.IsBusy) {
                return;
            }
            backgroundWorker1.RunWorkerAsync();
        }

        private const int MAX_THUMBNAIL_SIZE = 256;
        private const int MIN_THUMBNAIL_SIZE = 64;
        private const int FAST_THUMBNAIL_SIZE = 32;
        private void ReloadThumbnailsInBackground() {

            int thumbnailSize = (splitContainer1.SplitterDistance - 20) * 8 / 10;
            if (thumbnailSize > MAX_THUMBNAIL_SIZE) {
                thumbnailSize = MAX_THUMBNAIL_SIZE;
            }
            if (thumbnailSize < MIN_THUMBNAIL_SIZE) {
                thumbnailSize = MIN_THUMBNAIL_SIZE;
            }

            var pages = Workspace.ListDocumentPages();
            if (pages == null) {
                this.BeginInvoke(delegate () {
                    listView1.Clear();
                });
                return;
            }

            var smallImageList = new ImageList();
            smallImageList.ImageSize = new Size(thumbnailSize, thumbnailSize);

            int fullScore = pages.Count * 2 + 5;
            int score = 0;

            bool hasFastThumbnail = false;
            foreach (var page in pages) {
                var image = Workspace.GetCachePageThumbnailImage(page, thumbnailSize);
                if (image == null) {
                    image = Workspace.GetPageThumbnailImage(page, FAST_THUMBNAIL_SIZE);
                    hasFastThumbnail = true;
                }
                smallImageList.Images.Add(page.FileName, image);
                score += 1;
                backgroundWorker1.ReportProgress(score * 100 / fullScore);
            }


            this.BeginInvoke(delegate () {
                SetImageList(smallImageList, pages);
            });

            if (!hasFastThumbnail) {
                backgroundWorker1.ReportProgress(100);
                return;
            }

            var largeImageList = new ImageList();
            largeImageList.ImageSize = new Size(thumbnailSize, thumbnailSize);

            foreach (var page in pages) {
                var image = Workspace.GetPageThumbnailImage(page, thumbnailSize);
                largeImageList.Images.Add(page.FileName, image);

                score += 1;
                backgroundWorker1.ReportProgress(score * 100 / fullScore);
            }

            this.BeginInvoke(delegate () {
                SetImageList(largeImageList, pages);
            });

            backgroundWorker1.ReportProgress(100);

        }
        private void backgroundWorker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e) {
            while (true) {
                int startVersion = reloadThumbnailsVersion;
                ReloadThumbnailsInBackground();
                if (startVersion != reloadThumbnailsVersion) {
                    continue;
                }
                break;
            }
        }
        private void SetImageList(ImageList imageList, List<PDFDocument> pages) {

            var offset = listView1.AutoScrollOffset;
            var selectedPages = ListSelectedPages();

            this.isReloadingListView += 1;
            listView1.BeginUpdate();

            ListViewItem? firstSelectedItemView = null;
            try {

                listView1.Clear();
                listView1.LargeImageList = imageList;

                int pageIndex = 0;
                foreach (var page in pages) {

                    var itemView = new ListViewItem();
                    itemView.Text = String.Format("{0}", pageIndex + 1);
                    itemView.ImageKey = page.FileName;
                    listView1.Items.Add(itemView);

                    if (selectedPages.Contains(page)) {
                        itemView.Selected = true;
                        if (firstSelectedItemView == null) {
                            firstSelectedItemView = itemView;
                        }
                    }
                    pageIndex += 1;
                }

            }
            finally {
                listView1.EndUpdate();
                listView1.AutoScrollOffset = offset;
                this.isReloadingListView -= 1;

                if (firstSelectedItemView != null) {
                    firstSelectedItemView.EnsureVisible();
                }
            }


        }
        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Undo();

        }

        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Redo();
        }

        private int isReloadingListView = 0;
        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (isReloadingListView > 0) {
                return;
            }

            ReloadHasSelectedPages();
            ReloadPagePreview();
        }

        private bool ShowPagePreview(string? fileName) {
            if (string.IsNullOrEmpty(fileName)) {
                webView1.Visible = false;
                return true;
            }

            try {
                webView1.Visible = true;
                var url = new Uri(string.Format("file://{0}", fileName));
                System.Diagnostics.Debug.WriteLine("Load:{0}", fileName);
                if (webView1.Source != url) {
                    webView1.Source = url;
                }
                else {
                    webView1.Source = url;
                    webView1.Reload();
                }
                return true;
            }
            catch{ 
            }

            return false;
        }

        public void ReloadPagePreview() {


            var firstPage = GetFirstSelectedPage();
            if (firstPage != null) {
                if (ShowPagePreview(firstPage.FileName)) {
                    return;
                }
            }

            ShowPagePreview(null);
        }

        public void ReloadHasSelectedPages() {

            bool hasSelected = listView1.SelectedItems.Count > 0;

            rotateLeftToolStripMenuItem.Enabled = hasSelected;
            rotateRightToolStripMenuItem.Enabled = hasSelected;

            cutToolStripMenuItem.Enabled = hasSelected;
            copyToolStripMenuItem.Enabled = hasSelected;
            deleteToolStripMenuItem.Enabled = hasSelected;
            invertSelectionToolStripMenuItem.Enabled = hasSelected;
            exportSelectedPagesToolStripMenuItem.Enabled = hasSelected;
            pastToolStripMenuItem.Enabled = hasSelected;

        }

        public void ReloadHasDocument()
        {
            bool hasDocument = Workspace.Document != null;

            saveToolStripMenuItem.Enabled = hasDocument;
            saveAsToolStripMenuItem.Enabled = hasDocument;
            closeDocumentToolStripMenuItem.Enabled = hasDocument;
            selectAllToolStripMenuItem.Enabled = hasDocument;

            listView1.Visible = hasDocument;
            webView1.Visible = hasDocument;
            splitContainer1.Visible = hasDocument;
            emptyPanel.Visible = !hasDocument;
        }
        
        public List<PDFDocument> ListSelectedPages() {
            var pages = new List<PDFDocument>();

            foreach(ListViewItem item in listView1.SelectedItems) {
                var page = Workspace.GetDocumentPage(item.Index);
                if (page != null) {
                    pages.Add(page);
                }
            }
            return pages;
        }

        public PDFDocument? GetFirstSelectedPage()
        {
            foreach (ListViewItem item in listView1.SelectedItems) {
                var page = Workspace.GetDocumentPage(item.Index);
                if (page != null) {
                    return page;
                }
            }
            return null;
        }


        public int GetLastSelectedPageIndex()
        {
            int lastIndex = -1;
            foreach (ListViewItem item in listView1.SelectedItems) {
                if (lastIndex < item.Index) {
                    lastIndex = item.Index;
                }
            }
            return lastIndex;
        }



        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var pages = ListSelectedPages();
            if (pages != null) {
                ExecuteAction(new DeleteDocumentPageAction(pages));
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var document = Workspace.Document;
            if (document != null) {
                Workspace.SaveAs(document.FileName);
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.AddExtension = true;

            var doc = Workspace.Document;
            if (doc != null) {
                saveFileDialog.InitialDirectory = Path.GetDirectoryName(doc.FileName);
                saveFileDialog.FileName = Path.GetFileName(doc.FileName);
            }
            saveFileDialog.Filter = "Adobe PDF Files|*.pdf";
            saveFileDialog.Title = "Save As";


            var result = saveFileDialog.ShowDialog();

            if (result != DialogResult.OK) {
                return;
            }

            if (string.IsNullOrEmpty(saveFileDialog.FileName)) {
                return;
            }

            Workspace.SaveAs(saveFileDialog.FileName);
            ExecuteAction(new OpenDocumentAction(saveFileDialog.FileName));
        }

        private void BrowseDocumentToOpen() {

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Adobe PDF Files|*.pdf";
            openFileDialog.Title = "Open";

            var result = openFileDialog.ShowDialog();
            if (result != DialogResult.OK)
            {
                return;
            }

            if (string.IsNullOrEmpty(openFileDialog.FileName))
            {
                return;
            }

            ExecuteAction(new OpenDocumentAction(openFileDialog.FileName));
        }
        private void openButtonInEmptyPanel_Click(object sender, EventArgs e)
        {
            BrowseDocumentToOpen();
        }


        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BrowseDocumentToOpen();

        }

        private void rotateLeftToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExecuteAction(new RotateDocumentPageAction(-90));
        }

        private void rotateRightToolStripMenuItem_Click(object sender, EventArgs e)
        {

            ExecuteAction(new RotateDocumentPageAction(90));
        }

        private void closeDocumentToolStripMenuItem_Click(object sender, EventArgs e)
        {

            ExecuteAction(new OpenDocumentAction(null));

            ReloadHasDocument();
        }

        private static string MOVE_SELECTED_PAGES = "MoveSelectedPages";


        private LazyMergeDocument? GetLazyMergeDocument(string suffix)
        {
            var doc = Workspace.Document;
            if (doc == null) {
                return null;
            }

            var pages = ListSelectedPages();
            if (pages == null) {
                return null;
            }

            var files = new List<string>();
            foreach (var page in pages) {
                files.Add(page.FileName);
            }

            return new LazyMergeDocument(MOVE_SELECTED_PAGES, suffix, doc.FileName, files.ToArray());
        }

        private void listView1_ItemDrag(object sender, ItemDragEventArgs e)
        {
            var doc = GetLazyMergeDocument("(Drag Out)");
            if (doc == null) {
                return;
            }
            DoDragDrop(doc, DragDropEffects.Move);
        }

        private void MainForm_DragEnter(object sender, DragEventArgs e)
        {
            var data = e.Data;
            if (data == null) {
                e.Effect = DragDropEffects.None;
                return;
            }

            string text = (string)data.GetData(DataFormats.Text);
            if (text != null && text.Equals(MOVE_SELECTED_PAGES)) {
                e.Effect = DragDropEffects.Move;
                return;
            }


            string[] files = (string[])data.GetData(DataFormats.FileDrop);
            if (files != null) {
                e.Effect = DragDropEffects.Copy;
                return;
            }

            e.Effect = DragDropEffects.None;

        }

        private int GetDropTargetIndex() {
            if (Workspace.Document == null) {
                return 0;
            }

            // Retrieve the index of the insertion mark;
            int targetIndex = listView1.InsertionMark.Index;

            // If the insertion mark is not visible, exit the method.
            if (targetIndex == -1) {
                return -1;
            }

            // If the insertion mark is to the right of the item with
            // the corresponding index, increment the target index.
            if (listView1.InsertionMark.AppearsAfterItem) {
                targetIndex++;
            }

            return targetIndex;
        }

        private void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            var data = e.Data;
            if (data == null) {
                return;
            }


            string text = (string)data.GetData(DataFormats.Text);
            if (text != null && text.Equals(MOVE_SELECTED_PAGES)) {
                int targetIndex = GetDropTargetIndex();
                if (targetIndex < 0) {
                    return;
                }

                var pages = ListSelectedPages();
                if (pages != null) {
                    ExecuteAction(new MoveDocumentPagesAction(pages, targetIndex));
                }

                return;
            }


            string[] files = (string[])data.GetData(DataFormats.FileDrop);
            if (files != null) {
                int targetIndex = GetDropTargetIndex();
                if (targetIndex < 0) {
                    return;
                }

                ExecuteAction(new InsertDocumentsAction(files, targetIndex));
                return;
            }



        }

        private void MainForm_DragOver(object sender, DragEventArgs e)
        {

            // Retrieve the client coordinates of the mouse pointer.
            Point targetPoint =
                listView1.PointToClient(new Point(e.X, e.Y));

            // Retrieve the index of the item closest to the mouse pointer.
            int targetIndex = listView1.InsertionMark.NearestIndex(targetPoint);

            // Confirm that the mouse pointer is not over the dragged item.
            if (targetIndex > -1)
            {
                // Determine whether the mouse pointer is to the left or
                // the right of the midpoint of the closest item and set
                // the InsertionMark.AppearsAfterItem property accordingly.
                Rectangle itemBounds = listView1.GetItemRect(targetIndex);
                if (targetPoint.X > itemBounds.Left + (itemBounds.Width / 2))
                {
                    listView1.InsertionMark.AppearsAfterItem = true;
                }
                else
                {
                    listView1.InsertionMark.AppearsAfterItem = false;
                }
            }

            // Set the location of the insertion mark. If the mouse is
            // over the dragged item, the targetIndex value is -1 and
            // the insertion mark disappears.
            listView1.InsertionMark.Index = targetIndex;
        }

        private void MainForm_DragLeave(object sender, EventArgs e)
        {

            listView1.InsertionMark.Index = -1;
        }

        public void DeselectAll() {
            foreach (ListViewItem item in listView1.Items) {
                item.Selected = false;
            }
        }
        public void SelectAll() {
            foreach (ListViewItem item in listView1.Items) {
                item.Selected = true;
            }
        }

        public void SelectInvert() {
            foreach (ListViewItem item in listView1.Items) {
                item.Selected = !item.Selected;
            }
        }

        public void SelectPage(int index) {
            try {
                listView1.Items[index].Selected = true;
            }
            catch (ArgumentOutOfRangeException){ 
                
            }
        }

        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e) {
            SelectAll();
        }

        private void invertSelectionToolStripMenuItem_Click(object sender, EventArgs e) {
            SelectInvert();
        }

        private void exportSelectedPagesToolStripMenuItem_Click(object sender, EventArgs e)
        {

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.AddExtension = true;

            var doc = Workspace.Document;
            if (doc != null) {
                saveFileDialog.InitialDirectory = Path.GetDirectoryName(doc.FileName);
                saveFileDialog.FileName = Path.GetFileName(doc.FileName);
            }
            saveFileDialog.Filter = "Adobe PDF Files|*.pdf";
            saveFileDialog.Title = "Export Selected Pages";


            var result = saveFileDialog.ShowDialog();

            if (result != DialogResult.OK) {
                return;
            }

            if (string.IsNullOrEmpty(saveFileDialog.FileName)) {
                return;
            }

            Workspace.SaveAs(saveFileDialog.FileName, ListSelectedPages());
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var doc = GetLazyMergeDocument("(Copy)");
            if (doc == null) {
                return;
            }
            Clipboard.SetDataObject(doc);
        }

        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var doc = GetLazyMergeDocument("(Cut)");
            if (doc == null)
            {
                return;
            }
            Clipboard.SetDataObject(doc);


            var pages = ListSelectedPages();
            if (pages != null)
            {
                ExecuteAction(new DeleteDocumentPageAction(pages));
            }
        }




        private void pastToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var dataObject = Clipboard.GetDataObject();
            var files = (string[])dataObject.GetData(DataFormats.FileDrop);

            if (files != null) {

                var pageIndex = GetLastSelectedPageIndex() + 1;

                ExecuteAction(new InsertDocumentsAction(files, pageIndex));
            }


        }


        private bool isResizing = false;
        private void MainForm_Resize(object sender, EventArgs e)
        {
            isResizing = true;
            emptyContainer.Top = (emptyPanel.Height - emptyContainer.Height) / 2;
            emptyContainer.Left = (emptyPanel.Width - emptyContainer.Width) / 2;

            splitContainer1.SplitterDistance = Properties.Settings.Default.SplitterDistance;

        }
        private void MainForm_ResizeEnd(object sender, EventArgs e)
        {
            isResizing = false;
            ReloadDocumentThumbnails();

        }

        private void backgroundWorker1_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {

            if (e.ProgressPercentage >= 100) {
                progressBar1.Visible = false;
                progressBar1.Value = 100;
            }
            else {
                progressBar1.Visible = true;
                progressBar1.Value = e.ProgressPercentage;
            }
        }


        public const int MAX_SAVE_SPLITTER_DISTANCE = 256;
        private void splitContainer1_SplitterMoved(object sender, SplitterEventArgs e)
        {
            ReloadDocumentThumbnails();

            if (isResizing) {
                return;
            }

            var d = splitContainer1.SplitterDistance;
            if (d > MAX_SAVE_SPLITTER_DISTANCE) {
                d = MAX_SAVE_SPLITTER_DISTANCE;
            }
            Properties.Settings.Default.SplitterDistance = d;
            Properties.Settings.Default.Save();

        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (Workspace != null) {
                try {
                    Workspace.DeleteRootDirectory();
                }
                catch { 
                }
            }
        }
    }
}