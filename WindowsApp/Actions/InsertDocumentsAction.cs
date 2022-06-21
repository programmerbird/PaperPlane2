using PaperPlane2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaperPlane2.Actions
{
    public class InsertDocumentsAction : DocumentAction
    {
        private string[] fileNames;
        private int afterPageIndex;
        private int totalInsertPages;
        private PDFDocument? lastDocument;

        public InsertDocumentsAction(string[] fileNames, int pageIndex) {
            this.fileNames = fileNames;
            this.afterPageIndex = pageIndex;
        }

        public override void Init(MainForm form)
        {
            lastDocument = form.Workspace.Document;
        }

        public override void Redo(MainForm form)
        {

            List<PDFDocument>? mainPages = null;

            int counter = 0;
            foreach (string fileName in fileNames) {
                if (!fileName.ToLower().EndsWith(".pdf")) {
                    continue;
                }

                var document = new PDFDocument(fileName);
                if (form.Workspace.Document == null) {
                    form.Workspace.OpenDocument(document);
                    continue;
                }

                var pages = form.Workspace.ListDocumentPages(document);
                if (pages == null) {
                    continue;
                }

                if (mainPages == null) {
                    mainPages = form.Workspace.ListDocumentPages();
                }
                if (mainPages == null) {
                    continue;
                }

                foreach (var page in pages) {
                    mainPages.Insert(afterPageIndex + counter, page);
                    counter += 1;
                }
            }

            totalInsertPages = counter;

            form.DeselectAll();
            int lastIndex = afterPageIndex + totalInsertPages - 1;
            for (var i = lastIndex; i >= afterPageIndex; i--) {
                form.SelectPage(i);
            }

            form.ReloadHasDocument();
            form.ReloadPagePreview();
            form.ReloadDocumentThumbnails();
        }

        public override void Undo(MainForm form)
        {

            if (lastDocument == null) {
                form.Workspace.OpenDocument(lastDocument);
            }
            else { 
                var pages = form.Workspace.ListDocumentPages();
                if (pages != null) {
                    int lastIndex = afterPageIndex + totalInsertPages - 1;
                    for (var i = lastIndex; i >= afterPageIndex; i--) {
                        pages.RemoveAt(i);
                    }
                }
            }

            form.ReloadHasDocument();
            form.ReloadPagePreview();
            form.ReloadDocumentThumbnails();

        }



    }
}
