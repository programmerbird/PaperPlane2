using PaperPlane2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaperPlane2.Actions
{
    public class DeleteDocumentPageAction : DocumentAction
    {

        private List<PDFDocument> pagesToDelete;
        private List<PDFDocument>? pagesBeforeDelete;

        public DeleteDocumentPageAction(List<PDFDocument> pages) {
            this.pagesToDelete = pages;
        }

        public override void Init(MainForm form)
        {
            pagesBeforeDelete = new List<PDFDocument>();

            var pages = form.Workspace.ListDocumentPages();
            if (pages == null) {
                return;
            }
            foreach (var page in pages) {
                pagesBeforeDelete.Add(page); 
            }
        }
        public override void Redo(MainForm form)
        {
            var pages = form.Workspace.ListDocumentPages();
            if (pages == null) {
                return;
            }

            foreach (var page in pagesToDelete) {
                pages.Remove(page);
            }

            form.DeselectAll();
            form.ReloadPagePreview();
            form.ReloadDocumentThumbnails();
        }

        public override void Undo(MainForm form)
        {
            var pages = form.Workspace.ListDocumentPages();
            if (pages == null) {
                return;
            }

            pages.Clear();
            form.DeselectAll();

            if (pagesBeforeDelete != null) {
                int index = 0;
                foreach (var page in pagesBeforeDelete) {
                    if (pagesToDelete.Contains(page)) {
                        form.SelectPage(index);
                    }
                    pages.Add(page);
                    index += 1;
                }
            }

            form.ReloadPagePreview();
            form.ReloadDocumentThumbnails();

        }
    }
}
