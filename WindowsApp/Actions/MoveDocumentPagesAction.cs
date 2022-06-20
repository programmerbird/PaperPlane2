using PaperPlane2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaperPlane2.Actions
{
    public class MoveDocumentPagesAction : DocumentAction
    {

        private List<PDFDocument> pagesToMove;

        private List<PDFDocument>? pagesBeforeMove;

        private int afterPageIndex;

        public MoveDocumentPagesAction(List<PDFDocument> pagesToMove, int pageIndex) {
            this.pagesToMove = pagesToMove;
            this.afterPageIndex = pageIndex;
        }

        public override void Init(MainForm form)
        {
            var pages = form.Workspace.ListDocumentPages();
            if (pages == null) {
                return;
            }

            var r = new List<PDFDocument>();
            foreach (var page in pages) {
                r.Add(page);
            }
            pagesBeforeMove = r;
        }

        public override void Redo(MainForm form) {
            var pages = form.Workspace.ListDocumentPages();
            if (pages == null) {
                return;
            }


            int counter = 0;
            foreach (var page in pagesToMove) {
                pages.Insert(afterPageIndex + counter, page);
                counter += 1;
            }

            int totalPages = pages.Count;
            for (int i = totalPages - 1; i >= afterPageIndex + counter; i--) {
                var page = pages[i];
                if (pagesToMove.Contains(page)) {
                    pages.RemoveAt(i);
                }
            }

            for (int i = afterPageIndex - 1; i >= 0; i--) {
                var page = pages[i];
                if (pagesToMove.Contains(page)) {
                    pages.RemoveAt(i);
                }
            }

            form.DeselectAll();

            counter = 0;
            foreach (var page in pages) {
                if (pagesToMove.Contains(page)) {
                    form.SelectPage(counter);
                }
                counter += 1;
            }


            form.ReloadDocumentThumbnails();
        }

        public override void Undo(MainForm form)
        {
            var pages = form.Workspace.ListDocumentPages();
            if (pages == null) {
                return;
            }

            pages.Clear();

            if (pagesBeforeMove != null)
            {
                foreach (var page in pagesBeforeMove)
                {
                    pages.Add(page);
                }
            }

            form.DeselectAll();

            if (pagesBeforeMove != null) {
                int index = 0;
                foreach (var page in pagesBeforeMove) {
                    if (pagesToMove.Contains(page)) {
                        form.SelectPage(index);
                    }
                    index += 1;
                }
            }

            form.ReloadDocumentThumbnails();

        }
    }
}
