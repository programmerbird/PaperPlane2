using PaperPlane2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaperPlane2.Actions
{
    public class OpenDocumentAction : DocumentAction
    {
        private PDFDocument? document;

        private PDFDocument? lastDocument;

        public OpenDocumentAction(string? fileName)
        {
            if (!String.IsNullOrEmpty(fileName)) {
                document = new PDFDocument(fileName);
            }
        }

        public override void Init(MainForm form)
        {
            if (form.Workspace.Document != null) {
                lastDocument = form.Workspace.Document;
            }
        }
        public override void Redo(MainForm form)
        {
            form.Workspace.OpenDocument(document);
            form.ReloadHasDocument();
            form.ReloadPagePreview();
            form.ReloadDocumentThumbnails();
        }

        public override void Undo(MainForm form)
        {
            form.Workspace.OpenDocument(lastDocument);
            form.ReloadHasDocument();
            form.ReloadPagePreview();
            form.ReloadDocumentThumbnails();
        }
    }
}
